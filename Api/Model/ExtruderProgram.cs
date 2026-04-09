using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class ExtruderProgram
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? TechnologyCardId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int VersionNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<ExtruderProgramParameter> ExtruderProgramParameters { get; set; } = new List<ExtruderProgramParameter>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();

    public virtual TechnologyCard? TechnologyCard { get; set; }
}
