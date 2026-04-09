using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class Supplier
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? TaxId { get; set; }

    public virtual ICollection<RawMaterialLot> RawMaterialLots { get; set; } = new List<RawMaterialLot>();
}
