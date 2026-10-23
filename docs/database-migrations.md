# Database Migration Deployment

The web application does not apply EF Core migrations or provision Identity users/roles at runtime. Apply schema changes as a controlled deployment step before rolling out the application version that depends on them. Static Identity roles and profile lookup data are managed by EF migrations. `ApplicationDbContextFactory` provides EF tooling a model without opening a database connection or executing application startup logic.

## Generate the migration script

From the repository root, generate an idempotent PostgreSQL script:

```bash
dotnet ef migrations script --idempotent --project MeetCampus/MeetCampus.csproj --startup-project MeetCampus/MeetCampus.csproj --output artifacts/MeetCampus.Migrations.sql
```

The EF CLI is a tool prerequisite. Keep its version aligned with the EF runtime packages; this workspace currently has EF tooling 10.0.6 and runtime packages 10.0.13. On Linux, update/install the matching tool with `dotnet tool update --global dotnet-ef --version 10.0.13` and ensure `$HOME/.dotnet/tools` is on `PATH`. Script generation was verified without a database connection. Review generated SQL as a code artifact and retain it with the deployment release. Do not put database credentials in the script or source control.

## Deployment sequence

1. Generate the script from the exact application commit being deployed.
2. Review the SQL and test it against an isolated PostgreSQL database containing a representative prior schema and data.
3. Take/verify a recoverable database backup before production schema changes.
4. Apply the reviewed script using the deployment identity and credentials from the deployment secret store. Configure libpq using a secured PostgreSQL service file or `.pgpass` file (mode `0600`), then run:

   ```bash
   psql --set ON_ERROR_STOP=on --file artifacts/MeetCampus.Migrations.sql
   ```

   Do not put passwords in command history, command-line arguments, or logs.
5. Verify the migration history table contains the expected migration IDs and perform the deployment's database health checks.
6. Deploy/start the application only after schema application succeeds. Application startup assumes the schema and migration-managed static data are already current.

The script is idempotent for repeated application against the same migration history. Idempotence does not make arbitrary changes reversible. For a failed or incompatible production migration, use the approved database restore procedure or a reviewed forward-fix migration; do not assume that rolling back the application automatically rolls back the database.

## Bootstrap the initial administrator

Provision the initial administrator as a separate, explicitly invoked deployment step after applying migrations. Publish the command with the application release:

```bash
dotnet publish MeetCampus.AdminBootstrap/MeetCampus.AdminBootstrap.csproj --configuration Release --output artifacts/admin-bootstrap
```

Inject these configuration values from the deployment secret store into the one-shot command environment; do not put credentials in application settings, source control, or command-line arguments:

- `ConnectionStrings__DefaultConnection`
- `BootstrapAdmin__Email`
- `BootstrapAdmin__Password`

Run the published command:

```bash
dotnet artifacts/admin-bootstrap/MeetCampus.AdminBootstrap.dll
```

The command verifies there are no pending migrations and requires the `Admin` role created by migrations. It creates a new confirmed administrator using `UserManager` and assigns the migration-managed role using `RoleManager`. A rerun for an existing administrator is a no-op; if the configured email belongs to a non-administrator, the command fails without elevating that account or changing its password. Normal web startup does not create or promote administrators.

## Automated migration tests

`MeetCampus.IntegrationTests` uses Testcontainers to start a disposable PostgreSQL container and applies migrations to its empty database. It never uses the application's remote connection string. Run it with `dotnet test MeetCampus.IntegrationTests/MeetCampus.IntegrationTests.csproj` on a machine or Linux CI runner with a working Docker-compatible container engine. No local PostgreSQL installation or remote test database configuration is required.
