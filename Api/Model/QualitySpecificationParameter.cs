using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class QualitySpecificationParameter
{
    public int Id { get; set; }

    public int QualitySpecificationId { get; set; }

    public int SortOrder { get; set; }

    public string Name { get; set; } = null!;

    public int ValueType { get; set; }

    public string? Unit { get; set; }

    public decimal? MinNumericValue { get; set; }

    public decimal? MaxNumericValue { get; set; }

    public string? TargetTextValue { get; set; }

    public bool? TargetBooleanValue { get; set; }

    public bool IsRequired { get; set; }

    public virtual ICollection<LaboratoryTestParameterResult> LaboratoryTestParameterResults { get; set; } = new List<LaboratoryTestParameterResult>();

    public virtual QualitySpecification QualitySpecification { get; set; } = null!;
}
