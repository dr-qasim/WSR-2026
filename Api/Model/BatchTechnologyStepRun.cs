using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class BatchTechnologyStepRun
{
    public int Id { get; set; }

    public int ProductionBatchId { get; set; }

    public int TechnologyStepId { get; set; }

    public int Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? StartedByUserId { get; set; }

    public int? CompletedByUserId { get; set; }

    public string? Comment { get; set; }

    public virtual ICollection<BatchStepMeasuredValue> BatchStepMeasuredValues { get; set; } = new List<BatchStepMeasuredValue>();

    public virtual AppUser? CompletedByUser { get; set; }

    public virtual ICollection<ProcessDeviation> ProcessDeviations { get; set; } = new List<ProcessDeviation>();

    public virtual ProductionBatch ProductionBatch { get; set; } = null!;

    public virtual AppUser? StartedByUser { get; set; }

    public virtual TechnologyStep TechnologyStep { get; set; } = null!;
}
