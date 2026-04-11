using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/laboratory")]
public sealed class LaboratoryController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
{
    [HttpGet("specifications")]
    public async Task<IActionResult> GetSpecifications(CancellationToken cancellationToken)
    {
        var items = await dbContext.QualitySpecifications
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new QualitySpecificationItem(
                x.Id,
                x.Code,
                x.Name,
                x.SubjectType,
                x.RawMaterialId,
                x.RawMaterial != null ? x.RawMaterial.Name : null,
                x.ProductId,
                x.Product != null ? x.Product.Name : null,
                x.VersionNumber,
                x.Status,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<QualitySpecificationItem>>.Ok(items));
    }

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests(CancellationToken cancellationToken)
    {
        var items = await dbContext.LaboratoryTests
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new LaboratoryTestItem(
                x.Id,
                x.TestNumber,
                x.SubjectType,
                x.RawMaterialLotId,
                x.RawMaterialLot != null ? x.RawMaterialLot.InternalLotNumber : null,
                x.ProductionBatchId,
                x.ProductionBatch != null ? x.ProductionBatch.BatchNumber : null,
                x.QualitySpecificationId,
                x.QualitySpecification.Name,
                x.TestKind,
                x.Status,
                x.Priority,
                x.Comment,
                x.ResultSummary,
                x.CreatedAt,
                x.AssignedAt,
                x.StartedAt,
                x.CompletedAt,
                x.TesterUser != null ? x.TesterUser.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<LaboratoryTestItem>>.Ok(items));
    }

