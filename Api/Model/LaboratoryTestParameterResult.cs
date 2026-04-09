using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class LaboratoryTestParameterResult
{
    public int Id { get; set; }

    public int LaboratoryTestId { get; set; }

    public int? QualitySpecificationParameterId { get; set; }

    public int SortOrder { get; set; }

    public string ParameterName { get; set; } = null!;

    public int ValueType { get; set; }

    public string? Unit { get; set; }

    public decimal? MinNumericValue { get; set; }

    public decimal? MaxNumericValue { get; set; }

    public string? TargetTextValue { get; set; }

    public bool? TargetBooleanValue { get; set; }

    public decimal? ActualNumericValue { get; set; }

    public string? ActualTextValue { get; set; }

    public bool? ActualBooleanValue { get; set; }

    public bool IsRequired { get; set; }

    public bool? IsWithinRange { get; set; }

    public string? Comment { get; set; }

    public virtual LaboratoryTest LaboratoryTest { get; set; } = null!;

    public virtual QualitySpecificationParameter? QualitySpecificationParameter { get; set; }
}
