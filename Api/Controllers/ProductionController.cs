using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/production")]
public sealed class ProductionController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
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

        var productExists = await dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);

        if (!productExists)
        {
            return BadRequest(ApiResponse.Fail("Продукт не найден."));
        }

        var lineExists = await dbContext.ProductionLines
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductionLineId && x.IsActive, cancellationToken);

        if (!lineExists)
        {
            return BadRequest(ApiResponse.Fail("Производственная линия не найдена или отключена."));
        }

        var userExists = await dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CreatedByUserId && x.IsActive, cancellationToken);

        if (!userExists)
        {
            return BadRequest(ApiResponse.Fail("Пользователь-создатель не найден."));
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

        if (request.Consumptions.Count == 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать хотя бы один расход сырья."));
        }

        if (request.Consumptions.Any(x => x.RawMaterialLotId <= 0 || x.QuantityUsed <= 0))
        {
            return BadRequest(ApiResponse.Fail("В расходе сырья есть некорректные данные."));
        }

        if (request.Consumptions.Select(x => x.RawMaterialLotId).Distinct().Count() != request.Consumptions.Count)
        {
            return BadRequest(ApiResponse.Fail("Одна и та же партия сырья не должна повторяться в расходе."));
        }

        var productExists = await dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);

        if (!productExists)
        {
            return BadRequest(ApiResponse.Fail("Продукт не найден."));
        }

        var lineExists = await dbContext.ProductionLines
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductionLineId && x.IsActive, cancellationToken);

        if (!lineExists)
        {
            return BadRequest(ApiResponse.Fail("Производственная линия не найдена или отключена."));
        }

        var recipe = await dbContext.RecipeVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RecipeVersionId, cancellationToken);

        if (recipe is null)
        {
            return BadRequest(ApiResponse.Fail("Рецептура не найдена."));
        }

        if (recipe.ProductId != request.ProductId)
        {
            return BadRequest(ApiResponse.Fail("Рецептура относится к другому продукту."));
        }

        if (!recipe.IsActive || recipe.Status != 3)
        {
            return BadRequest(ApiResponse.Fail("Для партии можно использовать только активную утвержденную рецептуру."));
        }

        var technologyCard = await dbContext.TechnologyCards
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TechnologyCardId, cancellationToken);

        if (technologyCard is null)
        {
            return BadRequest(ApiResponse.Fail("Технологическая карта не найдена."));
        }

        if (technologyCard.ProductId != request.ProductId)
        {
            return BadRequest(ApiResponse.Fail("Технологическая карта относится к другому продукту."));
        }

        if (!technologyCard.IsActive || technologyCard.Status != 3)
        {
            return BadRequest(ApiResponse.Fail("Для партии можно использовать только активную утвержденную технологическую карту."));
        }

        if (request.ProductionOrderId is not null)
        {
            var order = await dbContext.ProductionOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProductionOrderId.Value, cancellationToken);

            if (order is null)
            {
                return BadRequest(ApiResponse.Fail("Производственный заказ не найден."));
            }

            if (order.ProductId != request.ProductId || order.ProductionLineId != request.ProductionLineId)
            {
                return BadRequest(ApiResponse.Fail("Заказ не соответствует выбранному продукту или линии."));
            }
        }

        if (request.ExtruderProgramId is not null)
        {
            var extruderProgram = await dbContext.ExtruderPrograms
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ExtruderProgramId.Value, cancellationToken);

            if (extruderProgram is null)
            {
                return BadRequest(ApiResponse.Fail("Программа экструдера не найдена."));
            }

            if (extruderProgram.ProductId != request.ProductId || extruderProgram.TechnologyCardId != request.TechnologyCardId || !extruderProgram.IsActive)
            {
                return BadRequest(ApiResponse.Fail("Программа экструдера не подходит для выбранной партии."));
            }
        }

        var lotIds = request.Consumptions.Select(x => x.RawMaterialLotId).ToList();
        var lots = await dbContext.RawMaterialLots
            .Where(x => lotIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (lots.Count != lotIds.Count)
        {
            return BadRequest(ApiResponse.Fail("Одна или несколько партий сырья не найдены."));
        }

        foreach (var consumption in request.Consumptions)
        {
            var lot = lots.First(x => x.Id == consumption.RawMaterialLotId);
            if (lot.QuantityAvailable < consumption.QuantityUsed)
            {
                return BadRequest(ApiResponse.Fail($"Недостаточно остатка по партии сырья {lot.InternalLotNumber}."));
            }
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

    [HttpGet("deviations")]
    public async Task<IActionResult> GetDeviations(CancellationToken cancellationToken)
    {
        var items = await dbContext.ProcessDeviations
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new DeviationItem(
                x.Id,
                x.ProductionBatchId,
                x.ProductionBatch.BatchNumber,
                x.BatchTechnologyStepRunId,
                x.BatchTechnologyStepRun != null ? x.BatchTechnologyStepRun.TechnologyStep.StepOrder : null,
                x.BatchTechnologyStepRun != null ? x.BatchTechnologyStepRun.TechnologyStep.Title : null,
                x.Title,
                x.ParameterName,
                x.PlannedValue,
                x.ActualValue,
                x.Severity,
                x.Details,
                x.CreatedAt,
                x.ReportedByUser != null ? x.ReportedByUser.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<DeviationItem>>.Ok(items));
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

        if (batch.Status >= 6)
        {
            return BadRequest(ApiResponse.Fail("Завершенную партию нельзя снова запустить."));
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

        var hasUnfinishedSteps = await dbContext.BatchTechnologyStepRuns
            .AsNoTracking()
            .AnyAsync(x => x.ProductionBatchId == id && x.Status != 3, cancellationToken);

        if (hasUnfinishedSteps)
        {
            return BadRequest(ApiResponse.Fail("Нельзя завершить партию, пока не завершены все шаги."));
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
            .Include(x => x.ProductionBatch)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (stepRun is null)
        {
            return NotFound(ApiResponse.Fail("Шаг партии не найден."));
        }

        if (stepRun.ProductionBatch.Status < 2)
        {
            return BadRequest(ApiResponse.Fail("Сначала нужно перевести партию в работу."));
        }

        if (stepRun.Status == 3)
        {
            return BadRequest(ApiResponse.Fail("Шаг уже завершен."));
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
            .Include(x => x.TechnologyStep)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (stepRun is null)
        {
            return NotFound(ApiResponse.Fail("Шаг партии не найден."));
        }

        if (stepRun.Status == 1)
        {
            return BadRequest(ApiResponse.Fail("Сначала нужно начать шаг."));
        }

        if (stepRun.Status == 3)
        {
            return BadRequest(ApiResponse.Fail("Шаг уже завершен."));
        }

        var requiredParameterIds = await dbContext.TechnologyStepParameters
            .AsNoTracking()
            .Where(x => x.TechnologyStepId == stepRun.TechnologyStepId && x.IsRequired)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (requiredParameterIds.Count > 0)
        {
            var measuredParameterIds = await dbContext.BatchStepMeasuredValues
                .AsNoTracking()
                .Where(x => x.BatchTechnologyStepRunId == id)
                .Select(x => x.TechnologyStepParameterId)
                .ToListAsync(cancellationToken);

            var hasAllRequiredMeasurements = requiredParameterIds.All(measuredParameterIds.Contains);
            if (!hasAllRequiredMeasurements)
            {
                return BadRequest(ApiResponse.Fail("Нельзя завершить шаг, пока не заполнены обязательные параметры."));
            }
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

        var stepRun = await dbContext.BatchTechnologyStepRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (stepRun is null)
        {
            return NotFound(ApiResponse.Fail("Шаг партии не найден."));
        }

        if (stepRun.Status == 3)
        {
            return BadRequest(ApiResponse.Fail("Нельзя записать факт в уже завершенный шаг."));
        }

        var metadata = await dbContext.TechnologyStepParameters
            .AsNoTracking()
            .Where(x => x.Id == request.TechnologyStepParameterId)
            .Select(x => new ParameterMetadata(
                x.TechnologyStepId,
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

        if (metadata.TechnologyStepId != stepRun.TechnologyStepId)
        {
            return BadRequest(ApiResponse.Fail("Параметр не относится к выбранному шагу."));
        }

        var hasMeasurement = await dbContext.BatchStepMeasuredValues
            .AsNoTracking()
            .AnyAsync(x => x.BatchTechnologyStepRunId == id && x.TechnologyStepParameterId == request.TechnologyStepParameterId, cancellationToken);

        if (hasMeasurement)
        {
            return BadRequest(ApiResponse.Fail("По этому параметру уже записано фактическое значение."));
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

        var batchExists = await dbContext.ProductionBatches
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductionBatchId, cancellationToken);

        if (!batchExists)
        {
            return BadRequest(ApiResponse.Fail("Партия не найдена."));
        }

        if (request.BatchTechnologyStepRunId is not null)
        {
            var stepRunMatchesBatch = await dbContext.BatchTechnologyStepRuns
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.BatchTechnologyStepRunId.Value && x.ProductionBatchId == request.ProductionBatchId, cancellationToken);

            if (!stepRunMatchesBatch)
            {
                return BadRequest(ApiResponse.Fail("Шаг не относится к выбранной партии."));
            }
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
        int TechnologyStepId,
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

public sealed record DeviationItem(
    int Id,
    int ProductionBatchId,
    string BatchNumber,
    int? BatchTechnologyStepRunId,
    int? StepOrder,
    string? StepTitle,
    string Title,
    string? ParameterName,
    string? PlannedValue,
    string? ActualValue,
    int Severity,
    string? Details,
    DateTime CreatedAt,
    string? ReportedByName);

public sealed record DeviationResponse(int Id);
