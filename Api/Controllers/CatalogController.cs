using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PlantProduction.Api.Common;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/catalog")]
public sealed class CatalogController(NpgsqlDataSource dataSource) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.id,
                p.code,
                p.name,
                p.description,
                p.status,
                pt.name AS product_type_name,
                pf.name AS product_form_name
            FROM products p
            JOIN product_types pt ON pt.id = p.product_type_id
            JOIN product_forms pf ON pf.id = p.product_form_id
            ORDER BY p.name;
            """;

        var items = new List<ProductListItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductListItem(
                reader.GetInt32("id"),
                reader.GetString("code"),
                reader.GetString("name"),
                reader.GetNullableString("description"),
                reader.GetInt32("status"),
                reader.GetString("product_type_name"),
                reader.GetString("product_form_name")));
        }

        return Ok(ApiResponse<List<ProductListItem>>.Ok(items));
    }

    [HttpGet("raw-materials")]
    public async Task<IActionResult> GetRawMaterials(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rm.id,
                rm.code,
                rm.name,
                rm.unit,
                rm.description,
                rm.status,
                mc.name AS category_name
            FROM raw_materials rm
            JOIN material_categories mc ON mc.id = rm.material_category_id
            ORDER BY rm.name;
            """;

        var items = new List<RawMaterialListItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new RawMaterialListItem(
                reader.GetInt32("id"),
                reader.GetString("code"),
                reader.GetString("name"),
                reader.GetString("unit"),
                reader.GetNullableString("description"),
                reader.GetInt32("status"),
                reader.GetString("category_name")));
        }

        return Ok(ApiResponse<List<RawMaterialListItem>>.Ok(items));
    }

    [HttpGet("raw-material-lots")]
    public async Task<IActionResult> GetRawMaterialLots(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                lot.id,
                lot.internal_lot_number,
                lot.supplier_lot_number,
                lot.received_at,
                lot.quantity_received,
                lot.quantity_available,
                lot.storage_location,
                lot.status,
                rm.id AS raw_material_id,
                rm.name AS raw_material_name,
                s.id AS supplier_id,
                s.name AS supplier_name
            FROM raw_material_lots lot
            JOIN raw_materials rm ON rm.id = lot.raw_material_id
            JOIN suppliers s ON s.id = lot.supplier_id
            ORDER BY lot.received_at DESC, lot.id DESC;
            """;

        var items = new List<RawMaterialLotListItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new RawMaterialLotListItem(
                reader.GetInt32("id"),
                reader.GetString("internal_lot_number"),
                reader.GetNullableString("supplier_lot_number"),
                reader.GetDateTime("received_at"),
                reader.GetDecimal("quantity_received"),
                reader.GetDecimal("quantity_available"),
                reader.GetString("storage_location"),
                reader.GetInt32("status"),
                reader.GetInt32("raw_material_id"),
                reader.GetString("raw_material_name"),
                reader.GetInt32("supplier_id"),
                reader.GetString("supplier_name")));
        }

        return Ok(ApiResponse<List<RawMaterialLotListItem>>.Ok(items));
    }

    [HttpGet("production-lines")]
    public async Task<IActionResult> GetProductionLines(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, code, name, is_active
            FROM production_lines
            ORDER BY name;
            """;

        var items = new List<ProductionLineItem>();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductionLineItem(
                reader.GetInt32("id"),
                reader.GetString("code"),
                reader.GetString("name"),
                reader.GetBoolean("is_active")));
        }

        return Ok(ApiResponse<List<ProductionLineItem>>.Ok(items));
    }

    public sealed record ProductListItem(
        int Id,
        string Code,
        string Name,
        string? Description,
        int Status,
        string ProductTypeName,
        string ProductFormName);

    public sealed record RawMaterialListItem(
        int Id,
        string Code,
        string Name,
        string Unit,
        string? Description,
        int Status,
        string CategoryName);

    public sealed record RawMaterialLotListItem(
        int Id,
        string InternalLotNumber,
        string? SupplierLotNumber,
        DateTime ReceivedAt,
        decimal QuantityReceived,
        decimal QuantityAvailable,
        string StorageLocation,
        int Status,
        int RawMaterialId,
        string RawMaterialName,
        int SupplierId,
        string SupplierName);

    public sealed record ProductionLineItem(
        int Id,
        string Code,
        string Name,
        bool IsActive);
}
