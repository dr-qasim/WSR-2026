using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class RawMaterialLot
{
    public int Id { get; set; }

    public int RawMaterialId { get; set; }

    public int SupplierId { get; set; }

    public string InternalLotNumber { get; set; } = null!;

    public string? SupplierLotNumber { get; set; }

    public DateTime ReceivedAt { get; set; }

    public decimal QuantityReceived { get; set; }

    public decimal QuantityAvailable { get; set; }

    public string StorageLocation { get; set; } = null!;

    public int Status { get; set; }

    public DateTime? LastLaboratoryDecisionAt { get; set; }

    public virtual ICollection<BatchRawMaterialConsumption> BatchRawMaterialConsumptions { get; set; } = new List<BatchRawMaterialConsumption>();

    public virtual ICollection<LaboratoryTest> LaboratoryTests { get; set; } = new List<LaboratoryTest>();

    public virtual QualityDecision? QualityDecision { get; set; }

    public virtual RawMaterial RawMaterial { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
