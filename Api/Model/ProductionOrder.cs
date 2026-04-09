using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class ProductionOrder
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int ProductId { get; set; }

    public int ProductionLineId { get; set; }

    public decimal PlannedQuantity { get; set; }

    public DateTime? PlannedStartAt { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedByUserId { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();

    public virtual ProductionLine ProductionLine { get; set; } = null!;
}
