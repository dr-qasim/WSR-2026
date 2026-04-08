INSERT INTO departments (id, code, name)
VALUES (1, 'TECH', 'Technology Department');
INSERT INTO departments (id, code, name)
VALUES (2, 'LAB', 'Laboratory Department');
INSERT INTO departments (id, code, name)
VALUES (3, 'PROD', 'Production Department');
INSERT INTO departments (id, code, name)
VALUES (4, 'ADMIN', 'Administration');

INSERT INTO material_categories (id, code, name)
VALUES (1, 'ACTIVE', 'Active Ingredient');
INSERT INTO material_categories (id, code, name)
VALUES (2, 'CARRIER', 'Carrier');
INSERT INTO material_categories (id, code, name)
VALUES (3, 'ADDITIVE', 'Additive');

INSERT INTO product_forms (id, code, name)
VALUES (1, 'SC', 'Suspension Concentrate');
INSERT INTO product_forms (id, code, name)
VALUES (2, 'WDG', 'Water Dispersible Granules');

INSERT INTO product_types (id, code, name)
VALUES (1, 'HERBICIDE', 'Herbicide');
INSERT INTO product_types (id, code, name)
VALUES (2, 'FUNGICIDE', 'Fungicide');

INSERT INTO production_lines (id, code, is_active, name)
VALUES (1, 'LINE-MIX-01', TRUE, 'Mixing Line 01');
INSERT INTO production_lines (id, code, is_active, name)
VALUES (2, 'LINE-EXT-01', TRUE, 'Extrusion Line 01');

INSERT INTO suppliers (id, code, name, tax_id)
VALUES (1, 'AGROCHEM', 'AgroChem Supply', '7701001001');
INSERT INTO suppliers (id, code, name, tax_id)
VALUES (2, 'RUSREAGENT', 'RusReagent Trade', '7701001002');

INSERT INTO user_roles (id, code, name)
VALUES (1, 'TECHNOLOGIST', 'Technologist');
INSERT INTO user_roles (id, code, name)
VALUES (2, 'LAB_TECHNICIAN', 'Laboratory Technician');
INSERT INTO user_roles (id, code, name)
VALUES (3, 'OPERATOR', 'Operator');
INSERT INTO user_roles (id, code, name)
VALUES (4, 'ADMINISTRATOR', 'Administrator');

INSERT INTO app_users (id, created_at, department_id, email, full_name, is_active, login, password_hash, password_salt, user_role_id)
VALUES (1, TIMESTAMPTZ '2026-01-10T08:00:00Z', 1, 'technologist1@wsr2026.local', 'Ivan Petrov', TRUE, 'technologist1', 'seed-hash-tech', 'seed-salt-tech', 1);
INSERT INTO app_users (id, created_at, department_id, email, full_name, is_active, login, password_hash, password_salt, user_role_id)
VALUES (2, TIMESTAMPTZ '2026-01-10T08:05:00Z', 2, 'lab1@wsr2026.local', 'Elena Smirnova', TRUE, 'lab1', 'seed-hash-lab', 'seed-salt-lab', 2);
INSERT INTO app_users (id, created_at, department_id, email, full_name, is_active, login, password_hash, password_salt, user_role_id)
VALUES (3, TIMESTAMPTZ '2026-01-10T08:10:00Z', 3, 'operator1@wsr2026.local', 'Sergey Ivanov', TRUE, 'operator1', 'seed-hash-operator', 'seed-salt-operator', 3);
INSERT INTO app_users (id, created_at, department_id, email, full_name, is_active, login, password_hash, password_salt, user_role_id)
VALUES (4, TIMESTAMPTZ '2026-01-10T08:15:00Z', 4, 'admin@wsr2026.local', 'Anna Volkova', TRUE, 'admin', 'seed-hash-admin', 'seed-salt-admin', 4);

INSERT INTO equipment (id, code, equipment_type, is_active, name, production_line_id)
VALUES (1, 'MIXER-01', 2, TRUE, 'Primary Mixer', 1);
INSERT INTO equipment (id, code, equipment_type, is_active, name, production_line_id)
VALUES (2, 'PACK-01', 4, TRUE, 'Packaging Station', 1);
INSERT INTO equipment (id, code, equipment_type, is_active, name, production_line_id)
VALUES (3, 'EXTR-01', 1, TRUE, 'Granule Extruder', 2);
INSERT INTO equipment (id, code, equipment_type, is_active, name, production_line_id)
VALUES (4, 'COOL-01', 3, TRUE, 'Cooling Chamber', 2);

