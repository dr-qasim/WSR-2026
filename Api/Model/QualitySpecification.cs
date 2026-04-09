using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class QualitySpecification
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int SubjectType { get; set; }

    public int? RawMaterialId { get; set; }

    public int? ProductId { get; set; }

    public int VersionNumber { get; set; }

    public int Status { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LaboratoryTest> LaboratoryTests { get; set; } = new List<LaboratoryTest>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<QualitySpecificationParameter> QualitySpecificationParameters { get; set; } = new List<QualitySpecificationParameter>();

    public virtual RawMaterial? RawMaterial { get; set; }
}
