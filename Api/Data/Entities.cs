namespace PlantProduction.Api.Data;

public sealed class Department
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class MaterialCategory
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductForm
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductionLine
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class Supplier
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
}

public sealed class UserRole
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class RawMaterial
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int MaterialCategoryId { get; set; }
    public MaterialCategory MaterialCategory { get; set; } = null!;
}

public sealed class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int ProductTypeId { get; set; }
    public int ProductFormId { get; set; }
    public ProductType ProductType { get; set; } = null!;
    public ProductForm ProductForm { get; set; } = null!;
}

public sealed class AppUser
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserRoleId { get; set; }
    public int DepartmentId { get; set; }
    public UserRole UserRole { get; set; } = null!;
    public Department Department { get; set; } = null!;
}

public sealed class RawMaterialLot
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public int SupplierId { get; set; }
    public string InternalLotNumber { get; set; } = string.Empty;
    public string? SupplierLotNumber { get; set; }
    public DateTime ReceivedAt { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityAvailable { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? LastLaboratoryDecisionAt { get; set; }
    public RawMaterial RawMaterial { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
}

public sealed class QualitySpecification
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SubjectType { get; set; }
    public int? RawMaterialId { get; set; }
    public int? ProductId { get; set; }
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public RawMaterial? RawMaterial { get; set; }
    public Product? Product { get; set; }
    public List<QualitySpecificationParameter> Parameters { get; set; } = [];
}

public sealed class QualitySpecificationParameter
{
    public int Id { get; set; }
    public int QualitySpecificationId { get; set; }
    public int SortOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ValueType { get; set; }
    public string? Unit { get; set; }
    public decimal? MinNumericValue { get; set; }
    public decimal? MaxNumericValue { get; set; }
    public string? TargetTextValue { get; set; }
    public bool? TargetBooleanValue { get; set; }
    public bool IsRequired { get; set; }
    public QualitySpecification QualitySpecification { get; set; } = null!;
}

public sealed class RecipeVersion
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int VersionNumber { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public Product Product { get; set; } = null!;
    public AppUser CreatedByUser { get; set; } = null!;
    public AppUser? ApprovedByUser { get; set; }
    public List<RecipeComponent> Components { get; set; } = [];
}

public sealed class RecipeComponent
{
    public int Id { get; set; }
    public int RecipeVersionId { get; set; }
    public int RawMaterialId { get; set; }
    public decimal Percentage { get; set; }
    public int LoadOrder { get; set; }
    public decimal AllowedDeviationPercent { get; set; }
    public RecipeVersion RecipeVersion { get; set; } = null!;
    public RawMaterial RawMaterial { get; set; } = null!;
}

public sealed class StatusHistory
{
    public long Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? Comment { get; set; }
}

public sealed class TechnologyCard
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public Product Product { get; set; } = null!;
    public AppUser CreatedByUser { get; set; } = null!;
    public AppUser? ApprovedByUser { get; set; }
    public List<TechnologyStep> Steps { get; set; } = [];
}

public sealed class TechnologyStep
{
    public int Id { get; set; }
    public int TechnologyCardId { get; set; }
    public int StepOrder { get; set; }
    public int StepType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public bool IsRequired { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public TechnologyCard TechnologyCard { get; set; } = null!;
    public List<TechnologyStepParameter> Parameters { get; set; } = [];
}

public sealed class TechnologyStepParameter
{
    public int Id { get; set; }
    public int TechnologyStepId { get; set; }
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
    public TechnologyStep TechnologyStep { get; set; } = null!;
}

public sealed class ExtruderProgram
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? TechnologyCardId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public Product Product { get; set; } = null!;
}

public sealed class ProductionOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int ProductionLineId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public DateTime? PlannedStartAt { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public Product Product { get; set; } = null!;
    public ProductionLine ProductionLine { get; set; } = null!;
    public AppUser CreatedByUser { get; set; } = null!;
}

public sealed class ProductionBatch
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
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
    public ProductionOrder? ProductionOrder { get; set; }
    public Product Product { get; set; } = null!;
    public ProductionLine ProductionLine { get; set; } = null!;
    public RecipeVersion RecipeVersion { get; set; } = null!;
    public TechnologyCard TechnologyCard { get; set; } = null!;
    public ExtruderProgram? ExtruderProgram { get; set; }
    public List<BatchRawMaterialConsumption> Consumptions { get; set; } = [];
    public List<BatchTechnologyStepRun> StepRuns { get; set; } = [];
}

public sealed class BatchRawMaterialConsumption
{
    public int ProductionBatchId { get; set; }
    public int RawMaterialLotId { get; set; }
    public decimal QuantityUsed { get; set; }
    public ProductionBatch ProductionBatch { get; set; } = null!;
    public RawMaterialLot RawMaterialLot { get; set; } = null!;
}

public sealed class BatchTechnologyStepRun
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public int TechnologyStepId { get; set; }
    public int Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? StartedByUserId { get; set; }
    public int? CompletedByUserId { get; set; }
    public string? Comment { get; set; }
    public ProductionBatch ProductionBatch { get; set; } = null!;
    public TechnologyStep TechnologyStep { get; set; } = null!;
    public AppUser? StartedByUser { get; set; }
    public AppUser? CompletedByUser { get; set; }
}

public sealed class BatchStepMeasuredValue
{
    public int Id { get; set; }
    public int BatchTechnologyStepRunId { get; set; }
    public int TechnologyStepParameterId { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public string? ActualTextValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public bool? IsWithinTolerance { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? Comment { get; set; }
    public BatchTechnologyStepRun BatchTechnologyStepRun { get; set; } = null!;
    public TechnologyStepParameter TechnologyStepParameter { get; set; } = null!;
}

public sealed class ProcessDeviation
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public int? BatchTechnologyStepRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ParameterName { get; set; }
    public string? PlannedValue { get; set; }
    public string? ActualValue { get; set; }
    public int Severity { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ReportedByUserId { get; set; }
    public ProductionBatch ProductionBatch { get; set; } = null!;
    public BatchTechnologyStepRun? BatchTechnologyStepRun { get; set; }
}

public sealed class LaboratoryTest
{
    public int Id { get; set; }
    public string TestNumber { get; set; } = string.Empty;
    public int SubjectType { get; set; }
    public int? RawMaterialLotId { get; set; }
    public int? ProductionBatchId { get; set; }
    public int QualitySpecificationId { get; set; }
    public string TestKind { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public string? Comment { get; set; }
    public string? ResultSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TesterUserId { get; set; }
    public RawMaterialLot? RawMaterialLot { get; set; }
    public ProductionBatch? ProductionBatch { get; set; }
    public QualitySpecification QualitySpecification { get; set; } = null!;
    public AppUser? TesterUser { get; set; }
    public List<LaboratoryTestParameterResult> ParameterResults { get; set; } = [];
}

public sealed class LaboratoryTestParameterResult
{
    public int Id { get; set; }
    public int LaboratoryTestId { get; set; }
    public int? QualitySpecificationParameterId { get; set; }
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
    public LaboratoryTest LaboratoryTest { get; set; } = null!;
    public QualitySpecificationParameter? QualitySpecificationParameter { get; set; }
}

public sealed class QualityDecision
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
    public LaboratoryTest LaboratoryTest { get; set; } = null!;
}
