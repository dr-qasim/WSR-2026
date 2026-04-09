using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class BatchRawMaterialConsumption
{
    public int ProductionBatchId { get; set; }

    public int RawMaterialLotId { get; set; }

    public decimal QuantityUsed { get; set; }

    public virtual ProductionBatch ProductionBatch { get; set; } = null!;

    public virtual RawMaterialLot RawMaterialLot { get; set; } = null!;
}
