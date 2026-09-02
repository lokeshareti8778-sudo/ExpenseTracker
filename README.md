# ExpenseTracker.API

ASP.NET Core 8 Web API for managing expenses with Entity Framework Core, SQL Server, Swagger, dependency injection, repository and service layers, structured logging, and global exception handling.

## Local setup

Prerequisites:

- .NET 8 SDK
- SQL Server
- Git

Restore, build, and test:

```bash
dotnet restore ExpenseTracker.sln
dotnet build ExpenseTracker.sln
dotnet test ExpenseTracker.sln
```

Run the API:

```bash
dotnet run --project ExpenseTracker.API
```

Swagger is available at the URL printed by the application. Set `ConnectionStrings:DefaultConnection` in `ExpenseTracker.API/appsettings.json`, or use the `ConnectionStrings__DefaultConnection` environment variable.

## Traditional deployment

The application is deployed as a normal framework-dependent .NET application. Install the .NET 8 ASP.NET Core runtime and SQL Server on the target machine. Apply EF Core migrations before using new database schema changes:

```bash
dotnet ef migrations add InitialCreate --project ExpenseTracker.API
dotnet ef database update --project ExpenseTracker.API
```

Publish the API with `dotnet publish`. The generated `ExpenseTracker.API.dll` can be run with `dotnet`; the production service listens on port `8080` on all network interfaces. The expense dashboard is available at `http://YOUR_VM_PUBLIC_IP:8080/`, and Swagger is available at `http://YOUR_VM_PUBLIC_IP:8080/swagger` after allowing TCP port `8080` in Azure and the VM firewall.

## SonarQube

`sonar-project.properties` defines the project key, sources, tests, exclusions, and OpenCover report path. CI uses the .NET SonarScanner and Coverlet coverage. Use an externally managed SonarQube instance configured with the secrets below.

## GitHub Secrets

Configure these repository secrets:

- `SONAR_HOST_URL`: SonarQube URL
- `SONAR_TOKEN`: SonarQube project analysis token
- `SONAR_PROJECT_KEY`: SonarQube project key
- `AZURE_VM_HOST`: Public DNS name or IP address of the Azure Ubuntu VM
- `AZURE_VM_USER`: SSH user on the VM
- `AZURE_VM_SSH_KEY`: Private SSH key matching the VM user's authorized key

## Azure Ubuntu VM setup

1. Create an Ubuntu 22.04 or newer Azure VM and allow inbound TCP `22` and `8080`. Restrict port `8080` to trusted networks or use a reverse proxy in production.
2. Install SQL Server and create the `expense-tracker` service account. The
	deployment script installs the ASP.NET Core 8 runtime automatically on Ubuntu
	22.04 or 24.04 when it is missing.
3. Create the production connection string file:

```bash
sudo install -d -m 0750 /etc/expense-tracker
sudo sh -c 'printf "%s\n" "ConnectionStrings__DefaultConnection=Server=localhost;Database=ExpenseTrackerDb;User Id=...;Password=...;TrustServerCertificate=True" > /etc/expense-tracker/expense-tracker.env'
sudo chown root:expense-tracker /etc/expense-tracker/expense-tracker.env
sudo chmod 0640 /etc/expense-tracker/expense-tracker.env
```

4. Ensure the deployment user can run `sudo` for `deploy.sh`. The script detects
	the installed .NET runtime, installs the systemd unit, stores releases under
	`/opt/expense-tracker/releases`, updates `/opt/expense-tracker/current`, and
	verifies Swagger over HTTP.

The CD workflow transfers a publish artifact containing `app/`, `deploy.sh`, and `expense-tracker.service`, then restarts the systemd service over SSH. A manual release can be deployed with:

```bash
sudo ./deploy.sh
```

## CI/CD execution

- CI runs on pushes and pull requests targeting `main`.
- CI restores packages, builds, tests, runs SonarQube analysis, and publishes a framework-dependent release artifact.
- CD deploys successful `main` builds to the Azure VM and verifies Swagger.

Pull requests run CI only. Deployment occurs after a successful push to `main`.
