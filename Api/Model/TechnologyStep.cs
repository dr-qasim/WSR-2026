using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class TechnologyStep
{
    public int Id { get; set; }

    public int TechnologyCardId { get; set; }

    public int StepOrder { get; set; }

    public int StepType { get; set; }

    public string Title { get; set; } = null!;

    public string? Instruction { get; set; }

    public bool IsRequired { get; set; }

    public int? ExpectedDurationMinutes { get; set; }

    public virtual ICollection<BatchTechnologyStepRun> BatchTechnologyStepRuns { get; set; } = new List<BatchTechnologyStepRun>();

    public virtual TechnologyCard TechnologyCard { get; set; } = null!;

    public virtual ICollection<TechnologyStepParameter> TechnologyStepParameters { get; set; } = new List<TechnologyStepParameter>();
}