    [HttpGet("decisions")]
    public async Task<IActionResult> GetDecisions(CancellationToken cancellationToken)
    {
        var items = await dbContext.QualityDecisions
            .AsNoTracking()
            .OrderByDescending(x => x.DecidedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new QualityDecisionItem(
                x.Id,
                x.SubjectType,
                x.RawMaterialLotId,
                x.RawMaterialLot != null ? x.RawMaterialLot.InternalLotNumber : null,
                x.ProductionBatchId,
                x.ProductionBatch != null ? x.ProductionBatch.BatchNumber : null,
                x.LaboratoryTestId,
                x.LaboratoryTest.TestNumber,
                x.DecisionStatus,
                x.Comment,
                x.BlockReason,
                x.IsCurrent,
                x.DecidedAt,
                x.DecidedByUser.FullName))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<QualityDecisionItem>>.Ok(items));
    }

    [HttpGet("tests/{id:int}")]
    public async Task<IActionResult> GetTest(int id, CancellationToken cancellationToken)
    {
        var test = await dbContext.LaboratoryTests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LaboratoryTestItem(
                x.Id,
                x.TestNumber,
                x.SubjectType,
                x.RawMaterialLotId,
                x.RawMaterialLot != null ? x.RawMaterialLot.InternalLotNumber : null,
                x.ProductionBatchId,
                x.ProductionBatch != null ? x.ProductionBatch.BatchNumber : null,
                x.QualitySpecificationId,
                x.QualitySpecification.Name,
                x.TestKind,
                x.Status,
                x.Priority,
                x.Comment,
                x.ResultSummary,
                x.CreatedAt,
                x.AssignedAt,
                x.StartedAt,
                x.CompletedAt,
                x.TesterUser != null ? x.TesterUser.FullName : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (test is null)
        {
            return NotFound(ApiResponse.Fail("Лабораторное испытание не найдено."));
        }

        var results = await dbContext.LaboratoryTestParameterResults
            .AsNoTracking()
            .Where(x => x.LaboratoryTestId == id)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LaboratoryTestParameterResultItem(
                x.Id,
                x.SortOrder,
                x.ParameterName,
                x.ValueType,
                x.Unit,
                x.MinNumericValue,
                x.MaxNumericValue,
                x.TargetTextValue,
                x.TargetBooleanValue,
                x.ActualNumericValue,
                x.ActualTextValue,
                x.ActualBooleanValue,
                x.IsRequired,
                x.IsWithinRange,
                x.Comment))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<LaboratoryTestDetail>.Ok(new LaboratoryTestDetail(test, results)));
    }

    [HttpPost("tests")]
    public async Task<IActionResult> CreateTest([FromBody] CreateLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.SubjectType is < 1 or > 2 || request.QualitySpecificationId <= 0 || string.IsNullOrWhiteSpace(request.TestKind))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные лабораторного испытания."));
        }

        if (request.SubjectType == 1)
        {
            if (request.RawMaterialLotId is null || request.ProductionBatchId is not null)
            {
                return BadRequest(ApiResponse.Fail("Для входного контроля нужно указать только партию сырья."));
            }
        }

        if (request.SubjectType == 2)
        {
            if (request.ProductionBatchId is null || request.RawMaterialLotId is not null)
            {
                return BadRequest(ApiResponse.Fail("Для контроля партии нужно указать только производственную партию."));
            }
        }

        var specification = await dbContext.QualitySpecifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.QualitySpecificationId, cancellationToken);

        if (specification is null)
        {
            return BadRequest(ApiResponse.Fail("Спецификация качества не найдена."));
        }

        if (!specification.IsActive || specification.Status != 2)
        {
            return BadRequest(ApiResponse.Fail("Можно использовать только активную утвержденную спецификацию."));
        }

        if (specification.SubjectType != request.SubjectType)
        {
            return BadRequest(ApiResponse.Fail("Спецификация не подходит для выбранного типа объекта контроля."));
        }

        if (request.SubjectType == 1)
        {
            var lot = await dbContext.RawMaterialLots
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.RawMaterialLotId, cancellationToken);

            if (lot is null)
            {
                return BadRequest(ApiResponse.Fail("Партия сырья не найдена."));
            }

            if (specification.RawMaterialId != lot.RawMaterialId)
            {
                return BadRequest(ApiResponse.Fail("Спецификация не подходит для выбранной партии сырья."));
            }
        }

        if (request.SubjectType == 2)
        {
            var batch = await dbContext.ProductionBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProductionBatchId, cancellationToken);

            if (batch is null)
            {
                return BadRequest(ApiResponse.Fail("Производственная партия не найдена."));
            }

            if (specification.ProductId != batch.ProductId)
            {
                return BadRequest(ApiResponse.Fail("Спецификация не подходит для выбранной производственной партии."));
            }
        }

        if (request.TesterUserId is not null)
        {
            var testerExists = await dbContext.AppUsers
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.TesterUserId.Value && x.IsActive, cancellationToken);

            if (!testerExists)
            {
                return BadRequest(ApiResponse.Fail("Лаборант не найден."));
            }
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var testNumber = $"TEST-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            var test = new LaboratoryTest
            {
                TestNumber = testNumber,
                SubjectType = request.SubjectType,
                RawMaterialLotId = request.RawMaterialLotId,
                ProductionBatchId = request.ProductionBatchId,
                QualitySpecificationId = request.QualitySpecificationId,
                TestKind = request.TestKind.Trim(),
                Status = 1,
                Priority = request.Priority,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = request.TesterUserId is null ? null : DateTime.UtcNow,
                TesterUserId = request.TesterUserId
            };

            dbContext.LaboratoryTests.Add(test);
            await dbContext.SaveChangesAsync(cancellationToken);

            var parameters = await dbContext.QualitySpecificationParameters
                .AsNoTracking()
                .Where(x => x.QualitySpecificationId == request.QualitySpecificationId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            foreach (var parameter in parameters)
            {
                dbContext.LaboratoryTestParameterResults.Add(new LaboratoryTestParameterResult
                {
                    LaboratoryTestId = test.Id,
                    QualitySpecificationParameterId = parameter.Id,
                    SortOrder = parameter.SortOrder,
                    ParameterName = parameter.Name,
                    ValueType = parameter.ValueType,
                    Unit = parameter.Unit,
                    MinNumericValue = parameter.MinNumericValue,
                    MaxNumericValue = parameter.MaxNumericValue,
                    TargetTextValue = parameter.TargetTextValue,
                    TargetBooleanValue = parameter.TargetBooleanValue,
                    IsRequired = parameter.IsRequired
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateLaboratoryTestResponse>.Ok(
                new CreateLaboratoryTestResponse(test.Id, testNumber),
                "Лабораторное испытание создано."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("tests/{id:int}/start")]
    public async Task<IActionResult> StartTest(int id, [FromBody] StartLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.TesterUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать лаборанта."));
        }

        var testerExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.TesterUserId && x.IsActive, cancellationToken);

        if (!testerExists)
        {
            return BadRequest(ApiResponse.Fail("Лаборант не найден."));
        }

        var test = await dbContext.LaboratoryTests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (test is null)
        {
            return NotFound(ApiResponse.Fail("Испытание не найдено."));
        }

        if (test.Status == 3)
        {
            return BadRequest(ApiResponse.Fail("Завершенное испытание нельзя начать заново."));
        }

        try
        {
            test.Status = 2;
            test.TesterUserId = request.TesterUserId;
            test.StartedAt ??= DateTime.UtcNow;
            test.AssignedAt ??= test.StartedAt;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Испытание начато."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("tests/{id:int}/results")]
    public async Task<IActionResult> SaveResults(int id, [FromBody] SaveLaboratoryResultsRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно передать хотя бы один результат испытания."));
        }

        var test = await dbContext.LaboratoryTests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (test is null)
        {
            return NotFound(ApiResponse.Fail("Испытание не найдено."));
        }

        if (test.Status == 3)
        {
            return BadRequest(ApiResponse.Fail("Нельзя менять результаты завершенного испытания."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var item in request.Items)
            {
                var actualValueCount = CountActualValues(item.ActualNumericValue, item.ActualTextValue, item.ActualBooleanValue);
                if (actualValueCount != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(ApiResponse.Fail("Для каждого результата нужно передать ровно одно фактическое значение."));
                }

                var parameterResult = await dbContext.LaboratoryTestParameterResults
                    .FirstOrDefaultAsync(x => x.Id == item.ParameterResultId, cancellationToken);

                if (parameterResult is null || parameterResult.LaboratoryTestId != id)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(ApiResponse.Fail("Результат не относится к выбранному испытанию."));
                }

                var metadata = new LaboratoryParameterMetadata(
                    parameterResult.LaboratoryTestId,
                    parameterResult.ValueType,
                    parameterResult.MinNumericValue,
                    parameterResult.MaxNumericValue,
                    parameterResult.TargetTextValue,
                    parameterResult.TargetBooleanValue);

                parameterResult.ActualNumericValue = item.ActualNumericValue;
                parameterResult.ActualTextValue = item.ActualTextValue;
                parameterResult.ActualBooleanValue = item.ActualBooleanValue;
                parameterResult.IsWithinRange = CalculateResult(metadata, item);
                parameterResult.Comment = item.Comment;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Результаты испытания сохранены."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("tests/{id:int}/complete")]
    public async Task<IActionResult> CompleteTest(int id, [FromBody] CompleteLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.TesterUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать лаборанта."));
        }

        var testerExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.TesterUserId && x.IsActive, cancellationToken);

        if (!testerExists)
        {
            return BadRequest(ApiResponse.Fail("Лаборант не найден."));
        }

        var test = await dbContext.LaboratoryTests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (test is null)
        {
            return NotFound(ApiResponse.Fail("Испытание не найдено."));
        }

        var requiredResults = await dbContext.LaboratoryTestParameterResults
            .AsNoTracking()
            .Where(x => x.LaboratoryTestId == id && x.IsRequired)
            .ToListAsync(cancellationToken);

        var hasAllRequiredResults = requiredResults.All(x =>
            x.ActualNumericValue is not null ||
            !string.IsNullOrWhiteSpace(x.ActualTextValue) ||
            x.ActualBooleanValue is not null);

        if (!hasAllRequiredResults)
        {
            return BadRequest(ApiResponse.Fail("Нельзя завершить испытание, пока не заполнены обязательные результаты."));
        }

        try
        {
            test.Status = 3;
            test.TesterUserId = request.TesterUserId;
            test.StartedAt ??= DateTime.UtcNow;
            test.CompletedAt = DateTime.UtcNow;
            test.ResultSummary = request.ResultSummary;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Испытание завершено."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("decisions")]
    public async Task<IActionResult> CreateDecision([FromBody] CreateQualityDecisionRequest request, CancellationToken cancellationToken)
    {
        if (request.LaboratoryTestId <= 0 || request.DecidedByUserId <= 0 || string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные решения по качеству."));
        }

        var deciderExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.DecidedByUserId && x.IsActive, cancellationToken);

        if (!deciderExists)
        {
            return BadRequest(ApiResponse.Fail("Пользователь, который принимает решение, не найден."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var subject = await dbContext.LaboratoryTests
                .AsNoTracking()
                .Where(x => x.Id == request.LaboratoryTestId)
                .Select(x => new DecisionSubject(
                    x.SubjectType,
                    x.RawMaterialLotId,
                    x.ProductionBatchId))
                .FirstOrDefaultAsync(cancellationToken);

            if (subject is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(ApiResponse.Fail("Испытание не найдено."));
            }

            var testCompleted = await dbContext.LaboratoryTests
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.LaboratoryTestId && x.Status == 3, cancellationToken);

            if (!testCompleted)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ApiResponse.Fail("Решение можно принять только по завершенному испытанию."));
            }

            await dbContext.QualityDecisions
                .Where(x => x.SubjectType == subject.SubjectType
                    && x.IsCurrent
                    && ((subject.RawMaterialLotId != null && x.RawMaterialLotId == subject.RawMaterialLotId)
                        || (subject.ProductionBatchId != null && x.ProductionBatchId == subject.ProductionBatchId)))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsCurrent, false), cancellationToken);

            var decidedAt = DateTime.UtcNow;
            var decision = new QualityDecision
            {
                SubjectType = subject.SubjectType,
                RawMaterialLotId = subject.RawMaterialLotId,
                ProductionBatchId = subject.ProductionBatchId,
                LaboratoryTestId = request.LaboratoryTestId,
                DecisionStatus = request.DecisionStatus,
                Comment = request.Comment.Trim(),
                BlockReason = request.BlockReason,
                IsCurrent = true,
                DecidedAt = decidedAt,
                DecidedByUserId = request.DecidedByUserId
            };

            dbContext.QualityDecisions.Add(decision);

            if (subject.RawMaterialLotId is not null)
            {
                var lot = await dbContext.RawMaterialLots
                    .FirstOrDefaultAsync(x => x.Id == subject.RawMaterialLotId.Value, cancellationToken);

                if (lot is not null)
                {
                    lot.LastLaboratoryDecisionAt = decidedAt;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateQualityDecisionResponse>.Ok(
                new CreateQualityDecisionResponse(decision.Id),
                "Решение по качеству принято."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    private static int CountActualValues(decimal? numericValue, string? textValue, bool? booleanValue)
    {
        var count = 0;
        if (numericValue is not null) count++;
        if (!string.IsNullOrWhiteSpace(textValue)) count++;
        if (booleanValue is not null) count++;
        return count;
    }

    private static bool CalculateResult(LaboratoryParameterMetadata metadata, SaveLaboratoryResultItem item)
    {
        return metadata.ValueType switch
        {
            1 => CalculateNumericResult(metadata, item.ActualNumericValue),
            2 => string.Equals(item.ActualTextValue?.Trim(), metadata.TargetTextValue?.Trim(), StringComparison.OrdinalIgnoreCase),
            3 => item.ActualBooleanValue == metadata.TargetBooleanValue,
            _ => false
        };
    }

    private static bool CalculateNumericResult(LaboratoryParameterMetadata metadata, decimal? actualNumericValue)
    {
        if (actualNumericValue is null)
        {
            return false;
        }

        if (metadata.MinNumericValue is not null && actualNumericValue < metadata.MinNumericValue)
        {
            return false;
        }

        if (metadata.MaxNumericValue is not null && actualNumericValue > metadata.MaxNumericValue)
        {
            return false;
        }

        return true;
    }

    private static string GetDbErrorMessage(DbUpdateException exception)
    {
        return exception.InnerException?.Message ?? exception.Message;
    }

    private sealed record LaboratoryParameterMetadata(
        int LaboratoryTestId,
        int ValueType,
        decimal? MinNumericValue,
        decimal? MaxNumericValue,
        string? TargetTextValue,
        bool? TargetBooleanValue);

    private sealed record DecisionSubject(
        int SubjectType,
        int? RawMaterialLotId,
        int? ProductionBatchId);
}

public sealed record QualitySpecificationItem(
    int Id,
    string Code,
    string Name,
    int SubjectType,
    int? RawMaterialId,
    string? RawMaterialName,
    int? ProductId,
    string? ProductName,
    int VersionNumber,
    int Status,
    bool IsActive);

public sealed record LaboratoryTestItem(
    int Id,
    string TestNumber,
    int SubjectType,
    int? RawMaterialLotId,
    string? RawMaterialLotNumber,
    int? ProductionBatchId,
    string? ProductionBatchNumber,
    int QualitySpecificationId,
    string QualitySpecificationName,
    string TestKind,
    int Status,
    int Priority,
    string? Comment,
    string? ResultSummary,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? TesterName);

public sealed record QualityDecisionItem(
    int Id,
    int SubjectType,
    int? RawMaterialLotId,
    string? RawMaterialLotNumber,
    int? ProductionBatchId,
    string? ProductionBatchNumber,
    int LaboratoryTestId,
    string TestNumber,
    int DecisionStatus,
    string? Comment,
    string? BlockReason,
    bool IsCurrent,
    DateTime DecidedAt,
    string DecidedByName);

public sealed record LaboratoryTestParameterResultItem(
    int Id,
    int SortOrder,
    string ParameterName,
    int ValueType,
    string? Unit,
    decimal? MinNumericValue,
    decimal? MaxNumericValue,
    string? TargetTextValue,
    bool? TargetBooleanValue,
    decimal? ActualNumericValue,
    string? ActualTextValue,
    bool? ActualBooleanValue,
    bool IsRequired,
    bool? IsWithinRange,
    string? Comment);

public sealed record LaboratoryTestDetail(
    LaboratoryTestItem Test,
    List<LaboratoryTestParameterResultItem> Results);

public sealed record CreateLaboratoryTestRequest(
    int SubjectType,
    int? RawMaterialLotId,
    int? ProductionBatchId,
    int QualitySpecificationId,
    string TestKind,
    int Priority,
    string? Comment,
    int? TesterUserId);

public sealed record CreateLaboratoryTestResponse(int Id, string TestNumber);

public sealed record StartLaboratoryTestRequest(int TesterUserId);

public sealed record SaveLaboratoryResultsRequest(List<SaveLaboratoryResultItem> Items);

public sealed record SaveLaboratoryResultItem(
    int ParameterResultId,
    decimal? ActualNumericValue,
    string? ActualTextValue,
    bool? ActualBooleanValue,
    string? Comment);

public sealed record CompleteLaboratoryTestRequest(int TesterUserId, string? ResultSummary);

public sealed record CreateQualityDecisionRequest(
    int LaboratoryTestId,
    int DecisionStatus,
    string Comment,
    string? BlockReason,
    int DecidedByUserId);

public sealed record CreateQualityDecisionResponse(int Id);
