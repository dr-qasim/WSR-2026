using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class RawMaterial
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public string? Description { get; set; }

    public int Status { get; set; }

    public int MaterialCategoryId { get; set; }

    public virtual MaterialCategory MaterialCategory { get; set; } = null!;

    public virtual QualitySpecification? QualitySpecification { get; set; }

    public virtual ICollection<RawMaterialLot> RawMaterialLots { get; set; } = new List<RawMaterialLot>();

    public virtual ICollection<RecipeComponent> RecipeComponents { get; set; } = new List<RecipeComponent>();
}
