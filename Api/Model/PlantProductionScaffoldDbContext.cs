using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PlantProduction.Api.Model;

public partial class PlantProductionScaffoldDbContext : DbContext
{
    public PlantProductionScaffoldDbContext(DbContextOptions<PlantProductionScaffoldDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    public virtual DbSet<BatchRawMaterialConsumption> BatchRawMaterialConsumptions { get; set; }

    public virtual DbSet<BatchStepMeasuredValue> BatchStepMeasuredValues { get; set; }

    public virtual DbSet<BatchTechnologyStepRun> BatchTechnologyStepRuns { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<EquipmentTelemetryReading> EquipmentTelemetryReadings { get; set; }

    public virtual DbSet<ExtruderProgram> ExtruderPrograms { get; set; }

    public virtual DbSet<ExtruderProgramParameter> ExtruderProgramParameters { get; set; }

    public virtual DbSet<LaboratoryTest> LaboratoryTests { get; set; }

    public virtual DbSet<LaboratoryTestParameterResult> LaboratoryTestParameterResults { get; set; }

    public virtual DbSet<MaterialCategory> MaterialCategories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<ProcessDeviation> ProcessDeviations { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductForm> ProductForms { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<ProductionBatch> ProductionBatches { get; set; }

    public virtual DbSet<ProductionLine> ProductionLines { get; set; }

    public virtual DbSet<ProductionOrder> ProductionOrders { get; set; }

    public virtual DbSet<QualityDecision> QualityDecisions { get; set; }

    public virtual DbSet<QualitySpecification> QualitySpecifications { get; set; }

    public virtual DbSet<QualitySpecificationParameter> QualitySpecificationParameters { get; set; }

    public virtual DbSet<RawMaterial> RawMaterials { get; set; }

    public virtual DbSet<RawMaterialLot> RawMaterialLots { get; set; }

    public virtual DbSet<RecipeComponent> RecipeComponents { get; set; }

    public virtual DbSet<RecipeVersion> RecipeVersions { get; set; }

    public virtual DbSet<StatusHistory> StatusHistories { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<TechnologyCard> TechnologyCards { get; set; }

    public virtual DbSet<TechnologyStep> TechnologySteps { get; set; }

    public virtual DbSet<TechnologyStepParameter> TechnologyStepParameters { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_app_users");

            entity.ToTable("app_users");

            entity.HasIndex(e => e.DepartmentId, "ix_app_users_department_id");

            entity.HasIndex(e => e.Email, "ix_app_users_email").IsUnique();

            entity.HasIndex(e => e.Login, "ix_app_users_login").IsUnique();

            entity.HasIndex(e => e.UserRoleId, "ix_app_users_user_role_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(200)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Login)
                .HasMaxLength(64)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(128)
                .HasColumnName("password_salt");
            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");

            entity.HasOne(d => d.Department).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_app_users_departments_department_id");

            entity.HasOne(d => d.UserRole).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_app_users_user_roles_user_role_id");
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_audit_events");

            entity.ToTable("audit_events");

            entity.HasIndex(e => e.ActingUserId, "ix_audit_events_acting_user_id");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.OccurredAt }, "ix_audit_events_entity_type_entity_id_occurred_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActingUserId).HasColumnName("acting_user_id");
            entity.Property(e => e.ActionName)
                .HasMaxLength(64)
                .HasColumnName("action_name");
            entity.Property(e => e.Details)
                .HasMaxLength(2000)
                .HasColumnName("details");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(64)
                .HasColumnName("entity_type");
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");

            entity.HasOne(d => d.ActingUser).WithMany(p => p.AuditEvents)
                .HasForeignKey(d => d.ActingUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_audit_events_app_users_acting_user_id");
        });

        modelBuilder.Entity<BatchRawMaterialConsumption>(entity =>
        {
            entity.HasKey(e => new { e.ProductionBatchId, e.RawMaterialLotId }).HasName("pk_batch_raw_material_consumptions");

            entity.ToTable("batch_raw_material_consumptions");

            entity.HasIndex(e => e.RawMaterialLotId, "ix_batch_raw_material_consumptions_raw_material_lot_id");

            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.RawMaterialLotId).HasColumnName("raw_material_lot_id");
            entity.Property(e => e.QuantityUsed)
                .HasPrecision(18, 3)
                .HasColumnName("quantity_used");

            entity.HasOne(d => d.ProductionBatch).WithMany(p => p.BatchRawMaterialConsumptions)
                .HasForeignKey(d => d.ProductionBatchId)
                .HasConstraintName("fk_batch_raw_material_consumptions_production_batches_producti");

            entity.HasOne(d => d.RawMaterialLot).WithMany(p => p.BatchRawMaterialConsumptions)
                .HasForeignKey(d => d.RawMaterialLotId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_batch_raw_material_consumptions_raw_material_lots_raw_mater");
        });

        modelBuilder.Entity<BatchStepMeasuredValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_batch_step_measured_values");

            entity.ToTable("batch_step_measured_values");

            entity.HasIndex(e => new { e.BatchTechnologyStepRunId, e.TechnologyStepParameterId }, "ix_batch_step_measured_values_batch_technology_step_run_id_tec").IsUnique();

            entity.HasIndex(e => e.TechnologyStepParameterId, "ix_batch_step_measured_values_technology_step_parameter_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualBooleanValue).HasColumnName("actual_boolean_value");
            entity.Property(e => e.ActualNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("actual_numeric_value");
            entity.Property(e => e.ActualTextValue)
                .HasMaxLength(256)
                .HasColumnName("actual_text_value");
            entity.Property(e => e.BatchTechnologyStepRunId).HasColumnName("batch_technology_step_run_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.IsWithinTolerance).HasColumnName("is_within_tolerance");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");
            entity.Property(e => e.TechnologyStepParameterId).HasColumnName("technology_step_parameter_id");

            entity.HasOne(d => d.BatchTechnologyStepRun).WithMany(p => p.BatchStepMeasuredValues)
                .HasForeignKey(d => d.BatchTechnologyStepRunId)
                .HasConstraintName("fk_batch_step_measured_values_batch_technology_step_runs_batch");

            entity.HasOne(d => d.TechnologyStepParameter).WithMany(p => p.BatchStepMeasuredValues)
                .HasForeignKey(d => d.TechnologyStepParameterId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_batch_step_measured_values_technology_step_parameters_techn");
        });

        modelBuilder.Entity<BatchTechnologyStepRun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_batch_technology_step_runs");

            entity.ToTable("batch_technology_step_runs");

            entity.HasIndex(e => e.CompletedByUserId, "ix_batch_technology_step_runs_completed_by_user_id");

            entity.HasIndex(e => new { e.ProductionBatchId, e.TechnologyStepId }, "ix_batch_technology_step_runs_production_batch_id_technology_s").IsUnique();

            entity.HasIndex(e => e.StartedByUserId, "ix_batch_technology_step_runs_started_by_user_id");

            entity.HasIndex(e => e.TechnologyStepId, "ix_batch_technology_step_runs_technology_step_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletedByUserId).HasColumnName("completed_by_user_id");
            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.StartedByUserId).HasColumnName("started_by_user_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TechnologyStepId).HasColumnName("technology_step_id");

            entity.HasOne(d => d.CompletedByUser).WithMany(p => p.BatchTechnologyStepRunCompletedByUsers)
                .HasForeignKey(d => d.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_batch_technology_step_runs_app_users_completed_by_user_id");

            entity.HasOne(d => d.ProductionBatch).WithMany(p => p.BatchTechnologyStepRuns)
                .HasForeignKey(d => d.ProductionBatchId)
                .HasConstraintName("fk_batch_technology_step_runs_production_batches_production_ba");

            entity.HasOne(d => d.StartedByUser).WithMany(p => p.BatchTechnologyStepRunStartedByUsers)
                .HasForeignKey(d => d.StartedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_batch_technology_step_runs_app_users_started_by_user_id");

            entity.HasOne(d => d.TechnologyStep).WithMany(p => p.BatchTechnologyStepRuns)
                .HasForeignKey(d => d.TechnologyStepId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_batch_technology_step_runs_technology_steps_technology_step");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_departments");

            entity.ToTable("departments");

            entity.HasIndex(e => e.Code, "ix_departments_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_departments_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_equipment");

            entity.ToTable("equipment");

            entity.HasIndex(e => e.Code, "ix_equipment_code").IsUnique();

            entity.HasIndex(e => e.ProductionLineId, "ix_equipment_production_line_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.EquipmentType).HasColumnName("equipment_type");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.ProductionLineId).HasColumnName("production_line_id");

            entity.HasOne(d => d.ProductionLine).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.ProductionLineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_equipment_production_lines_production_line_id");
        });

        modelBuilder.Entity<EquipmentTelemetryReading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_equipment_telemetry_readings");

            entity.ToTable("equipment_telemetry_readings");

            entity.HasIndex(e => e.EquipmentId, "ix_equipment_telemetry_readings_equipment_id");

            entity.HasIndex(e => new { e.ProductionBatchId, e.RecordedAt }, "ix_equipment_telemetry_readings_production_batch_id_recorded_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
            entity.Property(e => e.MetricName)
                .HasMaxLength(64)
                .HasColumnName("metric_name");
            entity.Property(e => e.NumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("numeric_value");
            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");
            entity.Property(e => e.ZoneName)
                .HasMaxLength(64)
                .HasColumnName("zone_name");

            entity.HasOne(d => d.Equipment).WithMany(p => p.EquipmentTelemetryReadings)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_equipment_telemetry_readings_equipment_equipment_id");

            entity.HasOne(d => d.ProductionBatch).WithMany(p => p.EquipmentTelemetryReadings)
                .HasForeignKey(d => d.ProductionBatchId)
                .HasConstraintName("fk_equipment_telemetry_readings_production_batches_production_");
        });

        modelBuilder.Entity<ExtruderProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_extruder_programs");

            entity.ToTable("extruder_programs");

            entity.HasIndex(e => new { e.Id, e.ProductId }, "ak_extruder_programs_id_product_id").IsUnique();

            entity.HasIndex(e => new { e.ProductId, e.Code, e.VersionNumber }, "ix_extruder_programs_product_id_code_version_number").IsUnique();

            entity.HasIndex(e => new { e.TechnologyCardId, e.ProductId }, "ix_extruder_programs_technology_card_id_product_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.TechnologyCardId).HasColumnName("technology_card_id");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.Product).WithMany(p => p.ExtruderPrograms)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_extruder_programs_products_product_id");

            entity.HasOne(d => d.TechnologyCard).WithMany(p => p.ExtruderPrograms)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.TechnologyCardId, d.ProductId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_extruder_programs_technology_cards_technology_card_id_produ");
        });

        modelBuilder.Entity<ExtruderProgramParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_extruder_program_parameters");

            entity.ToTable("extruder_program_parameters");

            entity.HasIndex(e => new { e.ExtruderProgramId, e.SortOrder }, "ix_extruder_program_parameters_extruder_program_id_sort_order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExtruderProgramId).HasColumnName("extruder_program_id");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TargetBooleanValue).HasColumnName("target_boolean_value");
            entity.Property(e => e.TargetNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("target_numeric_value");
            entity.Property(e => e.TargetTextValue)
                .HasMaxLength(256)
                .HasColumnName("target_text_value");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");
            entity.Property(e => e.ValueType).HasColumnName("value_type");
            entity.Property(e => e.ZoneNumber).HasColumnName("zone_number");

            entity.HasOne(d => d.ExtruderProgram).WithMany(p => p.ExtruderProgramParameters)
                .HasForeignKey(d => d.ExtruderProgramId)
                .HasConstraintName("fk_extruder_program_parameters_extruder_programs_extruder_prog");
        });

        modelBuilder.Entity<LaboratoryTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_laboratory_tests");

            entity.ToTable("laboratory_tests");

            entity.HasIndex(e => e.ProductionBatchId, "ix_laboratory_tests_production_batch_id");

            entity.HasIndex(e => e.QualitySpecificationId, "ix_laboratory_tests_quality_specification_id");

            entity.HasIndex(e => e.RawMaterialLotId, "ix_laboratory_tests_raw_material_lot_id");

            entity.HasIndex(e => new { e.SubjectType, e.RawMaterialLotId, e.ProductionBatchId }, "ix_laboratory_tests_subject_type_raw_material_lot_id_productio");

            entity.HasIndex(e => e.TestNumber, "ix_laboratory_tests_test_number").IsUnique();

            entity.HasIndex(e => e.TesterUserId, "ix_laboratory_tests_tester_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.QualitySpecificationId).HasColumnName("quality_specification_id");
            entity.Property(e => e.RawMaterialLotId).HasColumnName("raw_material_lot_id");
            entity.Property(e => e.ResultSummary)
                .HasMaxLength(1000)
                .HasColumnName("result_summary");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubjectType).HasColumnName("subject_type");
            entity.Property(e => e.TestKind)
                .HasMaxLength(128)
                .HasColumnName("test_kind");
            entity.Property(e => e.TestNumber)
                .HasMaxLength(64)
                .HasColumnName("test_number");
            entity.Property(e => e.TesterUserId).HasColumnName("tester_user_id");

            entity.HasOne(d => d.ProductionBatch).WithMany(p => p.LaboratoryTests)
                .HasForeignKey(d => d.ProductionBatchId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_laboratory_tests_production_batches_production_batch_id");

            entity.HasOne(d => d.QualitySpecification).WithMany(p => p.LaboratoryTests)
                .HasForeignKey(d => d.QualitySpecificationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_laboratory_tests_quality_specifications_quality_specificati");

            entity.HasOne(d => d.RawMaterialLot).WithMany(p => p.LaboratoryTests)
                .HasForeignKey(d => d.RawMaterialLotId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_laboratory_tests_raw_material_lots_raw_material_lot_id");

            entity.HasOne(d => d.TesterUser).WithMany(p => p.LaboratoryTests)
                .HasForeignKey(d => d.TesterUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_laboratory_tests_app_users_tester_user_id");
        });

        modelBuilder.Entity<LaboratoryTestParameterResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_laboratory_test_parameter_results");

            entity.ToTable("laboratory_test_parameter_results");

            entity.HasIndex(e => new { e.LaboratoryTestId, e.SortOrder }, "ix_laboratory_test_parameter_results_laboratory_test_id_sort_o").IsUnique();

            entity.HasIndex(e => e.QualitySpecificationParameterId, "ix_laboratory_test_parameter_results_quality_specification_par");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualBooleanValue).HasColumnName("actual_boolean_value");
            entity.Property(e => e.ActualNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("actual_numeric_value");
            entity.Property(e => e.ActualTextValue)
                .HasMaxLength(256)
                .HasColumnName("actual_text_value");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.IsWithinRange).HasColumnName("is_within_range");
            entity.Property(e => e.LaboratoryTestId).HasColumnName("laboratory_test_id");
            entity.Property(e => e.MaxNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("max_numeric_value");
            entity.Property(e => e.MinNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("min_numeric_value");
            entity.Property(e => e.ParameterName)
                .HasMaxLength(128)
                .HasColumnName("parameter_name");
            entity.Property(e => e.QualitySpecificationParameterId).HasColumnName("quality_specification_parameter_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TargetBooleanValue).HasColumnName("target_boolean_value");
            entity.Property(e => e.TargetTextValue)
                .HasMaxLength(256)
                .HasColumnName("target_text_value");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");
            entity.Property(e => e.ValueType).HasColumnName("value_type");

            entity.HasOne(d => d.LaboratoryTest).WithMany(p => p.LaboratoryTestParameterResults)
                .HasForeignKey(d => d.LaboratoryTestId)
                .HasConstraintName("fk_laboratory_test_parameter_results_laboratory_tests_laborato");

            entity.HasOne(d => d.QualitySpecificationParameter).WithMany(p => p.LaboratoryTestParameterResults)
                .HasForeignKey(d => d.QualitySpecificationParameterId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_laboratory_test_parameter_results_quality_specification_par");
        });

        modelBuilder.Entity<MaterialCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_material_categories");

            entity.ToTable("material_categories");

            entity.HasIndex(e => e.Code, "ix_material_categories_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_material_categories_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_notifications");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.RecipientUserId, e.IsRead, e.CreatedAt }, "ix_notifications_recipient_user_id_is_read_created_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(64)
                .HasColumnName("entity_type");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.RecipientUserId).HasColumnName("recipient_user_id");
            entity.Property(e => e.Severity).HasColumnName("severity");

            entity.HasOne(d => d.RecipientUser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_notifications_app_users_recipient_user_id");
        });

        modelBuilder.Entity<ProcessDeviation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_process_deviations");

            entity.ToTable("process_deviations");

            entity.HasIndex(e => e.BatchTechnologyStepRunId, "ix_process_deviations_batch_technology_step_run_id");

            entity.HasIndex(e => new { e.ProductionBatchId, e.CreatedAt }, "ix_process_deviations_production_batch_id_created_at");

            entity.HasIndex(e => e.ReportedByUserId, "ix_process_deviations_reported_by_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualValue)
                .HasMaxLength(256)
                .HasColumnName("actual_value");
            entity.Property(e => e.BatchTechnologyStepRunId).HasColumnName("batch_technology_step_run_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Details)
                .HasMaxLength(2000)
                .HasColumnName("details");
            entity.Property(e => e.ParameterName)
                .HasMaxLength(128)
                .HasColumnName("parameter_name");
            entity.Property(e => e.PlannedValue)
                .HasMaxLength(256)
                .HasColumnName("planned_value");
            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.ReportedByUserId).HasColumnName("reported_by_user_id");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.BatchTechnologyStepRun).WithMany(p => p.ProcessDeviations)
                .HasForeignKey(d => d.BatchTechnologyStepRunId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_process_deviations_batch_technology_step_runs_batch_technol");

            entity.HasOne(d => d.ProductionBatch).WithMany(p => p.ProcessDeviations)
                .HasForeignKey(d => d.ProductionBatchId)
                .HasConstraintName("fk_process_deviations_production_batches_production_batch_id");

            entity.HasOne(d => d.ReportedByUser).WithMany(p => p.ProcessDeviations)
                .HasForeignKey(d => d.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_process_deviations_app_users_reported_by_user_id");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_products");

            entity.ToTable("products");

            entity.HasIndex(e => e.Code, "ix_products_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_products_name").IsUnique();

            entity.HasIndex(e => e.ProductFormId, "ix_products_product_form_id");

            entity.HasIndex(e => e.ProductTypeId, "ix_products_product_type_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ProductFormId).HasColumnName("product_form_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.ProductForm).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductFormId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_products_product_forms_product_form_id");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_products_product_types_product_type_id");
        });

        modelBuilder.Entity<ProductForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_product_forms");

            entity.ToTable("product_forms");

            entity.HasIndex(e => e.Code, "ix_product_forms_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_product_forms_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_product_types");

            entity.ToTable("product_types");

            entity.HasIndex(e => e.Code, "ix_product_types_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_product_types_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProductionBatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_production_batches");

            entity.ToTable("production_batches");

            entity.HasIndex(e => e.BatchNumber, "ix_production_batches_batch_number").IsUnique();

            entity.HasIndex(e => new { e.ExtruderProgramId, e.ProductId }, "ix_production_batches_extruder_program_id_product_id");

            entity.HasIndex(e => new { e.ProductId, e.Status }, "ix_production_batches_product_id_status");

            entity.HasIndex(e => e.ProductionLineId, "ix_production_batches_production_line_id");

            entity.HasIndex(e => new { e.ProductionOrderId, e.ProductId, e.ProductionLineId }, "ix_production_batches_production_order_id_product_id_productio");

            entity.HasIndex(e => new { e.RecipeVersionId, e.ProductId }, "ix_production_batches_recipe_version_id_product_id");

            entity.HasIndex(e => new { e.TechnologyCardId, e.ProductId }, "ix_production_batches_technology_card_id_product_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatchNumber)
                .HasMaxLength(64)
                .HasColumnName("batch_number");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ExtruderProgramId).HasColumnName("extruder_program_id");
            entity.Property(e => e.PlannedQuantity)
                .HasPrecision(18, 3)
                .HasColumnName("planned_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductionLineId).HasColumnName("production_line_id");
            entity.Property(e => e.ProductionOrderId).HasColumnName("production_order_id");
            entity.Property(e => e.RecipeVersionId).HasColumnName("recipe_version_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TechnologyCardId).HasColumnName("technology_card_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductionBatches)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_products_product_id");

            entity.HasOne(d => d.ProductionLine).WithMany(p => p.ProductionBatches)
                .HasForeignKey(d => d.ProductionLineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_production_lines_production_line_id");

            entity.HasOne(d => d.ExtruderProgram).WithMany(p => p.ProductionBatches)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.ExtruderProgramId, d.ProductId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_extruder_programs_extruder_program_id_pr");

            entity.HasOne(d => d.RecipeVersion).WithMany(p => p.ProductionBatches)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.RecipeVersionId, d.ProductId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_recipe_versions_recipe_version_id_produc");

            entity.HasOne(d => d.TechnologyCard).WithMany(p => p.ProductionBatches)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.TechnologyCardId, d.ProductId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_technology_cards_technology_card_id_prod");

            entity.HasOne(d => d.ProductionOrder).WithMany(p => p.ProductionBatches)
                .HasPrincipalKey(p => new { p.Id, p.ProductId, p.ProductionLineId })
                .HasForeignKey(d => new { d.ProductionOrderId, d.ProductId, d.ProductionLineId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_batches_production_orders_production_order_id_pr");
        });

        modelBuilder.Entity<ProductionLine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_production_lines");

            entity.ToTable("production_lines");

            entity.HasIndex(e => e.Code, "ix_production_lines_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_production_lines_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_production_orders");

            entity.ToTable("production_orders");

            entity.HasIndex(e => new { e.Id, e.ProductId, e.ProductionLineId }, "ak_production_orders_id_product_id_production_line_id").IsUnique();

            entity.HasIndex(e => e.CreatedByUserId, "ix_production_orders_created_by_user_id");

            entity.HasIndex(e => e.OrderNumber, "ix_production_orders_order_number").IsUnique();

            entity.HasIndex(e => new { e.ProductId, e.Status }, "ix_production_orders_product_id_status");

            entity.HasIndex(e => e.ProductionLineId, "ix_production_orders_production_line_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(64)
                .HasColumnName("order_number");
            entity.Property(e => e.PlannedQuantity)
                .HasPrecision(18, 3)
                .HasColumnName("planned_quantity");
            entity.Property(e => e.PlannedStartAt).HasColumnName("planned_start_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductionLineId).HasColumnName("production_line_id");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ProductionOrders)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_orders_app_users_created_by_user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductionOrders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_orders_products_product_id");

            entity.HasOne(d => d.ProductionLine).WithMany(p => p.ProductionOrders)
                .HasForeignKey(d => d.ProductionLineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_production_orders_production_lines_production_line_id");
        });

        modelBuilder.Entity<QualityDecision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_quality_decisions");

            entity.ToTable("quality_decisions");

            entity.HasIndex(e => e.DecidedByUserId, "ix_quality_decisions_decided_by_user_id");

            entity.HasIndex(e => e.LaboratoryTestId, "ix_quality_decisions_laboratory_test_id");

            entity.HasIndex(e => e.ProductionBatchId, "ix_quality_decisions_production_batch_id")
                .IsUnique()
                .HasFilter("(is_current AND (production_batch_id IS NOT NULL))");

            entity.HasIndex(e => e.RawMaterialLotId, "ix_quality_decisions_raw_material_lot_id")
                .IsUnique()
                .HasFilter("(is_current AND (raw_material_lot_id IS NOT NULL))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockReason)
                .HasMaxLength(1000)
                .HasColumnName("block_reason");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.DecidedAt).HasColumnName("decided_at");
            entity.Property(e => e.DecidedByUserId).HasColumnName("decided_by_user_id");
            entity.Property(e => e.DecisionStatus).HasColumnName("decision_status");
            entity.Property(e => e.IsCurrent).HasColumnName("is_current");
            entity.Property(e => e.LaboratoryTestId).HasColumnName("laboratory_test_id");
            entity.Property(e => e.ProductionBatchId).HasColumnName("production_batch_id");
            entity.Property(e => e.RawMaterialLotId).HasColumnName("raw_material_lot_id");
            entity.Property(e => e.SubjectType).HasColumnName("subject_type");

            entity.HasOne(d => d.DecidedByUser).WithMany(p => p.QualityDecisions)
                .HasForeignKey(d => d.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_decisions_app_users_decided_by_user_id");

            entity.HasOne(d => d.LaboratoryTest).WithMany(p => p.QualityDecisions)
                .HasForeignKey(d => d.LaboratoryTestId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_decisions_laboratory_tests_laboratory_test_id");

            entity.HasOne(d => d.ProductionBatch).WithOne(p => p.QualityDecision)
                .HasForeignKey<QualityDecision>(d => d.ProductionBatchId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_decisions_production_batches_production_batch_id");

            entity.HasOne(d => d.RawMaterialLot).WithOne(p => p.QualityDecision)
                .HasForeignKey<QualityDecision>(d => d.RawMaterialLotId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_decisions_raw_material_lots_raw_material_lot_id");
        });

        modelBuilder.Entity<QualitySpecification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_quality_specifications");

            entity.ToTable("quality_specifications");

            entity.HasIndex(e => new { e.Code, e.VersionNumber }, "ix_quality_specifications_code_version_number").IsUnique();

            entity.HasIndex(e => e.ProductId, "ix_quality_specifications_product_id")
                .IsUnique()
                .HasFilter("((subject_type = 2) AND (status = 2) AND is_active AND (product_id IS NOT NULL))");

            entity.HasIndex(e => e.RawMaterialId, "ix_quality_specifications_raw_material_id")
                .IsUnique()
                .HasFilter("((subject_type = 1) AND (status = 2) AND is_active AND (raw_material_id IS NOT NULL))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.RawMaterialId).HasColumnName("raw_material_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubjectType).HasColumnName("subject_type");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.Product).WithOne(p => p.QualitySpecification)
                .HasForeignKey<QualitySpecification>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_specifications_products_product_id");

            entity.HasOne(d => d.RawMaterial).WithOne(p => p.QualitySpecification)
                .HasForeignKey<QualitySpecification>(d => d.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_quality_specifications_raw_materials_raw_material_id");
        });

        modelBuilder.Entity<QualitySpecificationParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_quality_specification_parameters");

            entity.ToTable("quality_specification_parameters");

            entity.HasIndex(e => new { e.QualitySpecificationId, e.Name }, "ix_quality_specification_parameters_quality_specification_id_n").IsUnique();

            entity.HasIndex(e => new { e.QualitySpecificationId, e.SortOrder }, "ix_quality_specification_parameters_quality_specification_id_s").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.MaxNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("max_numeric_value");
            entity.Property(e => e.MinNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("min_numeric_value");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.QualitySpecificationId).HasColumnName("quality_specification_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TargetBooleanValue).HasColumnName("target_boolean_value");
            entity.Property(e => e.TargetTextValue)
                .HasMaxLength(256)
                .HasColumnName("target_text_value");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");
            entity.Property(e => e.ValueType).HasColumnName("value_type");

            entity.HasOne(d => d.QualitySpecification).WithMany(p => p.QualitySpecificationParameters)
                .HasForeignKey(d => d.QualitySpecificationId)
                .HasConstraintName("fk_quality_specification_parameters_quality_specifications_qua");
        });

        modelBuilder.Entity<RawMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_raw_materials");

            entity.ToTable("raw_materials");

            entity.HasIndex(e => e.Code, "ix_raw_materials_code").IsUnique();

            entity.HasIndex(e => e.MaterialCategoryId, "ix_raw_materials_material_category_id");

            entity.HasIndex(e => e.Name, "ix_raw_materials_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.MaterialCategoryId).HasColumnName("material_category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");

            entity.HasOne(d => d.MaterialCategory).WithMany(p => p.RawMaterials)
                .HasForeignKey(d => d.MaterialCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_raw_materials_material_categories_material_category_id");
        });

        modelBuilder.Entity<RawMaterialLot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_raw_material_lots");

            entity.ToTable("raw_material_lots");

            entity.HasIndex(e => e.InternalLotNumber, "ix_raw_material_lots_internal_lot_number").IsUnique();

            entity.HasIndex(e => new { e.RawMaterialId, e.Status }, "ix_raw_material_lots_raw_material_id_status");

            entity.HasIndex(e => new { e.SupplierId, e.SupplierLotNumber }, "ix_raw_material_lots_supplier_id_supplier_lot_number").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InternalLotNumber)
                .HasMaxLength(64)
                .HasColumnName("internal_lot_number");
            entity.Property(e => e.LastLaboratoryDecisionAt).HasColumnName("last_laboratory_decision_at");
            entity.Property(e => e.QuantityAvailable)
                .HasPrecision(18, 3)
                .HasColumnName("quantity_available");
            entity.Property(e => e.QuantityReceived)
                .HasPrecision(18, 3)
                .HasColumnName("quantity_received");
            entity.Property(e => e.RawMaterialId).HasColumnName("raw_material_id");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StorageLocation)
                .HasMaxLength(64)
                .HasColumnName("storage_location");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierLotNumber)
                .HasMaxLength(64)
                .HasColumnName("supplier_lot_number");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.RawMaterialLots)
                .HasForeignKey(d => d.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_raw_material_lots_raw_materials_raw_material_id");

            entity.HasOne(d => d.Supplier).WithMany(p => p.RawMaterialLots)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_raw_material_lots_suppliers_supplier_id");
        });

