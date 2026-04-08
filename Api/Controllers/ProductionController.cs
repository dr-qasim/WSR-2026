using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/production")]
public sealed class ProductionController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                po.id,
                po.order_number,
                po.product_id,
                p.name AS product_name,
                po.production_line_id,
                pl.name AS production_line_name,
                po.planned_quantity,
                po.planned_start_at,
                po.status,
                po.created_at,
                u.full_name AS created_by_name
            FROM production_orders po
            JOIN products p ON p.id = po.product_id
            JOIN production_lines pl ON pl.id = po.production_line_id
            JOIN app_users u ON u.id = po.created_by_user_id
            ORDER BY po.created_at DESC, po.id DESC;
            """;

        var items = new List<ProductionOrderItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductionOrderItem(
                reader.GetInt32("id"),
                reader.GetString("order_number"),
                reader.GetInt32("product_id"),
                reader.GetString("product_name"),
                reader.GetInt32("production_line_id"),
                reader.GetString("production_line_name"),
                reader.GetDecimal("planned_quantity"),
                reader.GetNullableDateTime("planned_start_at"),
                reader.GetInt32("status"),
                reader.GetDateTime("created_at"),
                reader.GetString("created_by_name")));
        }

        return Ok(ApiResponse<List<ProductionOrderItem>>.Ok(items));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.ProductionLineId <= 0 || request.CreatedByUserId <= 0 || request.PlannedQuantity <= 0)
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные производственного заказа."));
        }

        const string sql = """
            INSERT INTO production_orders
                (order_number, product_id, production_line_id, planned_quantity, planned_start_at, status, created_at, created_by_user_id)
            VALUES
                (@orderNumber, @productId, @productionLineId, @plannedQuantity, @plannedStartAt, 1, @createdAt, @createdByUserId)
            RETURNING id;
            """;

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("orderNumber", orderNumber);
        command.Parameters.AddWithValue("productId", request.ProductId);
        command.Parameters.AddWithValue("productionLineId", request.ProductionLineId);
        command.Parameters.AddWithValue("plannedQuantity", request.PlannedQuantity);
        command.Parameters.AddWithValue("plannedStartAt", (object?)request.PlannedStartAt ?? DBNull.Value);
        command.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("createdByUserId", request.CreatedByUserId);

        try
        {
            var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return Ok(ApiResponse<CreateProductionOrderResponse>.Ok(
                new CreateProductionOrderResponse(id, orderNumber),
                "Производственный заказ создан."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pb.id,
                pb.batch_number,
                pb.production_order_id,
                po.order_number,
                pb.product_id,
                p.name AS product_name,
                pb.production_line_id,
                pl.name AS production_line_name,
                pb.recipe_version_id,
                pb.technology_card_id,
                pb.planned_quantity,
                pb.status,
                pb.started_at,
                pb.completed_at
            FROM production_batches pb
            JOIN products p ON p.id = pb.product_id
            JOIN production_lines pl ON pl.id = pb.production_line_id
            LEFT JOIN production_orders po ON po.id = pb.production_order_id
            ORDER BY pb.id DESC;
            """;

        var items = new List<ProductionBatchItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductionBatchItem(
                reader.GetInt32("id"),
                reader.GetString("batch_number"),
                reader.GetNullableInt32("production_order_id"),
                reader.GetNullableString("order_number"),
                reader.GetInt32("product_id"),
                reader.GetString("product_name"),
                reader.GetInt32("production_line_id"),
                reader.GetString("production_line_name"),
                reader.GetInt32("recipe_version_id"),
                reader.GetInt32("technology_card_id"),
                reader.GetDecimal("planned_quantity"),
                reader.GetInt32("status"),
                reader.GetNullableDateTime("started_at"),
                reader.GetNullableDateTime("completed_at")));
        }

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

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var batchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            const string insertBatchSql = """
                INSERT INTO production_batches
                    (batch_number, production_order_id, product_id, recipe_version_id, technology_card_id, production_line_id,
                     extruder_program_id, planned_quantity, status)
                VALUES
                    (@batchNumber, @productionOrderId, @productId, @recipeVersionId, @technologyCardId, @productionLineId,
                     @extruderProgramId, @plannedQuantity, 1)
                RETURNING id;
                """;

            int batchId;
            await using (var insertBatchCommand = new NpgsqlCommand(insertBatchSql, connection, transaction))
            {
                insertBatchCommand.Parameters.AddWithValue("batchNumber", batchNumber);
                insertBatchCommand.Parameters.AddWithValue("productionOrderId", (object?)request.ProductionOrderId ?? DBNull.Value);
                insertBatchCommand.Parameters.AddWithValue("productId", request.ProductId);
                insertBatchCommand.Parameters.AddWithValue("recipeVersionId", request.RecipeVersionId);
                insertBatchCommand.Parameters.AddWithValue("technologyCardId", request.TechnologyCardId);
                insertBatchCommand.Parameters.AddWithValue("productionLineId", request.ProductionLineId);
                insertBatchCommand.Parameters.AddWithValue("extruderProgramId", (object?)request.ExtruderProgramId ?? DBNull.Value);
                insertBatchCommand.Parameters.AddWithValue("plannedQuantity", request.PlannedQuantity);
                batchId = Convert.ToInt32(await insertBatchCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string insertConsumptionSql = """
                INSERT INTO batch_raw_material_consumptions
                    (production_batch_id, raw_material_lot_id, quantity_used)
                VALUES
                    (@productionBatchId, @rawMaterialLotId, @quantityUsed);
                """;

            const string updateLotSql = """
                UPDATE raw_material_lots
                SET quantity_available = quantity_available - @quantityUsed
                WHERE id = @rawMaterialLotId;
                """;

            foreach (var consumption in request.Consumptions)
            {
                await using (var insertConsumptionCommand = new NpgsqlCommand(insertConsumptionSql, connection, transaction))
                {
                    insertConsumptionCommand.Parameters.AddWithValue("productionBatchId", batchId);
                    insertConsumptionCommand.Parameters.AddWithValue("rawMaterialLotId", consumption.RawMaterialLotId);
                    insertConsumptionCommand.Parameters.AddWithValue("quantityUsed", consumption.QuantityUsed);
                    await insertConsumptionCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var updateLotCommand = new NpgsqlCommand(updateLotSql, connection, transaction))
                {
                    updateLotCommand.Parameters.AddWithValue("quantityUsed", consumption.QuantityUsed);
                    updateLotCommand.Parameters.AddWithValue("rawMaterialLotId", consumption.RawMaterialLotId);
                    await updateLotCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            const string insertStepRunsSql = """
                INSERT INTO batch_technology_step_runs
                    (production_batch_id, technology_step_id, status)
                SELECT
                    @productionBatchId,
                    id,
                    1
                FROM technology_steps
                WHERE technology_card_id = @technologyCardId
                ORDER BY step_order;
                """;

            await using (var insertStepRunsCommand = new NpgsqlCommand(insertStepRunsSql, connection, transaction))
            {
                insertStepRunsCommand.Parameters.AddWithValue("productionBatchId", batchId);
                insertStepRunsCommand.Parameters.AddWithValue("technologyCardId", request.TechnologyCardId);
                await insertStepRunsCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse<CreateProductionBatchResponse>.Ok(
                new CreateProductionBatchResponse(batchId, batchNumber),
                "Производственная партия создана."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpGet("batches/{id:int}/steps")]
    public async Task<IActionResult> GetBatchSteps(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                run.id,
                run.production_batch_id,
                run.technology_step_id,
                step.step_order,
                step.step_type,
                step.title,
                run.status,
                run.started_at,
                run.completed_at,
                run.comment
            FROM batch_technology_step_runs run
            JOIN technology_steps step ON step.id = run.technology_step_id
            WHERE run.production_batch_id = @batchId
            ORDER BY step.step_order;
            """;

        var items = new List<BatchStepRunItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("batchId", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new BatchStepRunItem(
                reader.GetInt32("id"),
                reader.GetInt32("production_batch_id"),
                reader.GetInt32("technology_step_id"),
                reader.GetInt32("step_order"),
                reader.GetInt32("step_type"),
                reader.GetString("title"),
                reader.GetInt32("status"),
                reader.GetNullableDateTime("started_at"),
                reader.GetNullableDateTime("completed_at"),
                reader.GetNullableString("comment")));
        }

        return Ok(ApiResponse<List<BatchStepRunItem>>.Ok(items));
    }

    [HttpPost("batches/{id:int}/start")]
    public async Task<IActionResult> StartBatch(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE production_batches
            SET status = 2,
                started_at = COALESCE(started_at, @startedAt)
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("startedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("id", id);
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows == 0)
        {
            return NotFound(ApiResponse.Fail("Партия не найдена."));
        }

        return Ok(ApiResponse.Ok("Партия переведена в работу."));
    }

    [HttpPost("batches/{id:int}/complete")]
    public async Task<IActionResult> CompleteBatch(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE production_batches
            SET status = 6,
                completed_at = @completedAt
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("completedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("id", id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound(ApiResponse.Fail("Партия не найдена."));
            }

            return Ok(ApiResponse.Ok("Партия завершена."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("step-runs/{id:int}/start")]
    public async Task<IActionResult> StartStepRun(int id, [FromBody] StartStepRunRequest request, CancellationToken cancellationToken)
    {
        if (request.StartedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который начал шаг."));
        }

        const string sql = """
            UPDATE batch_technology_step_runs
            SET status = 2,
                started_at = COALESCE(started_at, @startedAt),
                started_by_user_id = @startedByUserId,
                comment = COALESCE(@comment, comment)
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("startedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("startedByUserId", request.StartedByUserId);
        command.Parameters.AddWithValue("comment", (object?)request.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("id", id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound(ApiResponse.Fail("Шаг партии не найден."));
            }

            return Ok(ApiResponse.Ok("Шаг партии начат."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("step-runs/{id:int}/complete")]
    public async Task<IActionResult> CompleteStepRun(int id, [FromBody] CompleteStepRunRequest request, CancellationToken cancellationToken)
    {
        if (request.CompletedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который завершил шаг."));
        }

        const string sql = """
            UPDATE batch_technology_step_runs
            SET status = 3,
                completed_at = @completedAt,
                completed_by_user_id = @completedByUserId,
                comment = COALESCE(@comment, comment)
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("completedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("completedByUserId", request.CompletedByUserId);
        command.Parameters.AddWithValue("comment", (object?)request.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("id", id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound(ApiResponse.Fail("Шаг партии не найден."));
            }

            return Ok(ApiResponse.Ok("Шаг партии завершен."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
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

        const string parameterSql = """
            SELECT
                value_type,
                target_numeric_value,
                min_numeric_value,
                max_numeric_value,
                target_text_value,
                target_boolean_value
            FROM technology_step_parameters
            WHERE id = @id
            LIMIT 1;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        ParameterMetadata? metadata = null;
        await using (var parameterCommand = new NpgsqlCommand(parameterSql, connection))
        {
            parameterCommand.Parameters.AddWithValue("id", request.TechnologyStepParameterId);
            await using var reader = await parameterCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                metadata = new ParameterMetadata(
                    reader.GetInt32("value_type"),
                    reader.GetNullableDecimal("target_numeric_value"),
                    reader.GetNullableDecimal("min_numeric_value"),
                    reader.GetNullableDecimal("max_numeric_value"),
                    reader.GetNullableString("target_text_value"),
                    reader.GetNullableBoolean("target_boolean_value"));
            }
        }

        if (metadata is null)
        {
            return NotFound(ApiResponse.Fail("Параметр технологического шага не найден."));
        }

        var isWithinTolerance = CalculateMeasurementResult(metadata, request);

        const string insertSql = """
            INSERT INTO batch_step_measured_values
                (batch_technology_step_run_id, technology_step_parameter_id, actual_numeric_value, actual_text_value,
                 actual_boolean_value, is_within_tolerance, recorded_at, comment)
            VALUES
                (@batchTechnologyStepRunId, @technologyStepParameterId, @actualNumericValue, @actualTextValue,
                 @actualBooleanValue, @isWithinTolerance, @recordedAt, @comment)
            RETURNING id;
            """;

        try
        {
            await using var command = new NpgsqlCommand(insertSql, connection);
            command.Parameters.AddWithValue("batchTechnologyStepRunId", id);
            command.Parameters.AddWithValue("technologyStepParameterId", request.TechnologyStepParameterId);
            command.Parameters.AddWithValue("actualNumericValue", (object?)request.ActualNumericValue ?? DBNull.Value);
            command.Parameters.AddWithValue("actualTextValue", (object?)request.ActualTextValue ?? DBNull.Value);
            command.Parameters.AddWithValue("actualBooleanValue", (object?)request.ActualBooleanValue ?? DBNull.Value);
            command.Parameters.AddWithValue("isWithinTolerance", isWithinTolerance);
            command.Parameters.AddWithValue("recordedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("comment", (object?)request.Comment ?? DBNull.Value);

            var measurementId = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

            return Ok(ApiResponse<MeasurementResponse>.Ok(
                new MeasurementResponse(measurementId, isWithinTolerance),
                "Фактическое значение записано."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("deviations")]
    public async Task<IActionResult> CreateDeviation([FromBody] CreateDeviationRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductionBatchId <= 0 || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные отклонения."));
        }

        const string sql = """
            INSERT INTO process_deviations
                (production_batch_id, batch_technology_step_run_id, title, parameter_name, planned_value, actual_value,
                 severity, details, created_at, reported_by_user_id)
            VALUES
                (@productionBatchId, @batchTechnologyStepRunId, @title, @parameterName, @plannedValue, @actualValue,
                 @severity, @details, @createdAt, @reportedByUserId)
            RETURNING id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("productionBatchId", request.ProductionBatchId);
        command.Parameters.AddWithValue("batchTechnologyStepRunId", (object?)request.BatchTechnologyStepRunId ?? DBNull.Value);
        command.Parameters.AddWithValue("title", request.Title.Trim());
        command.Parameters.AddWithValue("parameterName", (object?)request.ParameterName ?? DBNull.Value);
        command.Parameters.AddWithValue("plannedValue", (object?)request.PlannedValue ?? DBNull.Value);
        command.Parameters.AddWithValue("actualValue", (object?)request.ActualValue ?? DBNull.Value);
        command.Parameters.AddWithValue("severity", request.Severity);
        command.Parameters.AddWithValue("details", (object?)request.Details ?? DBNull.Value);
        command.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("reportedByUserId", (object?)request.ReportedByUserId ?? DBNull.Value);

        try
        {
            var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return Ok(ApiResponse<DeviationResponse>.Ok(
                new DeviationResponse(id),
                "Отклонение зарегистрировано."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
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
