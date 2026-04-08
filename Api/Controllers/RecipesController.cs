using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recipes")]
public sealed class RecipesController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRecipes(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rv.id,
                rv.product_id,
                p.name AS product_name,
                rv.version_number,
                rv.status,
                rv.is_active,
                rv.notes,
                rv.created_at,
                creator.full_name AS created_by_name,
                rv.approved_at,
                approver.full_name AS approved_by_name
            FROM recipe_versions rv
            JOIN products p ON p.id = rv.product_id
            JOIN app_users creator ON creator.id = rv.created_by_user_id
            LEFT JOIN app_users approver ON approver.id = rv.approved_by_user_id
            ORDER BY p.name, rv.version_number DESC;
            """;

        var items = new List<RecipeListItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new RecipeListItem(
                reader.GetInt32("id"),
                reader.GetInt32("product_id"),
                reader.GetString("product_name"),
                reader.GetInt32("version_number"),
                reader.GetInt32("status"),
                reader.GetBoolean("is_active"),
                reader.GetNullableString("notes"),
                reader.GetDateTime("created_at"),
                reader.GetString("created_by_name"),
                reader.GetNullableDateTime("approved_at"),
                reader.GetNullableString("approved_by_name")));
        }

        return Ok(ApiResponse<List<RecipeListItem>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRecipe(int id, CancellationToken cancellationToken)
    {
        const string headerSql = """
            SELECT
                rv.id,
                rv.product_id,
                p.name AS product_name,
                rv.version_number,
                rv.status,
                rv.is_active,
                rv.notes,
                rv.created_at,
                creator.full_name AS created_by_name,
                rv.approved_at,
                approver.full_name AS approved_by_name
            FROM recipe_versions rv
            JOIN products p ON p.id = rv.product_id
            JOIN app_users creator ON creator.id = rv.created_by_user_id
            LEFT JOIN app_users approver ON approver.id = rv.approved_by_user_id
            WHERE rv.id = @id
            LIMIT 1;
            """;

        const string componentsSql = """
            SELECT
                rc.id,
                rc.raw_material_id,
                rm.name AS raw_material_name,
                rc.percentage,
                rc.load_order,
                rc.allowed_deviation_percent
            FROM recipe_components rc
            JOIN raw_materials rm ON rm.id = rc.raw_material_id
            WHERE rc.recipe_version_id = @id
            ORDER BY rc.load_order;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        RecipeHeader? header = null;
        await using (var headerCommand = new NpgsqlCommand(headerSql, connection))
        {
            headerCommand.Parameters.AddWithValue("id", id);
            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                header = new RecipeHeader(
                    reader.GetInt32("id"),
                    reader.GetInt32("product_id"),
                    reader.GetString("product_name"),
                    reader.GetInt32("version_number"),
                    reader.GetInt32("status"),
                    reader.GetBoolean("is_active"),
                    reader.GetNullableString("notes"),
                    reader.GetDateTime("created_at"),
                    reader.GetString("created_by_name"),
                    reader.GetNullableDateTime("approved_at"),
                    reader.GetNullableString("approved_by_name"));
            }
        }

        if (header is null)
        {
            return NotFound(ApiResponse.Fail("Рецептура не найдена."));
        }

        var components = new List<RecipeComponentItem>();
        await using (var componentsCommand = new NpgsqlCommand(componentsSql, connection))
        {
            componentsCommand.Parameters.AddWithValue("id", id);
            await using var reader = await componentsCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                components.Add(new RecipeComponentItem(
                    reader.GetInt32("id"),
                    reader.GetInt32("raw_material_id"),
                    reader.GetString("raw_material_name"),
                    reader.GetDecimal("percentage"),
                    reader.GetInt32("load_order"),
                    reader.GetDecimal("allowed_deviation_percent")));
            }
        }

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

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string nextVersionSql = """
                SELECT COALESCE(MAX(version_number), 0) + 1
                FROM recipe_versions
                WHERE product_id = @productId;
                """;

            int nextVersion;
            await using (var nextVersionCommand = new NpgsqlCommand(nextVersionSql, connection, transaction))
            {
                nextVersionCommand.Parameters.AddWithValue("productId", request.ProductId);
                nextVersion = Convert.ToInt32(await nextVersionCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string insertRecipeSql = """
                INSERT INTO recipe_versions
                    (product_id, version_number, status, is_active, notes, created_at, created_by_user_id)
                VALUES
                    (@productId, @versionNumber, 1, FALSE, @notes, @createdAt, @createdByUserId)
                RETURNING id;
                """;

            int recipeId;
            await using (var insertRecipeCommand = new NpgsqlCommand(insertRecipeSql, connection, transaction))
            {
                insertRecipeCommand.Parameters.AddWithValue("productId", request.ProductId);
                insertRecipeCommand.Parameters.AddWithValue("versionNumber", nextVersion);
                insertRecipeCommand.Parameters.AddWithValue("notes", (object?)request.Notes ?? DBNull.Value);
                insertRecipeCommand.Parameters.AddWithValue("createdAt", DateTime.UtcNow);
                insertRecipeCommand.Parameters.AddWithValue("createdByUserId", request.CreatedByUserId);
                recipeId = Convert.ToInt32(await insertRecipeCommand.ExecuteScalarAsync(cancellationToken));
            }

            const string insertComponentSql = """
                INSERT INTO recipe_components
                    (recipe_version_id, raw_material_id, percentage, load_order, allowed_deviation_percent)
                VALUES
                    (@recipeVersionId, @rawMaterialId, @percentage, @loadOrder, @allowedDeviationPercent);
                """;

            foreach (var component in request.Components)
            {
                await using var insertComponentCommand = new NpgsqlCommand(insertComponentSql, connection, transaction);
                insertComponentCommand.Parameters.AddWithValue("recipeVersionId", recipeId);
                insertComponentCommand.Parameters.AddWithValue("rawMaterialId", component.RawMaterialId);
                insertComponentCommand.Parameters.AddWithValue("percentage", component.Percentage);
                insertComponentCommand.Parameters.AddWithValue("loadOrder", component.LoadOrder);
                insertComponentCommand.Parameters.AddWithValue("allowedDeviationPercent", component.AllowedDeviationPercent);
                await insertComponentCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<CreateRecipeResponse>.Ok(
                new CreateRecipeResponse(recipeId, request.ProductId, nextVersion),
                "Черновик рецептуры создан."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveRecipe(int id, [FromBody] ApproveRecipeRequest request, CancellationToken cancellationToken)
    {
        if (request.ApprovedByUserId <= 0)
        {
            return BadRequest(ApiResponse.Fail("Нужно указать пользователя, который утверждает рецептуру."));
        }

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string productSql = """
                SELECT product_id
                FROM recipe_versions
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
                return NotFound(ApiResponse.Fail("Рецептура не найдена."));
            }

            const string deactivateSql = """
                UPDATE recipe_versions
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
                UPDATE recipe_versions
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
                    ('RecipeVersion', @entityId, 'Draft', 'Approved', @changedAt, @changedByUserId, @comment);
                """;

            await using (var historyCommand = new NpgsqlCommand(historySql, connection, transaction))
            {
                historyCommand.Parameters.AddWithValue("entityId", id);
                historyCommand.Parameters.AddWithValue("changedAt", DateTime.UtcNow);
                historyCommand.Parameters.AddWithValue("changedByUserId", request.ApprovedByUserId);
                historyCommand.Parameters.AddWithValue("comment", (object?)request.Comment ?? "Рецептура утверждена через API.");
                await historyCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Ok(ApiResponse.Ok("Рецептура утверждена."));
        }
        catch (PostgresException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(ApiResponse.Fail(exception.MessageText));
        }
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
