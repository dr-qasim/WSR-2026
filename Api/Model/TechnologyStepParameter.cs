using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class TechnologyStepParameter
{
    public int Id { get; set; }

    public int TechnologyStepId { get; set; }

    public string Name { get; set; } = null!;

    public int ValueType { get; set; }

    public string? Unit { get; set; }

    public decimal? TargetNumericValue { get; set; }

    public decimal? MinNumericValue { get; set; }

    public decimal? MaxNumericValue { get; set; }

    public string? TargetTextValue { get; set; }

    public bool? TargetBooleanValue { get; set; }

    public bool IsRequired { get; set; }

    public string? Comment { get; set; }

    public virtual ICollection<BatchStepMeasuredValue> BatchStepMeasuredValues { get; set; } = new List<BatchStepMeasuredValue>();

    public virtual TechnologyStep TechnologyStep { get; set; } = null!;
}
