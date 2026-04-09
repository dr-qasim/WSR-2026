using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class AuditEvent
{
    public long Id { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string ActionName { get; set; } = null!;

    public DateTime OccurredAt { get; set; }

    public int? ActingUserId { get; set; }

    public string? Details { get; set; }

    public virtual AppUser? ActingUser { get; set; }
}
