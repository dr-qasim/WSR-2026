using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class EquipmentTelemetryReading
{
    public long Id { get; set; }

    public int ProductionBatchId { get; set; }

    public int? EquipmentId { get; set; }

    public string ZoneName { get; set; } = null!;

    public string MetricName { get; set; } = null!;

    public decimal NumericValue { get; set; }

    public string? Unit { get; set; }

    public int Severity { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual ProductionBatch ProductionBatch { get; set; } = null!;
}