        modelBuilder.Entity<RecipeComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_recipe_components");

            entity.ToTable("recipe_components");

            entity.HasIndex(e => e.RawMaterialId, "ix_recipe_components_raw_material_id");

            entity.HasIndex(e => new { e.RecipeVersionId, e.LoadOrder }, "ix_recipe_components_recipe_version_id_load_order").IsUnique();

            entity.HasIndex(e => new { e.RecipeVersionId, e.RawMaterialId }, "ix_recipe_components_recipe_version_id_raw_material_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllowedDeviationPercent)
                .HasPrecision(5, 2)
                .HasColumnName("allowed_deviation_percent");
            entity.Property(e => e.LoadOrder).HasColumnName("load_order");
            entity.Property(e => e.Percentage)
                .HasPrecision(5, 2)
                .HasColumnName("percentage");
            entity.Property(e => e.RawMaterialId).HasColumnName("raw_material_id");
            entity.Property(e => e.RecipeVersionId).HasColumnName("recipe_version_id");

            entity.HasOne(d => d.RawMaterial).WithMany(p => p.RecipeComponents)
                .HasForeignKey(d => d.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_recipe_components_raw_materials_raw_material_id");

            entity.HasOne(d => d.RecipeVersion).WithMany(p => p.RecipeComponents)
                .HasForeignKey(d => d.RecipeVersionId)
                .HasConstraintName("fk_recipe_components_recipe_versions_recipe_version_id");
        });

