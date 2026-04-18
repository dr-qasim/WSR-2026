namespace PlantProduction.Desktop.Laboratory;

public class ApiEnvelope
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiEnvelope<T> : ApiEnvelope
{
    public T? Data { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class RawMaterialLotListItem
{
    public int Id { get; set; }
    public string InternalLotNumber { get; set; } = string.Empty;
    public string? SupplierLotNumber { get; set; }
    public DateTime ReceivedAt { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityAvailable { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public int Status { get; set; }
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

public class QualitySpecificationItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SubjectType { get; set; }
    public int? RawMaterialId { get; set; }
    public string? RawMaterialName { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
}

public class LaboratoryTestItem
{
    public int Id { get; set; }
    public string TestNumber { get; set; } = string.Empty;
    public int SubjectType { get; set; }
    public int? RawMaterialLotId { get; set; }
    public string? RawMaterialLotNumber { get; set; }
    public int? ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public int QualitySpecificationId { get; set; }
    public string QualitySpecificationName { get; set; } = string.Empty;
    public string TestKind { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public string? Comment { get; set; }
    public string? ResultSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? TesterName { get; set; }
}

public class QualityDecisionItem
{
    public int Id { get; set; }
    public int SubjectType { get; set; }
    public int? RawMaterialLotId { get; set; }
    public string? RawMaterialLotNumber { get; set; }
    public int? ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public int LaboratoryTestId { get; set; }
    public string TestNumber { get; set; } = string.Empty;
    public int DecisionStatus { get; set; }
    public string? Comment { get; set; }
    public string? BlockReason { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime DecidedAt { get; set; }
    public string DecidedByName { get; set; } = string.Empty;
}

public class LaboratoryTestParameterResultItem
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public int ValueType { get; set; }
    public string? Unit { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public string? TargetTextValue { get; set; }
    public bool? TargetBooleanValue { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public string? ActualTextValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public bool IsRequired { get; set; }
    public bool? IsWithinRange { get; set; }
    public string? Comment { get; set; }
}

public class LaboratoryTestDetail
{
    public LaboratoryTestItem Test { get; set; } = new();
    public List<LaboratoryTestParameterResultItem> Results { get; set; } = new();
}

public class CreateLaboratoryTestRequest
{
    public int SubjectType { get; set; }
    public int? RawMaterialLotId { get; set; }
    public int? ProductionBatchId { get; set; }
    public int QualitySpecificationId { get; set; }
    public string TestKind { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? Comment { get; set; }
    public int? TesterUserId { get; set; }
}

public class StartLaboratoryTestRequest
{
    public int TesterUserId { get; set; }
}

public class SaveLaboratoryResultsRequest
{
    public List<SaveLaboratoryResultItem> Items { get; set; } = new();
}

public class SaveLaboratoryResultItem
{
    public int ParameterResultId { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public string? ActualTextValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public string? Comment { get; set; }
}

public class CompleteLaboratoryTestRequest
{
    public int TesterUserId { get; set; }
    public string? ResultSummary { get; set; }
}

public class CreateQualityDecisionRequest
{
    public int LaboratoryTestId { get; set; }
    public int DecisionStatus { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? BlockReason { get; set; }
    public int DecidedByUserId { get; set; }
}