INSERT INTO products (id, code, description, name, product_form_id, product_type_id, status)
VALUES (1, 'PRD-HB-001', 'Suspension concentrate herbicide for cereal crops.', 'Herbicidex SC', 1, 1, 1);
INSERT INTO products (id, code, description, name, product_form_id, product_type_id, status)
VALUES (2, 'PRD-FG-002', 'Water dispersible granules for preventive fungicidal treatment.', 'Fungistop WDG', 2, 2, 1);

INSERT INTO raw_materials (id, code, description, material_category_id, name, status, unit)
VALUES (1, 'RM-GLY-01', 'Primary herbicide active ingredient.', 1, 'Glyphosate IPA', 1, 'kg');
INSERT INTO raw_materials (id, code, description, material_category_id, name, status, unit)
VALUES (2, 'RM-BEN-01', 'Inert carrier for granules and suspensions.', 2, 'Bentonite Clay', 1, 'kg');
INSERT INTO raw_materials (id, code, description, material_category_id, name, status, unit)
VALUES (3, 'RM-SUR-01', 'Wetting and spreading additive.', 3, 'Nonionic Surfactant', 1, 'kg');
INSERT INTO raw_materials (id, code, description, material_category_id, name, status, unit)
VALUES (4, 'RM-COP-01', 'Fungicidal active ingredient.', 1, 'Copper Oxychloride', 1, 'kg');
INSERT INTO raw_materials (id, code, description, material_category_id, name, status, unit)
VALUES (5, 'RM-STA-01', 'Stabilizer for extrusion and storage.', 3, 'Granule Stabilizer', 1, 'kg');

INSERT INTO production_orders (id, created_at, created_by_user_id, order_number, planned_quantity, planned_start_at, product_id, production_line_id, status)
VALUES (1, TIMESTAMPTZ '2026-01-18T14:00:00Z', 1, 'ORD-2026-001', 500.0, TIMESTAMPTZ '2026-01-20T08:00:00Z', 1, 1, 4);
INSERT INTO production_orders (id, created_at, created_by_user_id, order_number, planned_quantity, planned_start_at, product_id, production_line_id, status)
VALUES (2, TIMESTAMPTZ '2026-01-20T15:30:00Z', 1, 'ORD-2026-002', 300.0, TIMESTAMPTZ '2026-01-22T09:00:00Z', 2, 2, 3);

INSERT INTO quality_specifications (id, code, created_at, is_active, name, product_id, raw_material_id, status, subject_type, version_number)
VALUES (1, 'QS-RM-GLY', TIMESTAMPTZ '2026-01-05T08:00:00Z', TRUE, 'Glyphosate Incoming Control', NULL, 1, 2, 1, 1);
INSERT INTO quality_specifications (id, code, created_at, is_active, name, product_id, raw_material_id, status, subject_type, version_number)
VALUES (2, 'QS-RM-COP', TIMESTAMPTZ '2026-01-05T08:15:00Z', TRUE, 'Copper Oxychloride Incoming Control', NULL, 4, 2, 1, 1);
INSERT INTO quality_specifications (id, code, created_at, is_active, name, product_id, raw_material_id, status, subject_type, version_number)
VALUES (3, 'QS-FP-HB', TIMESTAMPTZ '2026-01-12T11:00:00Z', TRUE, 'Herbicidex SC Release Control', 1, NULL, 2, 2, 1);
INSERT INTO quality_specifications (id, code, created_at, is_active, name, product_id, raw_material_id, status, subject_type, version_number)
VALUES (4, 'QS-FP-FG', TIMESTAMPTZ '2026-01-17T11:00:00Z', TRUE, 'Fungistop WDG Release Control', 2, NULL, 2, 2, 1);

