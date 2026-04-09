using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class RecipeVersion
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int VersionNumber { get; set; }

    public int Status { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedByUserId { get; set; }

    public virtual AppUser? ApprovedByUser { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();

    public virtual ICollection<RecipeComponent> RecipeComponents { get; set; } = new List<RecipeComponent>();
}
