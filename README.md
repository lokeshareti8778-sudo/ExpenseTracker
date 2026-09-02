# ExpenseTracker.API

ASP.NET Core 8 Web API for managing expenses. The application uses Entity Framework Core with SQL Server, Swagger, dependency injection, repository and service layers, structured logging, and global exception handling.

## Local setup

Prerequisites:

- .NET 8 SDK
- SQL Server or Docker Desktop
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

Swagger is available at `http://localhost:5000/swagger` or the URL printed by the application. Set `ConnectionStrings:DefaultConnection` in `ExpenseTracker.API/appsettings.json`, or use the `ConnectionStrings__DefaultConnection` environment variable.

## Docker

Create a `.env` file in the repository root. Use a strong password containing at least eight characters, including upper case, lower case, and a number:

```dotenv
MSSQL_SA_PASSWORD=ReplaceWithAStrongPassword1
```

Start the API and SQL Server:

```bash
docker compose up -d --build
```

The API is available at `http://localhost:8080/swagger`. Stop the stack with `docker compose down`. Add `-v` only when the SQL Server data volume should be deleted.

For a real deployment, create and apply EF Core migrations before using new database schema changes:

```bash
dotnet ef migrations add InitialCreate --project ExpenseTracker.API
dotnet ef database update --project ExpenseTracker.API
```

## SonarQube

`sonar-project.properties` defines the project key, sources, tests, exclusions, and OpenCover report path. The CI workflow uses the .NET SonarScanner and sends coverage from Coverlet.

Install a standalone SonarQube instance on Ubuntu with Docker:

```bash
sudo bash scripts/install-sonarqube.sh
```

The script starts SonarQube and PostgreSQL at port `9000`. Open `http://VM_IP:9000`, sign in with the initial `admin` / `admin` credentials, change the password, create a project and token, and use the project key in GitHub secrets. Replace the example PostgreSQL password in the script before production use.

## GitHub Secrets

Configure these repository secrets:

- `SONAR_HOST_URL`: SonarQube URL, for example `http://sonarqube.example.com:9000`
- `SONAR_TOKEN`: SonarQube project analysis token
- `SONAR_PROJECT_KEY`: SonarQube project key, for example `expense-tracker-api`
- `AZURE_VM_HOST`: Public DNS name or IP address of the Azure Ubuntu VM
- `AZURE_VM_USER`: SSH user on the VM
- `AZURE_VM_SSH_KEY`: Private SSH key matching the VM user's authorized key

The GitHub-provided `GITHUB_TOKEN` is used to download the artifact produced by the triggering CI run.

## Azure Ubuntu VM setup

1. Create an Ubuntu 22.04 or newer Azure VM and allow inbound TCP `22` and `8080` in its network security group. Restrict port `8080` to trusted networks or place the VM behind a reverse proxy for production.
2. Install Docker and the Compose plugin, then add the deployment user to the Docker group.
3. Copy the repository's `scripts/install-sonarqube.sh` only to a SonarQube host if SonarQube will run on the VM.
4. Prepare the application service:

```bash
sudo mkdir -p /opt/expense-tracker
sudo tee /etc/systemd/system/expense-tracker.service >/dev/null <<'SERVICE'
[Unit]
Description=ExpenseTracker API Docker Compose application
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/expense-tracker/current
ExecStart=/usr/bin/docker compose -f /opt/expense-tracker/current/docker-compose.yml up -d
ExecStop=/usr/bin/docker compose -f /opt/expense-tracker/current/docker-compose.yml down

[Install]
WantedBy=multi-user.target
SERVICE
sudo systemctl daemon-reload
sudo systemctl enable expense-tracker.service
```

4. Ensure the VM has `/opt/expense-tracker/current` writable by the deployment process and that Docker can bind ports `8080` and `1433`.

The CD workflow creates a versioned release directory, updates the `current` symlink, runs `scripts/deploy.sh`, restarts `expense-tracker.service`, and verifies Swagger over HTTP.

## CI/CD execution

- CI runs on pushes and pull requests targeting `main`.
- CI restores packages, builds, runs xUnit tests with Coverlet OpenCover coverage, runs SonarQube analysis, and publishes coverage and release artifacts.
- CD listens for a successful CI workflow on `main`, downloads the matching release artifact, transfers it over SSH, restarts the Azure VM service, and verifies the deployed API.

Pull requests run CI only. Deployment occurs after a successful push to `main`.
## CI Retry

Updated SonarQube configuration and GitHub Actions workflow.