INSERT INTO raw_material_lots (id, internal_lot_number, last_laboratory_decision_at, quantity_available, quantity_received, raw_material_id, received_at, status, storage_location, supplier_id, supplier_lot_number)
VALUES (1, 'LOT-RM-001', TIMESTAMPTZ '2026-01-06T10:05:00Z', 187.5, 500.0, 1, TIMESTAMPTZ '2026-01-05T09:00:00Z', 2, 'WH-A1-01', 1, 'AG-GLY-240101');
INSERT INTO raw_material_lots (id, internal_lot_number, last_laboratory_decision_at, quantity_available, quantity_received, raw_material_id, received_at, status, storage_location, supplier_id, supplier_lot_number)
VALUES (2, 'LOT-RM-002', NULL, 375.0, 600.0, 2, TIMESTAMPTZ '2026-01-05T10:00:00Z', 2, 'WH-A2-03', 2, 'RR-BEN-240102');
INSERT INTO raw_material_lots (id, internal_lot_number, last_laboratory_decision_at, quantity_available, quantity_received, raw_material_id, received_at, status, storage_location, supplier_id, supplier_lot_number)
VALUES (3, 'LOT-RM-003', NULL, 112.5, 150.0, 3, TIMESTAMPTZ '2026-01-06T08:30:00Z', 2, 'WH-B1-02', 1, 'AG-SUR-240103');
INSERT INTO raw_material_lots (id, internal_lot_number, last_laboratory_decision_at, quantity_available, quantity_received, raw_material_id, received_at, status, storage_location, supplier_id, supplier_lot_number)
VALUES (4, 'LOT-RM-004', NULL, 190.0, 400.0, 4, TIMESTAMPTZ '2026-01-06T09:00:00Z', 2, 'WH-C1-01', 2, 'RR-COP-240104');
INSERT INTO raw_material_lots (id, internal_lot_number, last_laboratory_decision_at, quantity_available, quantity_received, raw_material_id, received_at, status, storage_location, supplier_id, supplier_lot_number)
VALUES (5, 'LOT-RM-005', NULL, 85.0, 100.0, 5, TIMESTAMPTZ '2026-01-07T11:00:00Z', 2, 'WH-C2-05', 1, 'AG-STA-240105');

INSERT INTO recipe_versions (id, approved_at, approved_by_user_id, created_at, created_by_user_id, is_active, notes, product_id, status, version_number)
VALUES (1, TIMESTAMPTZ '2026-01-12T10:30:00Z', 4, TIMESTAMPTZ '2026-01-10T09:00:00Z', 1, TRUE, 'Approved base recipe for Herbicidex SC.', 1, 3, 1);
INSERT INTO recipe_versions (id, approved_at, approved_by_user_id, created_at, created_by_user_id, is_active, notes, product_id, status, version_number)
VALUES (2, NULL, NULL, TIMESTAMPTZ '2026-02-05T11:00:00Z', 1, FALSE, 'Experimental recipe revision pending balancing.', 1, 1, 2);
INSERT INTO recipe_versions (id, approved_at, approved_by_user_id, created_at, created_by_user_id, is_active, notes, product_id, status, version_number)
VALUES (3, TIMESTAMPTZ '2026-01-17T09:45:00Z', 4, TIMESTAMPTZ '2026-01-15T09:30:00Z', 1, TRUE, 'Approved granule formulation for Fungistop WDG.', 2, 3, 1);

INSERT INTO technology_cards (id, approved_at, approved_by_user_id, created_at, created_by_user_id, description, is_active, product_id, status, title, version_number)
VALUES (1, TIMESTAMPTZ '2026-01-12T10:45:00Z', 4, TIMESTAMPTZ '2026-01-10T10:00:00Z', 1, 'Mixing and packaging sequence for Herbicidex SC.', TRUE, 1, 3, 'Herbicidex SC - Base Process', 1);
INSERT INTO technology_cards (id, approved_at, approved_by_user_id, created_at, created_by_user_id, description, is_active, product_id, status, title, version_number)
VALUES (2, NULL, NULL, TIMESTAMPTZ '2026-02-06T12:00:00Z', 1, 'Draft revision with experimental holding step.', FALSE, 1, 1, 'Herbicidex SC - Pilot Revision', 2);
INSERT INTO technology_cards (id, approved_at, approved_by_user_id, created_at, created_by_user_id, description, is_active, product_id, status, title, version_number)
VALUES (3, TIMESTAMPTZ '2026-01-17T10:15:00Z', 4, TIMESTAMPTZ '2026-01-15T10:00:00Z', 1, 'Loading, extrusion and cooling sequence for fungicidal granules.', TRUE, 2, 3, 'Fungistop WDG - Extrusion Process', 1);

