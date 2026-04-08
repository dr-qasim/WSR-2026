using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Data;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/production")]
public sealed class ProductionController(PlantProductionDbContext dbContext) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var items = await dbContext.ProductionOrders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new ProductionOrderItem(
                x.Id,
                x.OrderNumber,
                x.ProductId,
                x.Product.Name,
                x.ProductionLineId,
                x.ProductionLine.Name,
                x.PlannedQuantity,
                x.PlannedStartAt,
                x.Status,
                x.CreatedAt,
                x.CreatedByUser.FullName))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<ProductionOrderItem>>.Ok(items));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.ProductionLineId <= 0 || request.CreatedByUserId <= 0 || request.PlannedQuantity <= 0)
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные производственного заказа."));
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        try
        {
            var order = new ProductionOrder
            {
                OrderNumber = orderNumber,
                ProductId = request.ProductId,
                ProductionLineId = request.ProductionLineId,
                PlannedQuantity = request.PlannedQuantity,
                PlannedStartAt = request.PlannedStartAt,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            dbContext.ProductionOrders.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<CreateProductionOrderResponse>.Ok(
                new CreateProductionOrderResponse(order.Id, orderNumber),
                "Производственный заказ создан."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches(CancellationToken cancellationToken)
    {
        var items = await dbContext.ProductionBatches
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new ProductionBatchItem(
                x.Id,
                x.BatchNumber,
                x.ProductionOrderId,
                x.ProductionOrder != null ? x.ProductionOrder.OrderNumber : null,
                x.ProductId,
                x.Product.Name,
                x.ProductionLineId,
                x.ProductionLine.Name,
                x.RecipeVersionId,
                x.TechnologyCardId,
                x.PlannedQuantity,
                x.Status,
                x.StartedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<ProductionBatchItem>>.Ok(items));
    }

    [HttpPost("batches")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateProductionBatchRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.ProductionLineId <= 0 || request.RecipeVersionId <= 0 ||
            request.TechnologyCardId <= 0 || request.PlannedQuantity <= 0)
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные производственной партии."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var batchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            var batch = new ProductionBatch
            {
                BatchNumber = batchNumber,
                ProductionOrderId = request.ProductionOrderId,
                ProductId = request.ProductId,
                RecipeVersionId = request.RecipeVersionId,
                TechnologyCardId = request.TechnologyCardId,
                ProductionLineId = request.ProductionLineId,
                ExtruderProgramId = request.ExtruderProgramId,
                PlannedQuantity = request.PlannedQuantity,
                Status = 1
            };

            dbContext.ProductionBatches.Add(batch);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var consumption in request.Consumptions)
            {
                dbContext.BatchRawMaterialConsumptions.Add(new BatchRawMaterialConsumption
                {
                    ProductionBatchId = batch.Id,
                    RawMaterialLotId = consumption.RawMaterialLotId,
                    QuantityUsed = consumption.QuantityUsed
                });

                var lot = await dbContext.RawMaterialLots
                    .FirstOrDefaultAsync(x => x.Id == consumption.RawMaterialLotId, cancellationToken);

                if (lot is null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(ApiResponse.Fail("Партия сырья не найдена."));
                }

                lot.QuantityAvailable -= consumption.QuantityUsed;
            }

            var stepIds = await dbContext.TechnologySteps
                .AsNoTracking()
                .Where(x => x.TechnologyCardId == request.TechnologyCardId)
                .OrderBy(x => x.StepOrder)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var stepId in stepIds)
            {
                dbContext.BatchTechnologyStepRuns.Add(new BatchTechnologyStepRun
                {
                    ProductionBatchId = batch.Id,
                    TechnologyStepId = stepId,
                    Status = 1
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateProductionBatchResponse>.Ok(
                new CreateProductionBatchResponse(batch.Id, batchNumber),
                "Производственная партия создана."));
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpGet("batches/{id:int}/steps")]
    public async Task<IActionResult> GetBatchSteps(int id, CancellationToken cancellationToken)
    {
        var items = await dbContext.BatchTechnologyStepRuns
            .AsNoTracking()
            .Where(x => x.ProductionBatchId == id)
            .OrderBy(x => x.TechnologyStep.StepOrder)
            .Select(x => new BatchStepRunItem(
                x.Id,
                x.ProductionBatchId,
                x.TechnologyStepId,
                x.TechnologyStep.StepOrder,
                x.TechnologyStep.StepType,
                x.TechnologyStep.Title,
                x.Status,
                x.StartedAt,
                x.CompletedAt,
                x.Comment))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<BatchStepRunItem>>.Ok(items));
    }

    [HttpPost("batches/{id:int}/start")]
    public async Task<IActionResult> StartBatch(int id, CancellationToken cancellationToken)
    {
        var batch = await dbContext.ProductionBatches
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (batch is null)
        {
            return NotFound(ApiResponse.Fail("Партия не найдена."));
        }

        batch.Status = 2;
        batch.StartedAt ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse.Ok("Партия переведена в работу."));
    }

    [HttpPost("batches/{id:int}/complete")]
    public async Task<IActionResult> CompleteBatch(int id, CancellationToken cancellationToken)
    {
        var batch = await dbContext.ProductionBatches
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (batch is null)
        {
            return NotFound(ApiResponse.Fail("Партия не найдена."));
        }

        try
        {
            batch.Status = 6;
            batch.CompletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Партия завершена."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("step-runs/{id:int}/start")]
    public async Task<IActionResult> StartStepRun(int id, [FromBody] StartStepRunRequest request, CancellationToken cancellationToken)
    {
        if (request.StartedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который начал шаг."));
        }

        var stepRun = await dbContext.BatchTechnologyStepRuns
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (stepRun is null)
        {
            return NotFound(ApiResponse.Fail("Шаг партии не найден."));
        }

        try
        {
            stepRun.Status = 2;
            stepRun.StartedAt ??= DateTime.UtcNow;
            stepRun.StartedByUserId = request.StartedByUserId;
            stepRun.Comment = request.Comment ?? stepRun.Comment;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Шаг партии начат."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("step-runs/{id:int}/complete")]
    public async Task<IActionResult> CompleteStepRun(int id, [FromBody] CompleteStepRunRequest request, CancellationToken cancellationToken)
    {
        if (request.CompletedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который завершил шаг."));
        }

        var stepRun = await dbContext.BatchTechnologyStepRuns
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (stepRun is null)
        {
            return NotFound(ApiResponse.Fail("Шаг партии не найден."));
        }

        try
        {
            stepRun.Status = 3;
            stepRun.CompletedAt = DateTime.UtcNow;
            stepRun.CompletedByUserId = request.CompletedByUserId;
            stepRun.Comment = request.Comment ?? stepRun.Comment;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Шаг партии завершен."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("step-runs/{id:int}/measurements")]
    public async Task<IActionResult> AddMeasurement(int id, [FromBody] AddMeasurementRequest request, CancellationToken cancellationToken)
    {
        if (request.TechnologyStepParameterId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать параметр технологического шага."));
        }

        var actualValueCount = CountActualValues(request.ActualNumericValue, request.ActualTextValue, request.ActualBooleanValue);
        if (actualValueCount != 1)
        {
            return BadRequest(ApiResponse.Fail("Нужно передать ровно одно фактическое значение."));
        }

        var metadata = await dbContext.TechnologyStepParameters
            .AsNoTracking()
            .Where(x => x.Id == request.TechnologyStepParameterId)
            .Select(x => new ParameterMetadata(
                x.ValueType,
                x.TargetNumericValue,
                x.MinNumericValue,
                x.MaxNumericValue,
                x.TargetTextValue,
                x.TargetBooleanValue))
            .FirstOrDefaultAsync(cancellationToken);

        if (metadata is null)
        {
            return NotFound(ApiResponse.Fail("Параметр технологического шага не найден."));
        }

        var isWithinTolerance = CalculateMeasurementResult(metadata, request);

        try
        {
            var measurement = new BatchStepMeasuredValue
            {
                BatchTechnologyStepRunId = id,
                TechnologyStepParameterId = request.TechnologyStepParameterId,
                ActualNumericValue = request.ActualNumericValue,
                ActualTextValue = request.ActualTextValue,
                ActualBooleanValue = request.ActualBooleanValue,
                IsWithinTolerance = isWithinTolerance,
                RecordedAt = DateTime.UtcNow,
                Comment = request.Comment
            };

            dbContext.BatchStepMeasuredValues.Add(measurement);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<MeasurementResponse>.Ok(
                new MeasurementResponse(measurement.Id, isWithinTolerance),
                "Фактическое значение записано."));
        }
        catch (DbUpdateException exception)
        {
            return BadRequest(ApiResponse.Fail(GetDbErrorMessage(exception)));
        }
    }

    [HttpPost("deviations")]
    public async Task<IActionResult> CreateDeviation([FromBody] CreateDeviationRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductionBatchId <= 0 || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные отклонения."));
        }

        try
        {
            var deviation = new ProcessDeviation
            {
                ProductionBatchId = request.ProductionBatchId,
                BatchTechnologyStepRunId = request.BatchTechnologyStepRunId,
                Title = request.Title.Trim(),
                ParameterName = request.ParameterName,
                PlannedValue = request.PlannedValue,
                ActualValue = request.ActualValue,
                Severity = request.Severity,
                Details = request.Details,
                CreatedAt = DateTime.UtcNow,
                ReportedByUserId = request.ReportedByUserId
            };

            dbContext.ProcessDeviations.Add(deviation);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<DeviationResponse>.Ok(
                new DeviationResponse(deviation.Id),
                "Отклонение зарегистрировано."));
        }
        catch (DbUpdateException exception)
        {
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

    private static bool CalculateMeasurementResult(ParameterMetadata metadata, AddMeasurementRequest request)
    {
        return metadata.ValueType switch
        {
            1 => CalculateNumericMeasurementResult(metadata, request.ActualNumericValue),
            2 => string.Equals(
                request.ActualTextValue?.Trim(),
                metadata.TargetTextValue?.Trim(),
                StringComparison.OrdinalIgnoreCase),
            3 => request.ActualBooleanValue == metadata.TargetBooleanValue,
            _ => false
        };
    }

    private static bool CalculateNumericMeasurementResult(ParameterMetadata metadata, decimal? actualNumericValue)
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

        if (metadata.MinNumericValue is null && metadata.MaxNumericValue is null && metadata.TargetNumericValue is not null)
        {
            return actualNumericValue == metadata.TargetNumericValue;
        }

        return true;
    }

    private static string GetDbErrorMessage(DbUpdateException exception)
    {
        return exception.InnerException?.Message ?? exception.Message;
    }

    private sealed record ParameterMetadata(
        int ValueType,
        decimal? TargetNumericValue,
        decimal? MinNumericValue,
        decimal? MaxNumericValue,
        string? TargetTextValue,
        bool? TargetBooleanValue);
}

public sealed record ProductionOrderItem(
    int Id,
    string OrderNumber,
    int ProductId,
    string ProductName,
    int ProductionLineId,
    string ProductionLineName,
    decimal PlannedQuantity,
    DateTime? PlannedStartAt,
    int Status,
    DateTime CreatedAt,
    string CreatedByName);

public sealed record CreateProductionOrderRequest(
    int ProductId,
    int ProductionLineId,
    decimal PlannedQuantity,
    DateTime? PlannedStartAt,
    int CreatedByUserId);

public sealed record CreateProductionOrderResponse(int Id, string OrderNumber);

public sealed record ProductionBatchItem(
    int Id,
    string BatchNumber,
    int? ProductionOrderId,
    string? OrderNumber,
    int ProductId,
    string ProductName,
    int ProductionLineId,
    string ProductionLineName,
    int RecipeVersionId,
    int TechnologyCardId,
    decimal PlannedQuantity,
    int Status,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public sealed record BatchConsumptionRequest(int RawMaterialLotId, decimal QuantityUsed);

public sealed record CreateProductionBatchRequest(
    int? ProductionOrderId,
    int ProductId,
    int ProductionLineId,
    int RecipeVersionId,
    int TechnologyCardId,
    int? ExtruderProgramId,
    decimal PlannedQuantity,
    List<BatchConsumptionRequest> Consumptions);

public sealed record CreateProductionBatchResponse(int Id, string BatchNumber);

public sealed record BatchStepRunItem(
    int Id,
    int ProductionBatchId,
    int TechnologyStepId,
    int StepOrder,
    int StepType,
    string Title,
    int Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Comment);

public sealed record StartStepRunRequest(int StartedByUserId, string? Comment);

public sealed record CompleteStepRunRequest(int CompletedByUserId, string? Comment);

public sealed record AddMeasurementRequest(
    int TechnologyStepParameterId,
    decimal? ActualNumericValue,
    string? ActualTextValue,
    bool? ActualBooleanValue,
    string? Comment);

public sealed record MeasurementResponse(int Id, bool IsWithinTolerance);

public sealed record CreateDeviationRequest(
    int ProductionBatchId,
    int? BatchTechnologyStepRunId,
    string Title,
    string? ParameterName,
    string? PlannedValue,
    string? ActualValue,
    int Severity,
    string? Details,
    int? ReportedByUserId);

public sealed record DeviationResponse(int Id);
