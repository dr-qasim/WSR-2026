using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class RecipeComponent
{
    public int Id { get; set; }

    public int RecipeVersionId { get; set; }

    public int RawMaterialId { get; set; }

    public decimal Percentage { get; set; }

    public int LoadOrder { get; set; }

    public decimal AllowedDeviationPercent { get; set; }

    public virtual RawMaterial RawMaterial { get; set; } = null!;

    public virtual RecipeVersion RecipeVersion { get; set; } = null!;
}
