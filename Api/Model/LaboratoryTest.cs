using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class LaboratoryTest
{
    public int Id { get; set; }

    public string TestNumber { get; set; } = null!;

    public int SubjectType { get; set; }

    public int? RawMaterialLotId { get; set; }

    public int? ProductionBatchId { get; set; }

    public int QualitySpecificationId { get; set; }

    public string TestKind { get; set; } = null!;

    public int Status { get; set; }

    public int Priority { get; set; }

    public string? Comment { get; set; }

    public string? ResultSummary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? TesterUserId { get; set; }

    public virtual ICollection<LaboratoryTestParameterResult> LaboratoryTestParameterResults { get; set; } = new List<LaboratoryTestParameterResult>();

    public virtual ProductionBatch? ProductionBatch { get; set; }

    public virtual ICollection<QualityDecision> QualityDecisions { get; set; } = new List<QualityDecision>();

    public virtual QualitySpecification QualitySpecification { get; set; } = null!;

    public virtual RawMaterialLot? RawMaterialLot { get; set; }

    public virtual AppUser? TesterUser { get; set; }
}
