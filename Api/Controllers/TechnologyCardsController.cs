using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/technology-cards")]
public sealed class TechnologyCardsController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTechnologyCards(CancellationToken cancellationToken)
    {
        var items = await dbContext.TechnologyCards
            .AsNoTracking()
            .OrderBy(x => x.Product.Name)
            .ThenByDescending(x => x.VersionNumber)
            .Select(x => new TechnologyCardListItem(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.VersionNumber,
                x.Title,
                x.Description,
                x.Status,
                x.IsActive,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<TechnologyCardListItem>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTechnologyCard(int id, CancellationToken cancellationToken)
    {
        var header = await dbContext.TechnologyCards
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TechnologyCardHeader(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.VersionNumber,
                x.Title,
                x.Description,
                x.Status,
                x.IsActive,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return NotFound(ApiResponse.Fail("Технологическая карта не найдена."));
        }

        var steps = await dbContext.TechnologySteps
            .AsNoTracking()
            .Where(x => x.TechnologyCardId == id)
            .OrderBy(x => x.StepOrder)
            .Select(x => new TechnologyStepDetail(
                x.Id,
                x.StepOrder,
                x.StepType,
                x.Title,
                x.Instruction,
                x.IsRequired,
                x.ExpectedDurationMinutes,
                x.TechnologyStepParameters
                    .OrderBy(p => p.Id)
                    .Select(p => new TechnologyStepParameterDetail(
                        p.Id,
                        p.Name,
                        p.ValueType,
                        p.Unit,
                        p.TargetNumericValue,
                        p.MinNumericValue,
                        p.MaxNumericValue,
                        p.TargetTextValue,
                        p.TargetBooleanValue,
                        p.IsRequired,
                        p.Comment))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<TechnologyCardDetail>.Ok(new TechnologyCardDetail(header, steps)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTechnologyCard([FromBody] CreateTechnologyCardRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.CreatedByUserId <= 0 || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(ApiResponse.Fail("Не заполнены обязательные поля технологической карты."));
        }

        if (request.Steps.Count == 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно добавить хотя бы один шаг технологической карты."));
        }

        var productExists = await dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);

        if (!productExists)
        {
            return BadRequest(ApiResponse.Fail("Продукт не найден."));
        }

        var userExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CreatedByUserId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            return BadRequest(ApiResponse.Fail("Пользователь-создатель не найден."));
        }

        if (request.Steps.Any(x => x.StepOrder <= 0 || string.IsNullOrWhiteSpace(x.Title)))
        {
            return BadRequest(ApiResponse.Fail("В шагах технологической карты есть пустые или некорректные данные."));
        }

        if (request.Steps.Select(x => x.StepOrder).Distinct().Count() != request.Steps.Count)
        {
            return BadRequest(ApiResponse.Fail("Порядок шагов не должен повторяться."));
        }

        foreach (var step in request.Steps)
        {
            if (step.Parameters.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                return BadRequest(ApiResponse.Fail("У параметров шага должно быть название."));
            }
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var versionNumber = (await dbContext.TechnologyCards
                .Where(x => x.ProductId == request.ProductId)
                .MaxAsync(x => (int?)x.VersionNumber, cancellationToken) ?? 0) + 1;

            var card = new TechnologyCard
            {
                ProductId = request.ProductId,
                VersionNumber = versionNumber,
                Title = request.Title.Trim(),
                Description = request.Description,
                Status = 1,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            dbContext.TechnologyCards.Add(card);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var step in request.Steps.OrderBy(x => x.StepOrder))
            {
                var technologyStep = new TechnologyStep
                {
                    TechnologyCardId = card.Id,
                    StepOrder = step.StepOrder,
                    StepType = step.StepType,
                    Title = step.Title.Trim(),
                    Instruction = step.Instruction,
                    IsRequired = step.IsRequired,
                    ExpectedDurationMinutes = step.ExpectedDurationMinutes
                };

                dbContext.TechnologySteps.Add(technologyStep);
                await dbContext.SaveChangesAsync(cancellationToken);

                foreach (var parameter in step.Parameters)
                {
                    dbContext.TechnologyStepParameters.Add(new TechnologyStepParameter
                    {
                        TechnologyStepId = technologyStep.Id,
                        Name = parameter.Name.Trim(),
                        ValueType = parameter.ValueType,
                        Unit = parameter.Unit,
                        TargetNumericValue = parameter.TargetNumericValue,
                        MinNumericValue = parameter.MinNumericValue,
                        MaxNumericValue = parameter.MaxNumericValue,
                        TargetTextValue = parameter.TargetTextValue,
                        TargetBooleanValue = parameter.TargetBooleanValue,
                        IsRequired = parameter.IsRequired,
                        Comment = parameter.Comment
                    });
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateTechnologyCardResponse>.Ok(
                new CreateTechnologyCardResponse(card.Id, request.ProductId, versionNumber),
                "Черновик технологической карты создан."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveTechnologyCard(int id, [FromBody] ApproveTechnologyCardRequest request, CancellationToken cancellationToken)
    {
        if (request.ApprovedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который утверждает карту."));
        }

        var approverExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ApprovedByUserId && x.IsActive, cancellationToken);

        if (!approverExists)
        {
            return BadRequest(ApiResponse.Fail("Пользователь, который утверждает карту, не найден."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var card = await dbContext.TechnologyCards
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (card is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(ApiResponse.Fail("Технологическая карта не найдена."));
            }

            var hasSteps = await dbContext.TechnologySteps
                .AsNoTracking()
                .AnyAsync(x => x.TechnologyCardId == id, cancellationToken);

            if (!hasSteps)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ApiResponse.Fail("Нельзя утвердить технологическую карту без шагов."));
            }

            await dbContext.TechnologyCards
                .Where(x => x.ProductId == card.ProductId && x.Id != id && x.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsActive, false), cancellationToken);

            card.Status = 3;
            card.IsActive = true;
            card.ApprovedAt = DateTime.UtcNow;
            card.ApprovedByUserId = request.ApprovedByUserId;

            dbContext.StatusHistories.Add(new StatusHistory
            {
                EntityType = "TechnologyCard",
                EntityId = id,
                PreviousStatus = "Draft",
                NewStatus = "Approved",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.ApprovedByUserId,
                Comment = request.Comment ?? "Технологическая карта утверждена через API."
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Технологическая карта утверждена."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    private static string GetDbErrorMessage(DbUpdateException exception)
    {
        return exception.InnerException?.Message ?? exception.Message;
    }
}

public sealed record TechnologyCardListItem(
    int Id,
    int ProductId,
    string ProductName,
    int VersionNumber,
    string Title,
    string? Description,
    int Status,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedByName,
    DateTime? ApprovedAt,
    string? ApprovedByName);

public sealed record TechnologyCardHeader(
    int Id,
    int ProductId,
    string ProductName,
    int VersionNumber,
    string Title,
    string? Description,
    int Status,
    bool IsActive,
    DateTime CreatedAt,
    string CreatedByName,
    DateTime? ApprovedAt,
    string? ApprovedByName);

public sealed record TechnologyStepParameterDetail(
    int Id,
    string Name,
    int ValueType,
    string? Unit,
    decimal? TargetNumericValue,
    decimal? MinNumericValue,
    decimal? MaxNumericValue,
    string? TargetTextValue,
    bool? TargetBooleanValue,
    bool IsRequired,
    string? Comment);

public sealed record TechnologyStepDetail(
    int Id,
    int StepOrder,
    int StepType,
    string Title,
    string? Instruction,
    bool IsRequired,
    int? ExpectedDurationMinutes,
    List<TechnologyStepParameterDetail> Parameters);

public sealed record TechnologyCardDetail(
    TechnologyCardHeader Header,
    List<TechnologyStepDetail> Steps);

public sealed record CreateTechnologyCardRequest(
    int ProductId,
    int CreatedByUserId,
    string Title,
    string? Description,
    List<CreateTechnologyStepRequest> Steps);

public sealed record CreateTechnologyStepRequest(
    int StepOrder,
    int StepType,
    string Title,
    string? Instruction,
    bool IsRequired,
    int? ExpectedDurationMinutes,
    List<CreateTechnologyStepParameterRequest> Parameters);

public sealed record CreateTechnologyStepParameterRequest(
    string Name,
    int ValueType,
    string? Unit,
    decimal? TargetNumericValue,
    decimal? MinNumericValue,
    decimal? MaxNumericValue,
    string? TargetTextValue,
    bool? TargetBooleanValue,
    bool IsRequired,
    string? Comment);

public sealed record CreateTechnologyCardResponse(int Id, int ProductId, int VersionNumber);

public sealed record ApproveTechnologyCardRequest(int ApprovedByUserId, string? Comment);
