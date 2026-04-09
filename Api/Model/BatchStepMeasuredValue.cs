using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class BatchStepMeasuredValue
{
    public int Id { get; set; }

    public int BatchTechnologyStepRunId { get; set; }

    public int TechnologyStepParameterId { get; set; }

    public decimal? ActualNumericValue { get; set; }

    public string? ActualTextValue { get; set; }

    public bool? ActualBooleanValue { get; set; }

    public bool? IsWithinTolerance { get; set; }

    public DateTime RecordedAt { get; set; }

    public string? Comment { get; set; }

    public virtual BatchTechnologyStepRun BatchTechnologyStepRun { get; set; } = null!;

    public virtual TechnologyStepParameter TechnologyStepParameter { get; set; } = null!;
}
