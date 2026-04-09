using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class Notification
{
    public long Id { get; set; }

    public int RecipientUserId { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public string Message { get; set; } = null!;

    public int Severity { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual AppUser RecipientUser { get; set; } = null!;
}
