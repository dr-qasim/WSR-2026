using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class Equipment
{
    public int Id { get; set; }

    public int ProductionLineId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int EquipmentType { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<EquipmentTelemetryReading> EquipmentTelemetryReadings { get; set; } = new List<EquipmentTelemetryReading>();

    public virtual ProductionLine ProductionLine { get; set; } = null!;
}
