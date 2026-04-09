using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class Product
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Status { get; set; }

    public int ProductTypeId { get; set; }

    public int ProductFormId { get; set; }

    public virtual ICollection<ExtruderProgram> ExtruderPrograms { get; set; } = new List<ExtruderProgram>();

    public virtual ProductForm ProductForm { get; set; } = null!;

    public virtual ProductType ProductType { get; set; } = null!;

    public virtual ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();

    public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();

    public virtual QualitySpecification? QualitySpecification { get; set; }

    public virtual RecipeVersion? RecipeVersion { get; set; }

    public virtual TechnologyCard? TechnologyCard { get; set; }
}
