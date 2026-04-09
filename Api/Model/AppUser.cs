using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class AppUser
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int UserRoleId { get; set; }

    public int DepartmentId { get; set; }

    public virtual ICollection<AuditEvent> AuditEvents { get; set; } = new List<AuditEvent>();

    public virtual ICollection<BatchTechnologyStepRun> BatchTechnologyStepRunCompletedByUsers { get; set; } = new List<BatchTechnologyStepRun>();

    public virtual ICollection<BatchTechnologyStepRun> BatchTechnologyStepRunStartedByUsers { get; set; } = new List<BatchTechnologyStepRun>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<LaboratoryTest> LaboratoryTests { get; set; } = new List<LaboratoryTest>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ProcessDeviation> ProcessDeviations { get; set; } = new List<ProcessDeviation>();

    public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();

    public virtual ICollection<QualityDecision> QualityDecisions { get; set; } = new List<QualityDecision>();

    public virtual ICollection<RecipeVersion> RecipeVersionApprovedByUsers { get; set; } = new List<RecipeVersion>();

    public virtual ICollection<RecipeVersion> RecipeVersionCreatedByUsers { get; set; } = new List<RecipeVersion>();

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual ICollection<TechnologyCard> TechnologyCardApprovedByUsers { get; set; } = new List<TechnologyCard>();

    public virtual ICollection<TechnologyCard> TechnologyCardCreatedByUsers { get; set; } = new List<TechnologyCard>();

    public virtual UserRole UserRole { get; set; } = null!;
}
