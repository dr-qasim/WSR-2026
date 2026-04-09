using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class StatusHistory
{
    public long Id { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string? PreviousStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public int? ChangedByUserId { get; set; }

    public string? Comment { get; set; }

    public virtual AppUser? ChangedByUser { get; set; }
}
