using Microsoft.EntityFrameworkCore;

namespace PlantProduction.Api.Data;

public sealed class PlantProductionDbContext(DbContextOptions<PlantProductionDbContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<ProductForm> ProductForms => Set<ProductForm>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RawMaterialLot> RawMaterialLots => Set<RawMaterialLot>();
    public DbSet<QualitySpecification> QualitySpecifications => Set<QualitySpecification>();
    public DbSet<QualitySpecificationParameter> QualitySpecificationParameters => Set<QualitySpecificationParameter>();
    public DbSet<RecipeVersion> RecipeVersions => Set<RecipeVersion>();
    public DbSet<RecipeComponent> RecipeComponents => Set<RecipeComponent>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<TechnologyCard> TechnologyCards => Set<TechnologyCard>();
    public DbSet<TechnologyStep> TechnologySteps => Set<TechnologyStep>();
    public DbSet<TechnologyStepParameter> TechnologyStepParameters => Set<TechnologyStepParameter>();
    public DbSet<ExtruderProgram> ExtruderPrograms => Set<ExtruderProgram>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<BatchRawMaterialConsumption> BatchRawMaterialConsumptions => Set<BatchRawMaterialConsumption>();
    public DbSet<BatchTechnologyStepRun> BatchTechnologyStepRuns => Set<BatchTechnologyStepRun>();
    public DbSet<BatchStepMeasuredValue> BatchStepMeasuredValues => Set<BatchStepMeasuredValue>();
    public DbSet<ProcessDeviation> ProcessDeviations => Set<ProcessDeviation>();
    public DbSet<LaboratoryTest> LaboratoryTests => Set<LaboratoryTest>();
    public DbSet<LaboratoryTestParameterResult> LaboratoryTestParameterResults => Set<LaboratoryTestParameterResult>();
    public DbSet<QualityDecision> QualityDecisions => Set<QualityDecision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().ToTable("departments");
        modelBuilder.Entity<MaterialCategory>().ToTable("material_categories");
        modelBuilder.Entity<ProductForm>().ToTable("product_forms");
        modelBuilder.Entity<ProductType>().ToTable("product_types");
        modelBuilder.Entity<ProductionLine>().ToTable("production_lines");
        modelBuilder.Entity<Supplier>().ToTable("suppliers");
        modelBuilder.Entity<UserRole>().ToTable("user_roles");
        modelBuilder.Entity<RawMaterial>().ToTable("raw_materials");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<AppUser>().ToTable("app_users");
        modelBuilder.Entity<RawMaterialLot>().ToTable("raw_material_lots");
        modelBuilder.Entity<QualitySpecification>().ToTable("quality_specifications");
        modelBuilder.Entity<QualitySpecificationParameter>().ToTable("quality_specification_parameters");
        modelBuilder.Entity<RecipeVersion>().ToTable("recipe_versions");
        modelBuilder.Entity<RecipeComponent>().ToTable("recipe_components");
        modelBuilder.Entity<StatusHistory>().ToTable("status_histories");
        modelBuilder.Entity<TechnologyCard>().ToTable("technology_cards");
        modelBuilder.Entity<TechnologyStep>().ToTable("technology_steps");
        modelBuilder.Entity<TechnologyStepParameter>().ToTable("technology_step_parameters");
        modelBuilder.Entity<ExtruderProgram>().ToTable("extruder_programs");
        modelBuilder.Entity<ProductionOrder>().ToTable("production_orders");
        modelBuilder.Entity<ProductionBatch>().ToTable("production_batches");
        modelBuilder.Entity<BatchRawMaterialConsumption>().ToTable("batch_raw_material_consumptions");
        modelBuilder.Entity<BatchTechnologyStepRun>().ToTable("batch_technology_step_runs");
        modelBuilder.Entity<BatchStepMeasuredValue>().ToTable("batch_step_measured_values");
        modelBuilder.Entity<ProcessDeviation>().ToTable("process_deviations");
        modelBuilder.Entity<LaboratoryTest>().ToTable("laboratory_tests");
        modelBuilder.Entity<LaboratoryTestParameterResult>().ToTable("laboratory_test_parameter_results");
        modelBuilder.Entity<QualityDecision>().ToTable("quality_decisions");

        modelBuilder.Entity<BatchRawMaterialConsumption>()
            .HasKey(x => new { x.ProductionBatchId, x.RawMaterialLotId });

        modelBuilder.Entity<AppUser>()
            .HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId);

        modelBuilder.Entity<AppUser>()
            .HasOne(x => x.UserRole)
            .WithMany()
            .HasForeignKey(x => x.UserRoleId);

        modelBuilder.Entity<RawMaterial>()
            .HasOne(x => x.MaterialCategory)
            .WithMany()
            .HasForeignKey(x => x.MaterialCategoryId);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.ProductType)
            .WithMany()
            .HasForeignKey(x => x.ProductTypeId);

        modelBuilder.Entity<Product>()
            .HasOne(x => x.ProductForm)
            .WithMany()
            .HasForeignKey(x => x.ProductFormId);

        modelBuilder.Entity<RawMaterialLot>()
            .HasOne(x => x.RawMaterial)
            .WithMany()
            .HasForeignKey(x => x.RawMaterialId);

        modelBuilder.Entity<RawMaterialLot>()
            .HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId);

        modelBuilder.Entity<QualitySpecification>()
            .HasOne(x => x.RawMaterial)
            .WithMany()
            .HasForeignKey(x => x.RawMaterialId);

        modelBuilder.Entity<QualitySpecification>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<QualitySpecificationParameter>()
            .HasOne(x => x.QualitySpecification)
            .WithMany(x => x.Parameters)
            .HasForeignKey(x => x.QualitySpecificationId);

        modelBuilder.Entity<RecipeVersion>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<RecipeVersion>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecipeVersion>()
            .HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecipeComponent>()
            .HasOne(x => x.RecipeVersion)
            .WithMany(x => x.Components)
            .HasForeignKey(x => x.RecipeVersionId);

        modelBuilder.Entity<RecipeComponent>()
            .HasOne(x => x.RawMaterial)
            .WithMany()
            .HasForeignKey(x => x.RawMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TechnologyCard>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<TechnologyCard>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TechnologyCard>()
            .HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TechnologyStep>()
            .HasOne(x => x.TechnologyCard)
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.TechnologyCardId);

        modelBuilder.Entity<TechnologyStepParameter>()
            .HasOne(x => x.TechnologyStep)
            .WithMany(x => x.Parameters)
            .HasForeignKey(x => x.TechnologyStepId);

        modelBuilder.Entity<ProductionOrder>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<ProductionOrder>()
            .HasOne(x => x.ProductionLine)
            .WithMany()
            .HasForeignKey(x => x.ProductionLineId);

        modelBuilder.Entity<ProductionOrder>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExtruderProgram>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.ProductionOrder)
            .WithMany()
            .HasForeignKey(x => x.ProductionOrderId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.ProductionLine)
            .WithMany()
            .HasForeignKey(x => x.ProductionLineId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.RecipeVersion)
            .WithMany()
            .HasForeignKey(x => x.RecipeVersionId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.TechnologyCard)
            .WithMany()
            .HasForeignKey(x => x.TechnologyCardId);

        modelBuilder.Entity<ProductionBatch>()
            .HasOne(x => x.ExtruderProgram)
            .WithMany()
            .HasForeignKey(x => x.ExtruderProgramId);

        modelBuilder.Entity<BatchRawMaterialConsumption>()
            .HasOne(x => x.ProductionBatch)
            .WithMany(x => x.Consumptions)
            .HasForeignKey(x => x.ProductionBatchId);

        modelBuilder.Entity<BatchRawMaterialConsumption>()
            .HasOne(x => x.RawMaterialLot)
            .WithMany()
            .HasForeignKey(x => x.RawMaterialLotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BatchTechnologyStepRun>()
            .HasOne(x => x.ProductionBatch)
            .WithMany(x => x.StepRuns)
            .HasForeignKey(x => x.ProductionBatchId);

        modelBuilder.Entity<BatchTechnologyStepRun>()
            .HasOne(x => x.TechnologyStep)
            .WithMany()
            .HasForeignKey(x => x.TechnologyStepId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BatchTechnologyStepRun>()
            .HasOne(x => x.StartedByUser)
            .WithMany()
            .HasForeignKey(x => x.StartedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BatchTechnologyStepRun>()
            .HasOne(x => x.CompletedByUser)
            .WithMany()
            .HasForeignKey(x => x.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BatchStepMeasuredValue>()
            .HasOne(x => x.BatchTechnologyStepRun)
            .WithMany()
            .HasForeignKey(x => x.BatchTechnologyStepRunId);

        modelBuilder.Entity<BatchStepMeasuredValue>()
            .HasOne(x => x.TechnologyStepParameter)
            .WithMany()
            .HasForeignKey(x => x.TechnologyStepParameterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProcessDeviation>()
            .HasOne(x => x.ProductionBatch)
            .WithMany()
            .HasForeignKey(x => x.ProductionBatchId);

        modelBuilder.Entity<ProcessDeviation>()
            .HasOne(x => x.BatchTechnologyStepRun)
            .WithMany()
            .HasForeignKey(x => x.BatchTechnologyStepRunId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LaboratoryTest>()
            .HasOne(x => x.RawMaterialLot)
            .WithMany()
            .HasForeignKey(x => x.RawMaterialLotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LaboratoryTest>()
            .HasOne(x => x.ProductionBatch)
            .WithMany()
            .HasForeignKey(x => x.ProductionBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LaboratoryTest>()
            .HasOne(x => x.QualitySpecification)
            .WithMany()
            .HasForeignKey(x => x.QualitySpecificationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LaboratoryTest>()
            .HasOne(x => x.TesterUser)
            .WithMany()
            .HasForeignKey(x => x.TesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LaboratoryTestParameterResult>()
            .HasOne(x => x.LaboratoryTest)
            .WithMany(x => x.ParameterResults)
            .HasForeignKey(x => x.LaboratoryTestId);

        modelBuilder.Entity<LaboratoryTestParameterResult>()
            .HasOne(x => x.QualitySpecificationParameter)
            .WithMany()
            .HasForeignKey(x => x.QualitySpecificationParameterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QualityDecision>()
            .HasOne(x => x.LaboratoryTest)
            .WithMany()
            .HasForeignKey(x => x.LaboratoryTestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
