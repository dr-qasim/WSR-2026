using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recipes")]
public sealed class RecipesController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRecipes(CancellationToken cancellationToken)
    {
        var items = await dbContext.RecipeVersions
            .AsNoTracking()
            .OrderBy(x => x.Product.Name)
            .ThenByDescending(x => x.VersionNumber)
            .Select(x => new RecipeListItem(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.VersionNumber,
                x.Status,
                x.IsActive,
                x.Notes,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<RecipeListItem>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRecipe(int id, CancellationToken cancellationToken)
    {
        var header = await dbContext.RecipeVersions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RecipeHeader(
                x.Id,
                x.ProductId,
                x.Product.Name,
                x.VersionNumber,
                x.Status,
                x.IsActive,
                x.Notes,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return NotFound(ApiResponse.Fail("Рецептура не найдена."));
        }

        var components = await dbContext.RecipeComponents
            .AsNoTracking()
            .Where(x => x.RecipeVersionId == id)
            .OrderBy(x => x.LoadOrder)
            .Select(x => new RecipeComponentItem(
                x.Id,
                x.RawMaterialId,
                x.RawMaterial.Name,
                x.Percentage,
                x.LoadOrder,
                x.AllowedDeviationPercent))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<RecipeDetail>.Ok(new RecipeDetail(header, components)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.CreatedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Не заполнены обязательные поля рецептуры."));
        }

        if (request.Components.Count == 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно добавить хотя бы один компонент рецептуры."));
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

        if (request.Components.Any(x => x.RawMaterialId <= 0 || x.LoadOrder <= 0 || x.Percentage <= 0 || x.AllowedDeviationPercent < 0))
        {
            return BadRequest(ApiResponse.Fail("В компонентах рецептуры есть некорректные значения."));
        }

        if (request.Components.Select(x => x.RawMaterialId).Distinct().Count() != request.Components.Count)
        {
            return BadRequest(ApiResponse.Fail("Сырье в рецептуре не должно повторяться."));
        }

        if (request.Components.Select(x => x.LoadOrder).Distinct().Count() != request.Components.Count)
        {
            return BadRequest(ApiResponse.Fail("Порядок загрузки в рецептуре не должен повторяться."));
        }

        var rawMaterialIds = request.Components.Select(x => x.RawMaterialId).Distinct().ToList();
        var rawMaterialCount = await dbContext.RawMaterials
            .AsNoTracking()
            .CountAsync(x => rawMaterialIds.Contains(x.Id), cancellationToken);

        if (rawMaterialCount != rawMaterialIds.Count)
        {
            return BadRequest(ApiResponse.Fail("Одно или несколько видов сырья не найдены."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var nextVersion = (await dbContext.RecipeVersions
                .Where(x => x.ProductId == request.ProductId)
                .MaxAsync(x => (int?)x.VersionNumber, cancellationToken) ?? 0) + 1;

            var recipe = new RecipeVersion
            {
                ProductId = request.ProductId,
                VersionNumber = nextVersion,
                Status = 1,
                IsActive = false,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            dbContext.RecipeVersions.Add(recipe);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var component in request.Components)
            {
                dbContext.RecipeComponents.Add(new RecipeComponent
                {
                    RecipeVersionId = recipe.Id,
                    RawMaterialId = component.RawMaterialId,
                    Percentage = component.Percentage,
                    LoadOrder = component.LoadOrder,
                    AllowedDeviationPercent = component.AllowedDeviationPercent
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateRecipeResponse>.Ok(
                new CreateRecipeResponse(recipe.Id, request.ProductId, nextVersion),
                "Черновик рецептуры создан."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveRecipe(int id, [FromBody] ApproveRecipeRequest request, CancellationToken cancellationToken)
    {
        if (request.ApprovedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который утверждает рецептуру."));
        }

        var approverExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ApprovedByUserId && x.IsActive, cancellationToken);

        if (!approverExists)
        {
            return BadRequest(ApiResponse.Fail("Пользователь, который утверждает рецептуру, не найден."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var recipe = await dbContext.RecipeVersions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (recipe is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(ApiResponse.Fail("Рецептура не найдена."));
            }

            var hasComponents = await dbContext.RecipeComponents
                .AsNoTracking()
                .AnyAsync(x => x.RecipeVersionId == id, cancellationToken);

            if (!hasComponents)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ApiResponse.Fail("Нельзя утвердить рецептуру без компонентов."));
            }

            await dbContext.RecipeVersions
                .Where(x => x.ProductId == recipe.ProductId && x.Id != id && x.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsActive, false), cancellationToken);

            recipe.Status = 3;
            recipe.IsActive = true;
            recipe.ApprovedAt = DateTime.UtcNow;
            recipe.ApprovedByUserId = request.ApprovedByUserId;

            dbContext.StatusHistories.Add(new StatusHistory
            {
                EntityType = "RecipeVersion",
                EntityId = id,
                PreviousStatus = "Draft",
                NewStatus = "Approved",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.ApprovedByUserId,
                Comment = request.Comment ?? "Рецептура утверждена через API."
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Рецептура утверждена."));
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

public sealed record RecipeListItem(
    int Id,
    int ProductId,
    string ProductName,
    int VersionNumber,
    int Status,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    string CreatedByName,
    DateTime? ApprovedAt,
    string? ApprovedByName);

public sealed record RecipeHeader(
    int Id,
    int ProductId,
    string ProductName,
    int VersionNumber,
    int Status,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    string CreatedByName,
    DateTime? ApprovedAt,
    string? ApprovedByName);

public sealed record RecipeComponentItem(
    int Id,
    int RawMaterialId,
    string RawMaterialName,
    decimal Percentage,
    int LoadOrder,
    decimal AllowedDeviationPercent);

public sealed record RecipeDetail(RecipeHeader Header, List<RecipeComponentItem> Components);

public sealed record CreateRecipeRequest(
    int ProductId,
    int CreatedByUserId,
    string? Notes,
    List<CreateRecipeComponentRequest> Components);

public sealed record CreateRecipeComponentRequest(
    int RawMaterialId,
    decimal Percentage,
    int LoadOrder,
    decimal AllowedDeviationPercent);

public sealed record CreateRecipeResponse(int Id, int ProductId, int VersionNumber);

public sealed record ApproveRecipeRequest(int ApprovedByUserId, string? Comment);
