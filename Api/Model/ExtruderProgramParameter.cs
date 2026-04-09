using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class ExtruderProgramParameter
{
    public int Id { get; set; }

    public int ExtruderProgramId { get; set; }

    public int SortOrder { get; set; }

    public int? ZoneNumber { get; set; }

    public string Name { get; set; } = null!;

    public int ValueType { get; set; }

    public decimal? TargetNumericValue { get; set; }

    public string? TargetTextValue { get; set; }

    public bool? TargetBooleanValue { get; set; }

    public string? Unit { get; set; }

    public virtual ExtruderProgram ExtruderProgram { get; set; } = null!;
}
