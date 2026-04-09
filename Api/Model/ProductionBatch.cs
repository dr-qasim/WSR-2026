using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class ProductionBatch
{
    public int Id { get; set; }

    public string BatchNumber { get; set; } = null!;

    public int? ProductionOrderId { get; set; }

    public int ProductId { get; set; }

    public int RecipeVersionId { get; set; }

    public int TechnologyCardId { get; set; }

    public int ProductionLineId { get; set; }

    public int? ExtruderProgramId { get; set; }

    public decimal PlannedQuantity { get; set; }

    public int Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual ICollection<BatchRawMaterialConsumption> BatchRawMaterialConsumptions { get; set; } = new List<BatchRawMaterialConsumption>();

    public virtual ICollection<BatchTechnologyStepRun> BatchTechnologyStepRuns { get; set; } = new List<BatchTechnologyStepRun>();

    public virtual ICollection<EquipmentTelemetryReading> EquipmentTelemetryReadings { get; set; } = new List<EquipmentTelemetryReading>();

    public virtual ExtruderProgram? ExtruderProgram { get; set; }

    public virtual ICollection<LaboratoryTest> LaboratoryTests { get; set; } = new List<LaboratoryTest>();

    public virtual ICollection<ProcessDeviation> ProcessDeviations { get; set; } = new List<ProcessDeviation>();

    public virtual Product Product { get; set; } = null!;

    public virtual ProductionLine ProductionLine { get; set; } = null!;

    public virtual ProductionOrder? ProductionOrder { get; set; }

    public virtual QualityDecision? QualityDecision { get; set; }

    public virtual RecipeVersion RecipeVersion { get; set; } = null!;

    public virtual TechnologyCard TechnologyCard { get; set; } = null!;
}
