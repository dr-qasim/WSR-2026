using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/laboratory")]
public sealed class LaboratoryController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpGet("specifications")]
    public async Task<IActionResult> GetSpecifications(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                qs.id,
                qs.code,
                qs.name,
                qs.subject_type,
                qs.raw_material_id,
                rm.name AS raw_material_name,
                qs.product_id,
                p.name AS product_name,
                qs.version_number,
                qs.status,
                qs.is_active
            FROM quality_specifications qs
            LEFT JOIN raw_materials rm ON rm.id = qs.raw_material_id
            LEFT JOIN products p ON p.id = qs.product_id
            ORDER BY qs.name;
            """;

        var items = new List<QualitySpecificationItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new QualitySpecificationItem(
                reader.GetInt32("id"),
                reader.GetString("code"),
                reader.GetString("name"),
                reader.GetInt32("subject_type"),
                reader.GetNullableInt32("raw_material_id"),
                reader.GetNullableString("raw_material_name"),
                reader.GetNullableInt32("product_id"),
                reader.GetNullableString("product_name"),
                reader.GetInt32("version_number"),
                reader.GetInt32("status"),
                reader.GetBoolean("is_active")));
        }

        return Ok(ApiResponse<List<QualitySpecificationItem>>.Ok(items));
    }

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                lt.id,
                lt.test_number,
                lt.subject_type,
                lt.raw_material_lot_id,
                lot.internal_lot_number,
                lt.production_batch_id,
                batch.batch_number,
                lt.quality_specification_id,
                qs.name AS quality_specification_name,
                lt.test_kind,
                lt.status,
                lt.priority,
                lt.comment,
                lt.result_summary,
                lt.created_at,
                lt.assigned_at,
                lt.started_at,
                lt.completed_at,
                tester.full_name AS tester_name
            FROM laboratory_tests lt
            JOIN quality_specifications qs ON qs.id = lt.quality_specification_id
            LEFT JOIN raw_material_lots lot ON lot.id = lt.raw_material_lot_id
            LEFT JOIN production_batches batch ON batch.id = lt.production_batch_id
            LEFT JOIN app_users tester ON tester.id = lt.tester_user_id
            ORDER BY lt.created_at DESC, lt.id DESC;
            """;

        var items = new List<LaboratoryTestItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LaboratoryTestItem(
                reader.GetInt32("id"),
                reader.GetString("test_number"),
                reader.GetInt32("subject_type"),
                reader.GetNullableInt32("raw_material_lot_id"),
                reader.GetNullableString("internal_lot_number"),
                reader.GetNullableInt32("production_batch_id"),
                reader.GetNullableString("batch_number"),
                reader.GetInt32("quality_specification_id"),
                reader.GetString("quality_specification_name"),
                reader.GetString("test_kind"),
                reader.GetInt32("status"),
                reader.GetInt32("priority"),
                reader.GetNullableString("comment"),
                reader.GetNullableString("result_summary"),
                reader.GetDateTime("created_at"),
                reader.GetNullableDateTime("assigned_at"),
                reader.GetNullableDateTime("started_at"),
                reader.GetNullableDateTime("completed_at"),
                reader.GetNullableString("tester_name")));
        }

        return Ok(ApiResponse<List<LaboratoryTestItem>>.Ok(items));
    }

    [HttpGet("tests/{id:int}")]
    public async Task<IActionResult> GetTest(int id, CancellationToken cancellationToken)
    {
        const string testSql = """
            SELECT
                lt.id,
                lt.test_number,
                lt.subject_type,
                lt.raw_material_lot_id,
                lot.internal_lot_number,
                lt.production_batch_id,
                batch.batch_number,
                lt.quality_specification_id,
                qs.name AS quality_specification_name,
                lt.test_kind,
                lt.status,
                lt.priority,
                lt.comment,
                lt.result_summary,
                lt.created_at,
                lt.assigned_at,
                lt.started_at,
                lt.completed_at,
                tester.full_name AS tester_name
            FROM laboratory_tests lt
            JOIN quality_specifications qs ON qs.id = lt.quality_specification_id
            LEFT JOIN raw_material_lots lot ON lot.id = lt.raw_material_lot_id
            LEFT JOIN production_batches batch ON batch.id = lt.production_batch_id
            LEFT JOIN app_users tester ON tester.id = lt.tester_user_id
            WHERE lt.id = @id
            LIMIT 1;
            """;

        const string resultsSql = """
            SELECT
                id,
                sort_order,
                parameter_name,
                value_type,
                unit,
                min_numeric_value,
                max_numeric_value,
                target_text_value,
                target_boolean_value,
                actual_numeric_value,
                actual_text_value,
                actual_boolean_value,
                is_required,
                is_within_range,
                comment
            FROM laboratory_test_parameter_results
            WHERE laboratory_test_id = @id
            ORDER BY sort_order;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        LaboratoryTestItem? test = null;
        await using (var testCommand = new NpgsqlCommand(testSql, connection))
        {
            testCommand.Parameters.AddWithValue("id", id);
            await using var reader = await testCommand.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                test = new LaboratoryTestItem(
                    reader.GetInt32("id"),
                    reader.GetString("test_number"),
                    reader.GetInt32("subject_type"),
                    reader.GetNullableInt32("raw_material_lot_id"),
                    reader.GetNullableString("internal_lot_number"),
                    reader.GetNullableInt32("production_batch_id"),
                    reader.GetNullableString("batch_number"),
                    reader.GetInt32("quality_specification_id"),
                    reader.GetString("quality_specification_name"),
                    reader.GetString("test_kind"),
                    reader.GetInt32("status"),
                    reader.GetInt32("priority"),
                    reader.GetNullableString("comment"),
                    reader.GetNullableString("result_summary"),
                    reader.GetDateTime("created_at"),
                    reader.GetNullableDateTime("assigned_at"),
                    reader.GetNullableDateTime("started_at"),
                    reader.GetNullableDateTime("completed_at"),
                    reader.GetNullableString("tester_name"));
            }
        }

        if (test is null)
        {
            return NotFound(ApiResponse.Fail("Лабораторное испытание не найдено."));
        }

        var results = new List<LaboratoryTestParameterResultItem>();
        await using (var resultsCommand = new NpgsqlCommand(resultsSql, connection))
        {
            resultsCommand.Parameters.AddWithValue("id", id);
            await using var reader = await resultsCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new LaboratoryTestParameterResultItem(
                    reader.GetInt32("id"),
                    reader.GetInt32("sort_order"),
                    reader.GetString("parameter_name"),
                    reader.GetInt32("value_type"),
                    reader.GetNullableString("unit"),
                    reader.GetNullableDecimal("min_numeric_value"),
                    reader.GetNullableDecimal("max_numeric_value"),
                    reader.GetNullableString("target_text_value"),
                    reader.GetNullableBoolean("target_boolean_value"),
                    reader.GetNullableDecimal("actual_numeric_value"),
                    reader.GetNullableString("actual_text_value"),
                    reader.GetNullableBoolean("actual_boolean_value"),
                    reader.GetBoolean("is_required"),
                    reader.GetNullableBoolean("is_within_range"),
                    reader.GetNullableString("comment")));
            }
        }

        return Ok(ApiResponse<LaboratoryTestDetail>.Ok(new LaboratoryTestDetail(test, results)));
    }

    [HttpPost("tests")]
    public async Task<IActionResult> CreateTest([FromBody] CreateLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.SubjectType is < 1 or > 2 || request.QualitySpecificationId <= 0 || string.IsNullOrWhiteSpace(request.TestKind))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные лабораторного испытания."));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var testNumber = $"TEST-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            const string insertTestSql = """
                INSERT INTO laboratory_tests
                    (test_number, subject_type, raw_material_lot_id, production_batch_id, quality_specification_id,
                     test_kind, status, priority, comment, created_at, assigned_at, tester_user_id)
                VALUES
                    (@testNumber, @subjectType, @rawMaterialLotId, @productionBatchId, @qualitySpecificationId,
                     @testKind, 1, @priority, @comment, @createdAt, @assignedAt, @testerUserId)
                RETURNING id;
                """;

            int testId;
            await using (var insertTestCommand = new NpgsqlCommand(insertTestSql, connection, transaction))
            {
                insertTestCommand.Parameters.AddWithValue("testNumber", testNumber);
                insertTestCommand.Parameters.AddWithValue("subjectType", request.SubjectType);
                insertTestCommand.Parameters.AddWithValue("rawMaterialLotId", (object?)request.RawMaterialLotId ?? DBNull.Value);
                insertTestCommand.Parameters.AddWithValue("productionBatchId", (object?)request.ProductionBatchId ?? DBNull.Value);
                insertTestCommand.Parameters.AddWithValue("qualitySpecificationId", request.QualitySpecificationId);
                insertTestCommand.Parameters.AddWithValue("testKind", request.TestKind.Trim());
                insertTestCommand.Parameters.AddWithValue("priority", request.Priority);
                insertTestCommand.Parameters.AddWithValue("comment", (object?)request.Comment ?? DBNull.Value);
                insertTestCommand.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
                insertTestCommand.Parameters.AddWithValue("assignedAt", request.TesterUserId is null ? DBNull.Value : DateTime.UtcNow);
                insertTestCommand.Parameters.AddWithValue("testerUserId", (object?)request.TesterUserId ?? DBNull.Value);
                testId = Convert.ToInt32(await insertTestCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string copyParametersSql = """
                INSERT INTO laboratory_test_parameter_results
                    (laboratory_test_id, quality_specification_parameter_id, sort_order, parameter_name, value_type, unit,
                     min_numeric_value, max_numeric_value, target_text_value, target_boolean_value, is_required)
                SELECT
                    @laboratoryTestId,
                    qsp.id,
                    qsp.sort_order,
                    qsp.name,
                    qsp.value_type,
                    qsp.unit,
                    qsp.min_numeric_value,
                    qsp.max_numeric_value,
                    qsp.target_text_value,
                    qsp.target_boolean_value,
                    qsp.is_required
                FROM quality_specification_parameters qsp
                WHERE qsp.quality_specification_id = @qualitySpecificationId
                ORDER BY qsp.sort_order;
                """;

            await using (var copyParametersCommand = new NpgsqlCommand(copyParametersSql, connection, transaction))
            {
                copyParametersCommand.Parameters.AddWithValue("laboratoryTestId", testId);
                copyParametersCommand.Parameters.AddWithValue("qualitySpecificationId", request.QualitySpecificationId);
                await copyParametersCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse<CreateLaboratoryTestResponse>.Ok(
                new CreateLaboratoryTestResponse(testId, testNumber),
                "Лабораторное испытание создано."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("tests/{id:int}/start")]
    public async Task<IActionResult> StartTest(int id, [FromBody] StartLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.TesterUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать лаборанта."));
        }

        const string sql = """
            UPDATE laboratory_tests
            SET status = 2,
                tester_user_id = @testerUserId,
                started_at = COALESCE(started_at, @startedAt),
                assigned_at = COALESCE(assigned_at, @startedAt)
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("testerUserId", request.TesterUserId);
        command.Parameters.AddWithValue("startedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("id", id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound(ApiResponse.Fail("Испытание не найдено."));
            }

            return Ok(ApiResponse.Ok("Испытание начато."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("tests/{id:int}/results")]
    public async Task<IActionResult> SaveResults(int id, [FromBody] SaveLaboratoryResultsRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно передать хотя бы один результат испытания."));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string metadataSql = """
                SELECT
                    id,
                    laboratory_test_id,
                    value_type,
                    min_numeric_value,
                    max_numeric_value,
                    target_text_value,
                    target_boolean_value
                FROM laboratory_test_parameter_results
                WHERE id = @id
                LIMIT 1;
                """;

            const string updateSql = """
                UPDATE laboratory_test_parameter_results
                SET actual_numeric_value = @actualNumericValue,
                    actual_text_value = @actualTextValue,
                    actual_boolean_value = @actualBooleanValue,
                    is_within_range = @isWithinRange,
                    comment = @comment
                WHERE id = @id
                  AND laboratory_test_id = @laboratoryTestId;
                """;

            foreach (var item in request.Items)
            {
                var actualValueCount = CountActualValues(item.ActualNumericValue, item.ActualTextValue, item.ActualBooleanValue);
                if (actualValueCount != 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(ApiResponse.Fail("Для каждого результата нужно передать ровно одно фактическое значение."));
                }

                LaboratoryParameterMetadata? metadata = null;
                await using (var metadataCommand = new NpgsqlCommand(metadataSql, connection, transaction))
                {
                    metadataCommand.Parameters.AddWithValue("id", item.ParameterResultId);
                    await using var reader = await metadataCommand.ExecuteReaderAsync(cancellationToken);

                    if (await reader.ReadAsync(cancellationToken))
                    {
                        metadata = new LaboratoryParameterMetadata(
                            reader.GetInt32("laboratory_test_id"),
                            reader.GetInt32("value_type"),
                            reader.GetNullableDecimal("min_numeric_value"),
                            reader.GetNullableDecimal("max_numeric_value"),
                            reader.GetNullableString("target_text_value"),
                            reader.GetNullableBoolean("target_boolean_value"));
                    }
                }

                if (metadata is null || metadata.LaboratoryTestId != id)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return BadRequest(ApiResponse.Fail("Результат не относится к выбранному испытанию."));
                }

                var isWithinRange = CalculateResult(metadata, item);

                await using var updateCommand = new NpgsqlCommand(updateSql, connection, transaction);
                updateCommand.Parameters.AddWithValue("actualNumericValue", (object?)item.ActualNumericValue ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("actualTextValue", (object?)item.ActualTextValue ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("actualBooleanValue", (object?)item.ActualBooleanValue ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("isWithinRange", isWithinRange);
                updateCommand.Parameters.AddWithValue("comment", (object?)item.Comment ?? DBNull.Value);
                updateCommand.Parameters.AddWithValue("id", item.ParameterResultId);
                updateCommand.Parameters.AddWithValue("laboratoryTestId", id);
                await updateCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Результаты испытания сохранены."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("tests/{id:int}/complete")]
    public async Task<IActionResult> CompleteTest(int id, [FromBody] CompleteLaboratoryTestRequest request, CancellationToken cancellationToken)
    {
        if (request.TesterUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать лаборанта."));
        }

        const string sql = """
            UPDATE laboratory_tests
            SET status = 3,
                tester_user_id = @testerUserId,
                started_at = COALESCE(started_at, @completedAt),
                completed_at = @completedAt,
                result_summary = @resultSummary
            WHERE id = @id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("testerUserId", request.TesterUserId);
        command.Parameters.AddWithValue("completedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("resultSummary", (object?)request.ResultSummary ?? DBNull.Value);
        command.Parameters.AddWithValue("id", id);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return NotFound(ApiResponse.Fail("Испытание не найдено."));
            }

            return Ok(ApiResponse.Ok("Испытание завершено."));
        }
        catch (PostgresException exception)
        {
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("decisions")]
    public async Task<IActionResult> CreateDecision([FromBody] CreateQualityDecisionRequest request, CancellationToken cancellationToken)
    {
        if (request.LaboratoryTestId <= 0 || request.DecidedByUserId <= 0 || string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(ApiResponse.Fail("Некорректные данные решения по качеству."));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string subjectSql = """
                SELECT subject_type, raw_material_lot_id, production_batch_id
                FROM laboratory_tests
                WHERE id = @id
                LIMIT 1;
                """;

            DecisionSubject? subject = null;
            await using (var subjectCommand = new NpgsqlCommand(subjectSql, connection, transaction))
            {
                subjectCommand.Parameters.AddWithValue("id", request.LaboratoryTestId);
                await using var reader = await subjectCommand.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    subject = new DecisionSubject(
                        reader.GetInt32("subject_type"),
                        reader.GetNullableInt32("raw_material_lot_id"),
                        reader.GetNullableInt32("production_batch_id"));
                }
            }

            if (subject is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(ApiResponse.Fail("Испытание не найдено."));
            }

            const string resetCurrentSql = """
                UPDATE quality_decisions
                SET is_current = FALSE
                WHERE subject_type = @subjectType
                  AND ((@rawMaterialLotId IS NOT NULL AND raw_material_lot_id = @rawMaterialLotId)
                       OR (@productionBatchId IS NOT NULL AND production_batch_id = @productionBatchId))
                  AND is_current = TRUE;
                """;

            await using (var resetCurrentCommand = new NpgsqlCommand(resetCurrentSql, connection, transaction))
            {
                resetCurrentCommand.Parameters.AddWithValue("subjectType", subject.SubjectType);
                resetCurrentCommand.Parameters.AddWithValue("rawMaterialLotId", (object?)subject.RawMaterialLotId ?? DBNull.Value);
                resetCurrentCommand.Parameters.AddWithValue("productionBatchId", (object?)subject.ProductionBatchId ?? DBNull.Value);
                await resetCurrentCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            const string insertDecisionSql = """
                INSERT INTO quality_decisions
                    (subject_type, raw_material_lot_id, production_batch_id, laboratory_test_id, decision_status,
                     comment, block_reason, is_current, decided_at, decided_by_user_id)
                VALUES
                    (@subjectType, @rawMaterialLotId, @productionBatchId, @laboratoryTestId, @decisionStatus,
                     @comment, @blockReason, TRUE, @decidedAt, @decidedByUserId)
                RETURNING id;
                """;

            int decisionId;
            await using (var insertDecisionCommand = new NpgsqlCommand(insertDecisionSql, connection, transaction))
            {
                insertDecisionCommand.Parameters.AddWithValue("subjectType", subject.SubjectType);
                insertDecisionCommand.Parameters.AddWithValue("rawMaterialLotId", (object?)subject.RawMaterialLotId ?? DBNull.Value);
                insertDecisionCommand.Parameters.AddWithValue("productionBatchId", (object?)subject.ProductionBatchId ?? DBNull.Value);
                insertDecisionCommand.Parameters.AddWithValue("laboratoryTestId", request.LaboratoryTestId);
                insertDecisionCommand.Parameters.AddWithValue("decisionStatus", request.DecisionStatus);
                insertDecisionCommand.Parameters.AddWithValue("comment", request.Comment.Trim());
                insertDecisionCommand.Parameters.AddWithValue("blockReason", (object?)request.BlockReason ?? DBNull.Value);
                insertDecisionCommand.Parameters.AddWithValue("decidedAt", DateTime.UtcNow);
                insertDecisionCommand.Parameters.AddWithValue("decidedByUserId", request.DecidedByUserId);
                decisionId = Convert.ToInt32(await insertDecisionCommand.ExecuteScalarAsync(cancellationToken));
            }

            if (subject.RawMaterialLotId is not null)
            {
                const string updateLotSql = """
                    UPDATE raw_material_lots
                    SET last_laboratory_decision_at = @decidedAt
                    WHERE id = @id;
                    """;

                await using var updateLotCommand = new NpgsqlCommand(updateLotSql, connection, transaction);
                updateLotCommand.Parameters.AddWithValue("decidedAt", DateTime.UtcNow);
                updateLotCommand.Parameters.AddWithValue("id", subject.RawMaterialLotId.Value);
                await updateLotCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse<CreateQualityDecisionResponse>.Ok(
                new CreateQualityDecisionResponse(decisionId),
                "Решение по качеству принято."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
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
