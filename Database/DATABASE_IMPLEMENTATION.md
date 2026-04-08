# Database Implementation

## Scope

Implemented database layer covers:
- identity and access data (`departments`, `user_roles`, `app_users`);
- catalogs (`products`, `raw_materials`, `suppliers`, `production_lines`, `equipment`);
- recipes and technology cards with versioning;
- production orders, batches, consumptions, step runs, measurements, telemetry and deviations;
- laboratory specifications, tests, parameter results and quality decisions;
- notifications, audit log and status history.

## Enforced Rules

At database level the schema now enforces:
- only one active approved recipe per product;
- only one active approved technology card per product;
- batch consistency with product, recipe version, technology card, production order and extruder program through composite foreign keys;
- quantity, percentage, range and timeline checks;
- only approved and active normative versions can be used to create a production batch;
- technology step runs cannot reference steps from another technology card;
- measured step values cannot reference parameters from another technology step;
- process deviations cannot reference a step run from another production batch;
- only approved raw material lots can be consumed and total consumption cannot exceed received quantity;
- telemetry entries cannot reference equipment from another production line;
- subject integrity for laboratory tests, quality specifications and quality decisions;
- laboratory tests must use a specification that matches the tested raw material or batch product;
- laboratory test parameter results must belong to the specification of the parent test;
- quality decisions must reference a laboratory test for the same controlled object;
- uniqueness of current quality decision for a lot or production batch;
- recipe approval blocked when total component percentage is not exactly `100%`;
- direct edits of components in an already approved recipe are also blocked if they break the `100%` total.

## Criteria Coverage

The schema is designed to directly support the database scoring block from the competition criteria:
- full minimum set of entities for products, raw materials, recipes, technology cards, batches, laboratory control, notifications, audit and status history;
- explicit identifiers in every table;
- typed numeric/date/boolean fields with check constraints where business rules require them;
- 3NF-oriented structure with separation of normative and operational data;
- meaningful seed data for at least two end-to-end scenarios;
- all three mandatory integrity constraints from the task implemented at SQL level.

## Seed Data

Initial seed data contains:
- departments, roles and four users;
- two products and five raw materials;
- approved and draft recipe versions;
- approved and draft technology cards;
- two production orders and two production batches;
- laboratory specifications, tests and decisions;
- telemetry, deviation, audit and notification examples.

## Commands

Create SQL script:

```powershell
dotnet ef migrations script `
  --project .\src\WSR2026.PlantProduction.Infrastructure\WSR2026.PlantProduction.Infrastructure.csproj `
  --startup-project .\src\WSR2026.PlantProduction.Api\WSR2026.PlantProduction.Api.csproj `
  --context PlantProductionDbContext `
  --idempotent `
  -o .\docs\sql\initial-idempotent.sql
```

Apply migration to a configured PostgreSQL instance:

```powershell
dotnet ef database update `
  --project .\src\WSR2026.PlantProduction.Infrastructure\WSR2026.PlantProduction.Infrastructure.csproj `
  --startup-project .\src\WSR2026.PlantProduction.Api\WSR2026.PlantProduction.Api.csproj `
  --context PlantProductionDbContext
```