        modelBuilder.Entity<RecipeVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_recipe_versions");

            entity.ToTable("recipe_versions");

            entity.HasIndex(e => new { e.Id, e.ProductId }, "ak_recipe_versions_id_product_id").IsUnique();

            entity.HasIndex(e => e.ApprovedByUserId, "ix_recipe_versions_approved_by_user_id");

            entity.HasIndex(e => e.CreatedByUserId, "ix_recipe_versions_created_by_user_id");

            entity.HasIndex(e => e.ProductId, "ix_recipe_versions_product_id")
                .IsUnique()
                .HasFilter("((status = 3) AND is_active)");

            entity.HasIndex(e => new { e.ProductId, e.VersionNumber }, "ix_recipe_versions_product_id_version_number").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.RecipeVersionApprovedByUsers)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_recipe_versions_app_users_approved_by_user_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.RecipeVersionCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_recipe_versions_app_users_created_by_user_id");

            entity.HasOne(d => d.Product).WithOne(p => p.RecipeVersion)
                .HasForeignKey<RecipeVersion>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_recipe_versions_products_product_id");
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_status_histories");

            entity.ToTable("status_histories");

            entity.HasIndex(e => e.ChangedByUserId, "ix_status_histories_changed_by_user_id");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.ChangedAt }, "ix_status_histories_entity_type_entity_id_changed_at");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.ChangedByUserId).HasColumnName("changed_by_user_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(64)
                .HasColumnName("entity_type");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(64)
                .HasColumnName("new_status");
            entity.Property(e => e.PreviousStatus)
                .HasMaxLength(64)
                .HasColumnName("previous_status");

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_status_histories_app_users_changed_by_user_id");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_suppliers");

            entity.ToTable("suppliers");

            entity.HasIndex(e => e.Code, "ix_suppliers_code").IsUnique();

            entity.HasIndex(e => e.TaxId, "ix_suppliers_tax_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.TaxId)
                .HasMaxLength(16)
                .HasColumnName("tax_id");
        });

        modelBuilder.Entity<TechnologyCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_technology_cards");

            entity.ToTable("technology_cards");

            entity.HasIndex(e => new { e.Id, e.ProductId }, "ak_technology_cards_id_product_id").IsUnique();

            entity.HasIndex(e => e.ApprovedByUserId, "ix_technology_cards_approved_by_user_id");

            entity.HasIndex(e => e.CreatedByUserId, "ix_technology_cards_created_by_user_id");

            entity.HasIndex(e => e.ProductId, "ix_technology_cards_product_id")
                .IsUnique()
                .HasFilter("((status = 3) AND is_active)");

            entity.HasIndex(e => new { e.ProductId, e.VersionNumber }, "ix_technology_cards_product_id_version_number").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.TechnologyCardApprovedByUsers)
                .HasForeignKey(d => d.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_technology_cards_app_users_approved_by_user_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TechnologyCardCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_technology_cards_app_users_created_by_user_id");

            entity.HasOne(d => d.Product).WithOne(p => p.TechnologyCard)
                .HasForeignKey<TechnologyCard>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_technology_cards_products_product_id");
        });

        modelBuilder.Entity<TechnologyStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_technology_steps");

            entity.ToTable("technology_steps");

            entity.HasIndex(e => new { e.TechnologyCardId, e.StepOrder }, "ix_technology_steps_technology_card_id_step_order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpectedDurationMinutes).HasColumnName("expected_duration_minutes");
            entity.Property(e => e.Instruction)
                .HasMaxLength(2000)
                .HasColumnName("instruction");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.StepOrder).HasColumnName("step_order");
            entity.Property(e => e.StepType).HasColumnName("step_type");
            entity.Property(e => e.TechnologyCardId).HasColumnName("technology_card_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.TechnologyCard).WithMany(p => p.TechnologySteps)
                .HasForeignKey(d => d.TechnologyCardId)
                .HasConstraintName("fk_technology_steps_technology_cards_technology_card_id");
        });

        modelBuilder.Entity<TechnologyStepParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_technology_step_parameters");

            entity.ToTable("technology_step_parameters");

            entity.HasIndex(e => new { e.TechnologyStepId, e.Name }, "ix_technology_step_parameters_technology_step_id_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.MaxNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("max_numeric_value");
            entity.Property(e => e.MinNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("min_numeric_value");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.TargetBooleanValue).HasColumnName("target_boolean_value");
            entity.Property(e => e.TargetNumericValue)
                .HasPrecision(18, 3)
                .HasColumnName("target_numeric_value");
            entity.Property(e => e.TargetTextValue)
                .HasMaxLength(256)
                .HasColumnName("target_text_value");
            entity.Property(e => e.TechnologyStepId).HasColumnName("technology_step_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(16)
                .HasColumnName("unit");
            entity.Property(e => e.ValueType).HasColumnName("value_type");

            entity.HasOne(d => d.TechnologyStep).WithMany(p => p.TechnologyStepParameters)
                .HasForeignKey(d => d.TechnologyStepId)
                .HasConstraintName("fk_technology_step_parameters_technology_steps_technology_step");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_user_roles");

            entity.ToTable("user_roles");

            entity.HasIndex(e => e.Code, "ix_user_roles_code").IsUnique();

            entity.HasIndex(e => e.Name, "ix_user_roles_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
