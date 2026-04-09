using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class MaterialCategory
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<RawMaterial> RawMaterials { get; set; } = new List<RawMaterial>();
}
