using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantProduction.Api.Common;
using PlantProduction.Api.Model;

namespace PlantProduction.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/catalog")]
public sealed class CatalogController(PlantProductionScaffoldDbContext dbContext) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var items = await dbContext.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductListItem(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Status,
                x.ProductType.Name,
                x.ProductForm.Name))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<ProductListItem>>.Ok(items));
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetProduct(int id, CancellationToken cancellationToken)
    {
        var header = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDetailHeader(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Status,
                x.ProductType.Name,
                x.ProductForm.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return NotFound(ApiResponse.Fail("Продукт не найден."));
        }

        var recipes = await dbContext.RecipeVersions
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new ProductRecipeItem(
                x.Id,
                x.VersionNumber,
                x.Status,
                x.IsActive,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .ToListAsync(cancellationToken);

        var technologyCards = await dbContext.TechnologyCards
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new ProductTechnologyCardItem(
                x.Id,
                x.VersionNumber,
                x.Title,
                x.Status,
                x.IsActive,
                x.CreatedAt,
                x.CreatedByUser.FullName,
                x.ApprovedAt,
                x.ApprovedByUser != null ? x.ApprovedByUser.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<ProductDetail>.Ok(new ProductDetail(header, recipes, technologyCards)));
    }

    [HttpGet("raw-materials")]
    public async Task<IActionResult> GetRawMaterials(CancellationToken cancellationToken)
    {
        var items = await dbContext.RawMaterials
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RawMaterialListItem(
                x.Id,
                x.Code,
                x.Name,
                x.Unit,
                x.Description,
                x.Status,
                x.MaterialCategory.Name))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<RawMaterialListItem>>.Ok(items));
    }

    [HttpGet("raw-material-lots")]
    public async Task<IActionResult> GetRawMaterialLots(CancellationToken cancellationToken)
    {
        var items = await dbContext.RawMaterialLots
            .AsNoTracking()
            .OrderByDescending(x => x.ReceivedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new RawMaterialLotListItem(
                x.Id,
                x.InternalLotNumber,
                x.SupplierLotNumber,
                x.ReceivedAt,
                x.QuantityReceived,
                x.QuantityAvailable,
                x.StorageLocation,
                x.Status,
                x.RawMaterialId,
                x.RawMaterial.Name,
                x.SupplierId,
                x.Supplier.Name))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<RawMaterialLotListItem>>.Ok(items));
    }

    [HttpGet("production-lines")]
    public async Task<IActionResult> GetProductionLines(CancellationToken cancellationToken)
    {
        var items = await dbContext.ProductionLines
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductionLineItem(
                x.Id,
                x.Code,
                x.Name,
                x.IsActive))
            .ToListAsync(cancellationToken);

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

    public sealed record ProductDetailHeader(
        int Id,
        string Code,
        string Name,
        string? Description,
        int Status,
        string ProductTypeName,
        string ProductFormName);

    public sealed record ProductRecipeItem(
        int Id,
        int VersionNumber,
        int Status,
        bool IsActive,
        DateTime CreatedAt,
        string CreatedByName,
        DateTime? ApprovedAt,
        string? ApprovedByName);

    public sealed record ProductTechnologyCardItem(
        int Id,
        int VersionNumber,
        string Title,
        int Status,
        bool IsActive,
        DateTime CreatedAt,
        string CreatedByName,
        DateTime? ApprovedAt,
        string? ApprovedByName);

    public sealed record ProductDetail(
        ProductDetailHeader Header,
        List<ProductRecipeItem> Recipes,
        List<ProductTechnologyCardItem> TechnologyCards);

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
