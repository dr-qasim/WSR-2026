namespace PlantProduction.Desktop;

public class ApiEnvelope
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiEnvelope<T> : ApiEnvelope
{
    public T? Data { get; set; }
}

public class ReportLineItem
{
    public string Показатель { get; set; } = string.Empty;
    public string Значение { get; set; } = string.Empty;
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

public class ProductListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public string ProductFormName { get; set; } = string.Empty;
}

public class ProductDetailHeader
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string ProductTypeName { get; set; } = string.Empty;
    public string ProductFormName { get; set; } = string.Empty;
}

public class ProductRecipeItem
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class ProductTechnologyCardItem
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class ProductDetail
{
    public ProductDetailHeader Header { get; set; } = new();
    public List<ProductRecipeItem> Recipes { get; set; } = new();
    public List<ProductTechnologyCardItem> TechnologyCards { get; set; } = new();
}

public class RawMaterialListItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string CategoryName { get; set; } = string.Empty;
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

public class ProductionLineItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RecipeListItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class RecipeHeader
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class RecipeComponentItem
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int LoadOrder { get; set; }
    public decimal AllowedDeviationPercent { get; set; }
}

public class RecipeDetail
{
    public RecipeHeader Header { get; set; } = new();
    public List<RecipeComponentItem> Components { get; set; } = new();
}

public class CreateRecipeRequest
{
    public int ProductId { get; set; }
    public int CreatedByUserId { get; set; }
    public string? Notes { get; set; }
    public List<CreateRecipeComponentRequest> Components { get; set; } = new();
}

public class CreateRecipeComponentRequest
{
    public int RawMaterialId { get; set; }
    public decimal Percentage { get; set; }
    public int LoadOrder { get; set; }
    public decimal AllowedDeviationPercent { get; set; }
}

public class ApproveRecipeRequest
{
    public int ApprovedByUserId { get; set; }
    public string? Comment { get; set; }
}

public class TechnologyCardListItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class TechnologyCardHeader
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
}

public class TechnologyStepParameterDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValueType { get; set; }
    public string? Unit { get; set; }
    public decimal? TargetNumericValue { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public string? TargetTextValue { get; set; }
    public bool? TargetBooleanValue { get; set; }
    public bool IsRequired { get; set; }
    public string? Comment { get; set; }
}

public class TechnologyStepDetail
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public int StepType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public bool IsRequired { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public List<TechnologyStepParameterDetail> Parameters { get; set; } = new();
}

public class TechnologyCardDetail
{
    public TechnologyCardHeader Header { get; set; } = new();
    public List<TechnologyStepDetail> Steps { get; set; } = new();
}

public class CreateTechnologyCardRequest
{
    public int ProductId { get; set; }
    public int CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateTechnologyStepRequest> Steps { get; set; } = new();
}

public class CreateTechnologyStepRequest
{
    public int StepOrder { get; set; }
    public int StepType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public bool IsRequired { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public List<CreateTechnologyStepParameterRequest> Parameters { get; set; } = new();
}

public class CreateTechnologyStepParameterRequest
{
    public string Name { get; set; } = string.Empty;
    public int ValueType { get; set; }
    public string? Unit { get; set; }
    public decimal? TargetNumericValue { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public string? TargetTextValue { get; set; }
    public bool? TargetBooleanValue { get; set; }
    public bool IsRequired { get; set; }
    public string? Comment { get; set; }
}

public class ApproveTechnologyCardRequest
{
    public int ApprovedByUserId { get; set; }
    public string? Comment { get; set; }
}

public class ProductionOrderItem
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ProductionLineId { get; set; }
    public string ProductionLineName { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class CreateProductionOrderRequest
{
    public int ProductId { get; set; }
    public int ProductionLineId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public int CreatedByUserId { get; set; }
}

public class ProductionBatchItem
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int? ProductionOrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ProductionLineId { get; set; }
    public string ProductionLineName { get; set; } = string.Empty;
    public int RecipeVersionId { get; set; }
    public int TechnologyCardId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public int Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class BatchConsumptionRequest
{
    public int RawMaterialLotId { get; set; }
    public decimal QuantityUsed { get; set; }
}

public class CreateProductionBatchRequest
{
    public int? ProductionOrderId { get; set; }
    public int ProductId { get; set; }
    public int ProductionLineId { get; set; }
    public int RecipeVersionId { get; set; }
    public int TechnologyCardId { get; set; }
    public int? ExtruderProgramId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public List<BatchConsumptionRequest> Consumptions { get; set; } = new();
}

public class BatchStepRunItem
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public int TechnologyStepId { get; set; }
    public int StepOrder { get; set; }
    public int StepType { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Comment { get; set; }
}

public class StartStepRunRequest
{
    public int StartedByUserId { get; set; }
    public string? Comment { get; set; }
}

public class CompleteStepRunRequest
{
    public int CompletedByUserId { get; set; }
    public string? Comment { get; set; }
}

public class AddMeasurementRequest
{
    public int TechnologyStepParameterId { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public string? ActualTextValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public string? Comment { get; set; }
}

public class MeasurementResponse
{
    public int Id { get; set; }
    public bool IsWithinTolerance { get; set; }
}

public class CreateDeviationRequest
{
    public int ProductionBatchId { get; set; }
    public int? BatchTechnologyStepRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ParameterName { get; set; }
    public string? PlannedValue { get; set; }
    public string? ActualValue { get; set; }
    public int Severity { get; set; }
    public string? Details { get; set; }
    public int? ReportedByUserId { get; set; }
}

public class DeviationItem
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int? BatchTechnologyStepRunId { get; set; }
    public int? StepOrder { get; set; }
    public string? StepTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ParameterName { get; set; }
    public string? PlannedValue { get; set; }
    public string? ActualValue { get; set; }
    public int Severity { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReportedByName { get; set; }
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
