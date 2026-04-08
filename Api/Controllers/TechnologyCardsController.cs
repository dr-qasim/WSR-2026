using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/technology-cards")]
public sealed class TechnologyCardsController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTechnologyCards(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                tc.id,
                tc.product_id,
                p.name AS product_name,
                tc.version_number,
                tc.title,
                tc.description,
                tc.status,
                tc.is_active,
                tc.created_at,
                creator.full_name AS created_by_name,
                tc.approved_at,
                approver.full_name AS approved_by_name
            FROM technology_cards tc
            JOIN products p ON p.id = tc.product_id
            JOIN app_users creator ON creator.id = tc.created_by_user_id
            LEFT JOIN app_users approver ON approver.id = tc.approved_by_user_id
            ORDER BY p.name, tc.version_number DESC;
            """;

        var items = new List<TechnologyCardListItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TechnologyCardListItem(
                reader.GetInt32("id"),
                reader.GetInt32("product_id"),
                reader.GetString("product_name"),
                reader.GetInt32("version_number"),
                reader.GetString("title"),
                reader.GetNullableString("description"),
                reader.GetInt32("status"),
                reader.GetBoolean("is_active"),
                reader.GetDateTime("created_at"),
                reader.GetString("created_by_name"),
                reader.GetNullableDateTime("approved_at"),
                reader.GetNullableString("approved_by_name")));
        }

        return Ok(ApiResponse<List<TechnologyCardListItem>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTechnologyCard(int id, CancellationToken cancellationToken)
    {
        const string headerSql = """
            SELECT
                tc.id,
                tc.product_id,
                p.name AS product_name,
                tc.version_number,
                tc.title,
                tc.description,
                tc.status,
                tc.is_active,
                tc.created_at,
                creator.full_name AS created_by_name,
                tc.approved_at,
                approver.full_name AS approved_by_name
            FROM technology_cards tc
            JOIN products p ON p.id = tc.product_id
            JOIN app_users creator ON creator.id = tc.created_by_user_id
            LEFT JOIN app_users approver ON approver.id = tc.approved_by_user_id
            WHERE tc.id = @id
            LIMIT 1;
            """;

        const string stepsSql = """
            SELECT
                id,
                step_order,
                step_type,
                title,
                instruction,
                is_required,
                expected_duration_minutes
            FROM technology_steps
            WHERE technology_card_id = @id
            ORDER BY step_order;
            """;

        const string parametersSql = """
            SELECT
                id,
                technology_step_id,
                name,
                value_type,
                unit,
                target_numeric_value,
                min_numeric_value,
                max_numeric_value,
                target_text_value,
                target_boolean_value,
                is_required,
                comment
            FROM technology_step_parameters
            WHERE technology_step_id = ANY(@stepIds)
            ORDER BY technology_step_id, id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        TechnologyCardHeader? header = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                header = new TechnologyCardHeader(
                    reader.GetInt32("id"),
                    reader.GetInt32("product_id"),
                    reader.GetString("product_name"),
                    reader.GetInt32("version_number"),
                    reader.GetString("title"),
                    reader.GetNullableString("description"),
                    reader.GetInt32("status"),
                    reader.GetBoolean("is_active"),
                    reader.GetDateTime("created_at"),
                    reader.GetString("created_by_name"),
                    reader.GetNullableDateTime("approved_at"),
                    reader.GetNullableString("approved_by_name"));
            }
        }

        if (header is null)
        {
            return NotFound(ApiResponse.Fail("Технологическая карта не найдена."));
        }

        var steps = new List<TechnologyStepDetail>();
        var stepIds = new List<int>();

        await using (var stepsCommand = new NpgsqlCommand(stepsSql, connection))
        {
            stepsCommand.Parameters.AddWithValue("id", id);
            await using var reader = await stepsCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var stepId = reader.GetInt32("id");
                stepIds.Add(stepId);
                steps.Add(new TechnologyStepDetail(
                    stepId,
                    reader.GetInt32("step_order"),
                    reader.GetInt32("step_type"),
                    reader.GetString("title"),
                    reader.GetNullableString("instruction"),
                    reader.GetBoolean("is_required"),
                    reader.GetNullableInt32("expected_duration_minutes"),
                    new List<TechnologyStepParameterDetail>()));
            }
        }

        if (stepIds.Count > 0)
        {
            await using var parametersCommand = new NpgsqlCommand(parametersSql, connection);
            parametersCommand.Parameters.AddWithValue("stepIds", stepIds.ToArray());
            await using var reader = await parametersCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var step = steps.First(x => x.Id == reader.GetInt32("technology_step_id"));
                step.Parameters.Add(new TechnologyStepParameterDetail(
                    reader.GetInt32("id"),
                    reader.GetString("name"),
                    reader.GetInt32("value_type"),
                    reader.GetNullableString("unit"),
                    reader.GetNullableDecimal("target_numeric_value"),
                    reader.GetNullableDecimal("min_numeric_value"),
                    reader.GetNullableDecimal("max_numeric_value"),
                    reader.GetNullableString("target_text_value"),
                    reader.GetNullableBoolean("target_boolean_value"),
                    reader.GetBoolean("is_required"),
                    reader.GetNullableString("comment")));
            }
        }

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

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string nextVersionSql = """
                SELECT COALESCE(MAX(version_number), 0) + 1
                FROM technology_cards
                WHERE product_id = @productId;
                """;

            int versionNumber;
            await using (var nextVersionCommand = new NpgsqlCommand(nextVersionSql, connection, transaction))
            {
                nextVersionCommand.Parameters.AddWithValue("productId", request.ProductId);
                versionNumber = Convert.ToInt32(await nextVersionCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string insertCardSql = """
                INSERT INTO technology_cards
                    (product_id, version_number, title, description, status, is_active, created_at, created_by_user_id)
                VALUES
                    (@productId, @versionNumber, @title, @description, 1, FALSE, @createdAt, @createdByUserId)
                RETURNING id;
                """;

            int technologyCardId;
            await using (var insertCardCommand = new NpgsqlCommand(insertCardSql, connection, transaction))
            {
                insertCardCommand.Parameters.AddWithValue("productId", request.ProductId);
                insertCardCommand.Parameters.AddWithValue("versionNumber", versionNumber);
                insertCardCommand.Parameters.AddWithValue("title", request.Title.Trim());
                insertCardCommand.Parameters.AddWithValue("description", (object?)request.Description ?? DBNull.Value);
                insertCardCommand.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
                insertCardCommand.Parameters.AddWithValue("createdByUserId", request.CreatedByUserId);
                technologyCardId = Convert.ToInt32(await insertCardCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string insertStepSql = """
                INSERT INTO technology_steps
                    (technology_card_id, step_order, step_type, title, instruction, is_required, expected_duration_minutes)
                VALUES
                    (@technologyCardId, @stepOrder, @stepType, @title, @instruction, @isRequired, @expectedDurationMinutes)
                RETURNING id;
                """;

            const string insertParameterSql = """
                INSERT INTO technology_step_parameters
                    (technology_step_id, name, value_type, unit, target_numeric_value, min_numeric_value, max_numeric_value,
                     target_text_value, target_boolean_value, is_required, comment)
                VALUES
                    (@technologyStepId, @name, @valueType, @unit, @targetNumericValue, @minNumericValue, @maxNumericValue,
                     @targetTextValue, @targetBooleanValue, @isRequired, @comment);
                """;

            foreach (var step in request.Steps.OrderBy(x => x.StepOrder))
            {
                int stepId;
                await using (var insertStepCommand = new NpgsqlCommand(insertStepSql, connection, transaction))
                {
                    insertStepCommand.Parameters.AddWithValue("technologyCardId", technologyCardId);
                    insertStepCommand.Parameters.AddWithValue("stepOrder", step.StepOrder);
                    insertStepCommand.Parameters.AddWithValue("stepType", step.StepType);
                    insertStepCommand.Parameters.AddWithValue("title", step.Title.Trim());
                    insertStepCommand.Parameters.AddWithValue("instruction", (object?)step.Instruction ?? DBNull.Value);
                    insertStepCommand.Parameters.AddWithValue("isRequired", step.IsRequired);
                    insertStepCommand.Parameters.AddWithValue("expectedDurationMinutes", (object?)step.ExpectedDurationMinutes ?? DBNull.Value);
                    stepId = Convert.ToInt32(await insertStepCommand.ExecuteScalarAsync(cancellationToken));
                }

                foreach (var parameter in step.Parameters)
                {
                    await using var insertParameterCommand = new NpgsqlCommand(insertParameterSql, connection, transaction);
                    insertParameterCommand.Parameters.AddWithValue("technologyStepId", stepId);
                    insertParameterCommand.Parameters.AddWithValue("name", parameter.Name.Trim());
                    insertParameterCommand.Parameters.AddWithValue("valueType", parameter.ValueType);
                    insertParameterCommand.Parameters.AddWithValue("unit", (object?)parameter.Unit ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("targetNumericValue", (object?)parameter.TargetNumericValue ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("minNumericValue", (object?)parameter.MinNumericValue ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("maxNumericValue", (object?)parameter.MaxNumericValue ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("targetTextValue", (object?)parameter.TargetTextValue ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("targetBooleanValue", (object?)parameter.TargetBooleanValue ?? DBNull.Value);
                    insertParameterCommand.Parameters.AddWithValue("isRequired", parameter.IsRequired);
                    insertParameterCommand.Parameters.AddWithValue("comment", (object?)parameter.Comment ?? DBNull.Value);
                    await insertParameterCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse<CreateTechnologyCardResponse>.Ok(
                new CreateTechnologyCardResponse(technologyCardId, request.ProductId, versionNumber),
                "Черновик технологической карты создан."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveTechnologyCard(int id, [FromBody] ApproveTechnologyCardRequest request, CancellationToken cancellationToken)
    {
        if (request.ApprovedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который утверждает карту."));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string productSql = """
                SELECT product_id
                FROM technology_cards
                WHERE id = @id
                LIMIT 1;
                """;

            int? productId;
            await using (var productCommand = new NpgsqlCommand(productSql, connection, transaction))
            {
                productCommand.Parameters.AddWithValue("id", id);
                var value = await productCommand.ExecuteScalarAsync(cancellationToken);
                productId = value is null ? null : Convert.ToInt32(value);
            }

            if (productId is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return NotFound(ApiResponse.Fail("Технологическая карта не найдена."));
            }

            const string deactivateSql = """
                UPDATE technology_cards
                SET is_active = FALSE
                WHERE product_id = @productId
                  AND id <> @id
                  AND is_active = TRUE;
                """;

            await using (var deactivateCommand = new NpgsqlCommand(deactivateSql, connection, transaction))
            {
                deactivateCommand.Parameters.AddWithValue("productId", productId.Value);
                deactivateCommand.Parameters.AddWithValue("id", id);
                await deactivateCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            const string approveSql = """
                UPDATE technology_cards
                SET status = 3,
                    is_active = TRUE,
                    approved_at = @approvedAt,
                    approved_by_user_id = @approvedByUserId
                WHERE id = @id;
                """;

            await using (var approveCommand = new NpgsqlCommand(approveSql, connection, transaction))
            {
                approveCommand.Parameters.AddWithValue("approvedAt", DateTime.UtcNow);
                approveCommand.Parameters.AddWithValue("approvedByUserId", request.ApprovedByUserId);
                approveCommand.Parameters.AddWithValue("id", id);
                await approveCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            const string historySql = """
                INSERT INTO status_histories
                    (entity_type, entity_id, previous_status, new_status, changed_at, changed_by_user_id, comment)
                VALUES
                    ('TechnologyCard', @entityId, 'Draft', 'Approved', @changedAt, @changedByUserId, @comment);
                """;

            await using (var historyCommand = new NpgsqlCommand(historySql, connection, transaction))
            {
                historyCommand.Parameters.AddWithValue("entityId", id);
                historyCommand.Parameters.AddWithValue("changedAt", DateTime.UtcNow);
                historyCommand.Parameters.AddWithValue("changedByUserId", request.ApprovedByUserId);
                historyCommand.Parameters.AddWithValue("comment", (object?)request.Comment ?? "Технологическая карта утверждена через API.");
                await historyCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Технологическая карта утверждена."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
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