INSERT INTO extruder_programs (id, code, created_at, is_active, name, notes, product_id, technology_card_id, version_number)
VALUES (1, 'EXT-FG-01', TIMESTAMPTZ '2026-01-18T08:30:00Z', TRUE, 'Fungistop Standard Extrusion Program', 'Standard operating program for Fungistop WDG.', 2, 3, 1);

INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (1, TRUE, 100.0, 95.0, 'Purity', 1, 1, NULL, NULL, '%', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (2, TRUE, 2.0, 0.0, 'Moisture', 1, 2, NULL, NULL, '%', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (3, TRUE, 60.0, 50.0, 'Copper Content', 2, 1, NULL, NULL, '%', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (4, TRUE, 5.0, 0.0, 'Particle Residue', 2, 2, NULL, NULL, '%', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (5, TRUE, 8.0, 6.0, 'pH', 3, 1, NULL, NULL, 'pH', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (6, TRUE, NULL, NULL, 'Appearance', 3, 2, NULL, 'uniform suspension', NULL, 2);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (7, TRUE, 3.0, 0.0, 'Granule Moisture', 4, 1, NULL, NULL, '%', 1);
INSERT INTO quality_specification_parameters (id, is_required, max_numeric_value, min_numeric_value, name, quality_specification_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (8, TRUE, NULL, NULL, 'Granule Integrity', 4, 2, NULL, 'stable granules', NULL, 2);

INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (1, 0.5, 1, 62.5, 1, 1);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (2, 1.0, 2, 30.0, 2, 1);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (3, 0.5, 3, 7.5, 3, 1);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (4, 0.5, 1, 60.0, 1, 2);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (5, 1.0, 2, 30.0, 2, 2);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (6, 0.5, 3, 8.0, 3, 2);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (7, 0.5, 1, 70.0, 4, 3);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (8, 1.0, 2, 25.0, 2, 3);
INSERT INTO recipe_components (id, allowed_deviation_percent, load_order, percentage, raw_material_id, recipe_version_id)
VALUES (9, 0.5, 3, 5.0, 5, 3);

INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (1, 20, 'Dose approved lots according to the active recipe version.', TRUE, 1, 1, 1, 'Weigh Raw Materials');
INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (2, 45, 'Maintain mixer speed and temperature within approved tolerances.', TRUE, 2, 3, 1, 'Mix Concentrate');
INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (3, 25, 'Transfer finished concentrate to packaging station.', TRUE, 3, 7, 1, 'Fill and Seal');
INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (4, 20, 'Load approved raw materials and verify line readiness.', TRUE, 1, 2, 3, 'Load Extrusion Line');
INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (5, 60, 'Follow extruder program and monitor zone temperatures.', TRUE, 2, 5, 3, 'Extrude Granules');
INSERT INTO technology_steps (id, expected_duration_minutes, instruction, is_required, step_order, step_type, technology_card_id, title)
VALUES (6, 30, 'Stabilize granules before release control.', TRUE, 3, 6, 3, 'Cool Granules');

INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (1, 'Maintain stable mixer speed.', TRUE, 125.0, 115.0, 'Mixer Speed', NULL, 120.0, NULL, 2, 'rpm', 1);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (2, NULL, TRUE, 42.0, 38.0, 'Mixing Temperature', NULL, 40.0, NULL, 2, 'C', 1);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (3, NULL, TRUE, 47.0, 43.0, 'Mixing Duration', NULL, 45.0, NULL, 2, 'min', 1);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (4, NULL, TRUE, NULL, NULL, 'Visual Check', NULL, NULL, 'uniform suspension', 3, NULL, 2);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (5, NULL, TRUE, 118.0, 112.0, 'Zone 1 Temperature', NULL, 115.0, NULL, 5, 'C', 1);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (6, NULL, TRUE, 190.0, 170.0, 'Screw Speed', NULL, 180.0, NULL, 5, 'rpm', 1);
INSERT INTO technology_step_parameters (id, comment, is_required, max_numeric_value, min_numeric_value, name, target_boolean_value, target_numeric_value, target_text_value, technology_step_id, unit, value_type)
VALUES (7, NULL, TRUE, 30.0, 26.0, 'Cooling Temperature', NULL, 28.0, NULL, 6, 'C', 1);

INSERT INTO production_batches (id, batch_number, completed_at, extruder_program_id, planned_quantity, product_id, production_line_id, production_order_id, recipe_version_id, started_at, status, technology_card_id)
VALUES (1, 'BATCH-2026-001', TIMESTAMPTZ '2026-01-20T12:00:00Z', NULL, 500.0, 1, 1, 1, 1, TIMESTAMPTZ '2026-01-20T08:00:00Z', 6, 1);
INSERT INTO production_batches (id, batch_number, completed_at, extruder_program_id, planned_quantity, product_id, production_line_id, production_order_id, recipe_version_id, started_at, status, technology_card_id)
VALUES (2, 'BATCH-2026-002', TIMESTAMPTZ '2026-01-22T14:30:00Z', 1, 300.0, 2, 2, 2, 3, TIMESTAMPTZ '2026-01-22T09:00:00Z', 7, 3);

INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (1, 1, 312.5);
INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (1, 2, 150.0);
INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (1, 3, 37.5);
INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (2, 2, 75.0);
INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (2, 4, 210.0);
INSERT INTO batch_raw_material_consumptions (production_batch_id, raw_material_lot_id, quantity_used)
VALUES (2, 5, 15.0);

INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (1, NULL, TIMESTAMPTZ '2026-01-20T08:20:00Z', 3, 1, TIMESTAMPTZ '2026-01-20T08:00:00Z', 3, 3, 1);
INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (2, NULL, TIMESTAMPTZ '2026-01-20T09:05:00Z', 3, 1, TIMESTAMPTZ '2026-01-20T08:20:00Z', 3, 3, 2);
INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (3, NULL, TIMESTAMPTZ '2026-01-20T12:00:00Z', 3, 1, TIMESTAMPTZ '2026-01-20T09:15:00Z', 3, 3, 3);
INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (4, NULL, TIMESTAMPTZ '2026-01-22T09:18:00Z', 3, 2, TIMESTAMPTZ '2026-01-22T09:00:00Z', 3, 3, 4);
INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (5, 'Temperature deviation detected during extrusion.', TIMESTAMPTZ '2026-01-22T10:25:00Z', 3, 2, TIMESTAMPTZ '2026-01-22T09:20:00Z', 3, 3, 5);
INSERT INTO batch_technology_step_runs (id, comment, completed_at, completed_by_user_id, production_batch_id, started_at, started_by_user_id, status, technology_step_id)
VALUES (6, NULL, TIMESTAMPTZ '2026-01-22T11:05:00Z', 3, 2, TIMESTAMPTZ '2026-01-22T10:30:00Z', 3, 3, 6);

INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (1, NULL, 122.0, NULL, 2, NULL, TRUE, TIMESTAMPTZ '2026-01-20T08:35:00Z', 1);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (2, NULL, 40.5, NULL, 2, NULL, TRUE, TIMESTAMPTZ '2026-01-20T08:40:00Z', 2);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (3, NULL, 45.0, NULL, 2, NULL, TRUE, TIMESTAMPTZ '2026-01-20T09:00:00Z', 3);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (4, NULL, NULL, 'uniform suspension', 3, NULL, TRUE, TIMESTAMPTZ '2026-01-20T11:45:00Z', 4);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (5, NULL, 120.0, NULL, 5, 'Exceeded upper tolerance limit.', FALSE, TIMESTAMPTZ '2026-01-22T09:50:00Z', 5);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (6, NULL, 188.0, NULL, 5, NULL, TRUE, TIMESTAMPTZ '2026-01-22T10:00:00Z', 6);
INSERT INTO batch_step_measured_values (id, actual_boolean_value, actual_numeric_value, actual_text_value, batch_technology_step_run_id, comment, is_within_tolerance, recorded_at, technology_step_parameter_id)
VALUES (7, NULL, 29.0, NULL, 6, NULL, TRUE, TIMESTAMPTZ '2026-01-22T10:45:00Z', 7);

INSERT INTO process_deviations (id, actual_value, batch_technology_step_run_id, created_at, details, parameter_name, planned_value, production_batch_id, reported_by_user_id, severity, title)
VALUES (1, '120 C', 5, TIMESTAMPTZ '2026-01-22T09:55:00Z', 'Deviation registered during extrusion before release control.', 'Zone 1 Temperature', '112-118 C', 2, 3, 3, 'Extruder zone overheating');

INSERT INTO laboratory_tests (id, assigned_at, comment, completed_at, created_at, priority, production_batch_id, quality_specification_id, raw_material_lot_id, result_summary, started_at, status, subject_type, test_kind, test_number, tester_user_id)
VALUES (1, TIMESTAMPTZ '2026-01-06T09:15:00Z', 'Standard incoming control for glyphosate lot.', TIMESTAMPTZ '2026-01-06T10:00:00Z', TIMESTAMPTZ '2026-01-06T09:00:00Z', 1, NULL, 1, 1, 'All parameters within range.', TIMESTAMPTZ '2026-01-06T09:30:00Z', 3, 1, 'Incoming inspection', 'TEST-2026-001', 2);
INSERT INTO laboratory_tests (id, assigned_at, comment, completed_at, created_at, priority, production_batch_id, quality_specification_id, raw_material_lot_id, result_summary, started_at, status, subject_type, test_kind, test_number, tester_user_id)
VALUES (2, TIMESTAMPTZ '2026-01-22T15:05:00Z', 'Release control after extrusion and cooling.', TIMESTAMPTZ '2026-01-22T15:40:00Z', TIMESTAMPTZ '2026-01-22T15:00:00Z', 2, 2, 4, NULL, 'Granule moisture exceeded the release limit.', TIMESTAMPTZ '2026-01-22T15:10:00Z', 3, 2, 'Release control', 'TEST-2026-002', 2);

INSERT INTO laboratory_test_parameter_results (id, actual_boolean_value, actual_numeric_value, actual_text_value, comment, is_required, is_within_range, laboratory_test_id, max_numeric_value, min_numeric_value, parameter_name, quality_specification_parameter_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (1, NULL, 97.5, NULL, NULL, TRUE, TRUE, 1, 100.0, 95.0, 'Purity', 1, 1, NULL, NULL, '%', 1);
INSERT INTO laboratory_test_parameter_results (id, actual_boolean_value, actual_numeric_value, actual_text_value, comment, is_required, is_within_range, laboratory_test_id, max_numeric_value, min_numeric_value, parameter_name, quality_specification_parameter_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (2, NULL, 1.1, NULL, NULL, TRUE, TRUE, 1, 2.0, 0.0, 'Moisture', 2, 2, NULL, NULL, '%', 1);
INSERT INTO laboratory_test_parameter_results (id, actual_boolean_value, actual_numeric_value, actual_text_value, comment, is_required, is_within_range, laboratory_test_id, max_numeric_value, min_numeric_value, parameter_name, quality_specification_parameter_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (3, NULL, 3.8, NULL, NULL, TRUE, FALSE, 2, 3.0, 0.0, 'Granule Moisture', 7, 1, NULL, NULL, '%', 1);
INSERT INTO laboratory_test_parameter_results (id, actual_boolean_value, actual_numeric_value, actual_text_value, comment, is_required, is_within_range, laboratory_test_id, max_numeric_value, min_numeric_value, parameter_name, quality_specification_parameter_id, sort_order, target_boolean_value, target_text_value, unit, value_type)
VALUES (4, NULL, NULL, 'stable granules', NULL, TRUE, TRUE, 2, NULL, NULL, 'Granule Integrity', 8, 2, NULL, 'stable granules', NULL, 2);

INSERT INTO quality_decisions (id, block_reason, comment, decided_at, decided_by_user_id, decision_status, is_current, laboratory_test_id, production_batch_id, raw_material_lot_id, subject_type)
VALUES (1, NULL, 'Approved for production use.', TIMESTAMPTZ '2026-01-06T10:05:00Z', 2, 1, TRUE, 1, NULL, 1, 1);
INSERT INTO quality_decisions (id, block_reason, comment, decided_at, decided_by_user_id, decision_status, is_current, laboratory_test_id, production_batch_id, raw_material_lot_id, subject_type)
VALUES (2, 'Granule moisture exceeded specification.', 'Release blocked.', TIMESTAMPTZ '2026-01-22T15:45:00Z', 2, 2, TRUE, 2, 2, NULL, 2);

SELECT setval(pg_get_serial_sequence('departments', 'id'), COALESCE((SELECT MAX(id) FROM departments), 1), true);
SELECT setval(pg_get_serial_sequence('material_categories', 'id'), COALESCE((SELECT MAX(id) FROM material_categories), 1), true);
SELECT setval(pg_get_serial_sequence('product_forms', 'id'), COALESCE((SELECT MAX(id) FROM product_forms), 1), true);
SELECT setval(pg_get_serial_sequence('product_types', 'id'), COALESCE((SELECT MAX(id) FROM product_types), 1), true);
SELECT setval(pg_get_serial_sequence('production_lines', 'id'), COALESCE((SELECT MAX(id) FROM production_lines), 1), true);
SELECT setval(pg_get_serial_sequence('suppliers', 'id'), COALESCE((SELECT MAX(id) FROM suppliers), 1), true);
SELECT setval(pg_get_serial_sequence('user_roles', 'id'), COALESCE((SELECT MAX(id) FROM user_roles), 1), true);
SELECT setval(pg_get_serial_sequence('app_users', 'id'), COALESCE((SELECT MAX(id) FROM app_users), 1), true);
SELECT setval(pg_get_serial_sequence('equipment', 'id'), COALESCE((SELECT MAX(id) FROM equipment), 1), true);
SELECT setval(pg_get_serial_sequence('products', 'id'), COALESCE((SELECT MAX(id) FROM products), 1), true);
SELECT setval(pg_get_serial_sequence('raw_materials', 'id'), COALESCE((SELECT MAX(id) FROM raw_materials), 1), true);
SELECT setval(pg_get_serial_sequence('production_orders', 'id'), COALESCE((SELECT MAX(id) FROM production_orders), 1), true);
SELECT setval(pg_get_serial_sequence('quality_specifications', 'id'), COALESCE((SELECT MAX(id) FROM quality_specifications), 1), true);
SELECT setval(pg_get_serial_sequence('raw_material_lots', 'id'), COALESCE((SELECT MAX(id) FROM raw_material_lots), 1), true);
SELECT setval(pg_get_serial_sequence('recipe_versions', 'id'), COALESCE((SELECT MAX(id) FROM recipe_versions), 1), true);
SELECT setval(pg_get_serial_sequence('technology_cards', 'id'), COALESCE((SELECT MAX(id) FROM technology_cards), 1), true);
SELECT setval(pg_get_serial_sequence('extruder_programs', 'id'), COALESCE((SELECT MAX(id) FROM extruder_programs), 1), true);
SELECT setval(pg_get_serial_sequence('production_batches', 'id'), COALESCE((SELECT MAX(id) FROM production_batches), 1), true);
SELECT setval(pg_get_serial_sequence('quality_specification_parameters', 'id'), COALESCE((SELECT MAX(id) FROM quality_specification_parameters), 1), true);
SELECT setval(pg_get_serial_sequence('recipe_components', 'id'), COALESCE((SELECT MAX(id) FROM recipe_components), 1), true);
SELECT setval(pg_get_serial_sequence('technology_steps', 'id'), COALESCE((SELECT MAX(id) FROM technology_steps), 1), true);
SELECT setval(pg_get_serial_sequence('batch_technology_step_runs', 'id'), COALESCE((SELECT MAX(id) FROM batch_technology_step_runs), 1), true);
SELECT setval(pg_get_serial_sequence('laboratory_tests', 'id'), COALESCE((SELECT MAX(id) FROM laboratory_tests), 1), true);
SELECT setval(pg_get_serial_sequence('laboratory_test_parameter_results', 'id'), COALESCE((SELECT MAX(id) FROM laboratory_test_parameter_results), 1), true);
SELECT setval(pg_get_serial_sequence('quality_decisions', 'id'), COALESCE((SELECT MAX(id) FROM quality_decisions), 1), true);
SELECT setval(pg_get_serial_sequence('technology_step_parameters', 'id'), COALESCE((SELECT MAX(id) FROM technology_step_parameters), 1), true);
SELECT setval(pg_get_serial_sequence('batch_step_measured_values', 'id'), COALESCE((SELECT MAX(id) FROM batch_step_measured_values), 1), true);
SELECT setval(pg_get_serial_sequence('process_deviations', 'id'), COALESCE((SELECT MAX(id) FROM process_deviations), 1), true);
