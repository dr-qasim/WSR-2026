using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class QualityDecision
{
    public int Id { get; set; }

    public int SubjectType { get; set; }

    public int? RawMaterialLotId { get; set; }

    public int? ProductionBatchId { get; set; }

    public int LaboratoryTestId { get; set; }

    public int DecisionStatus { get; set; }

    public string? Comment { get; set; }

    public string? BlockReason { get; set; }

    public bool IsCurrent { get; set; }

    public DateTime DecidedAt { get; set; }

    public int DecidedByUserId { get; set; }

    public virtual AppUser DecidedByUser { get; set; } = null!;

    public virtual LaboratoryTest LaboratoryTest { get; set; } = null!;

    public virtual ProductionBatch? ProductionBatch { get; set; }

    public virtual RawMaterialLot? RawMaterialLot { get; set; }
}
