using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class ProcessDeviation
{
    public int Id { get; set; }

    public int ProductionBatchId { get; set; }

    public int? BatchTechnologyStepRunId { get; set; }

    public string Title { get; set; } = null!;

    public string? ParameterName { get; set; }

    public string? PlannedValue { get; set; }

    public string? ActualValue { get; set; }

    public int Severity { get; set; }

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ReportedByUserId { get; set; }

    public virtual BatchTechnologyStepRun? BatchTechnologyStepRun { get; set; }

    public virtual ProductionBatch ProductionBatch { get; set; } = null!;

    public virtual AppUser? ReportedByUser { get; set; }
}
