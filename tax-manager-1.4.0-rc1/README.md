# TaxManager

## :mag: Overview

Information about the service like what it does, how it works, what are the goals, etc.

## :link: Useful links and resources

Add links to useful resources like documentation, diagrams, etc.

* [Atlas platform documentation](https://mews.atlassian.net/wiki/spaces/AP/overview?homepageId=35357768)

## :file_folder: Proposed solution structure 

```
docs/
infra/
src/
  buildingBlocks/
   TaxManager.BuildingBlocks.Application/
   TaxManager.BuildingBlocks.Domain/
   TaxManager.BuildingBlocks.Infrastructure/
  hosts/
   TaxManager/
      TaxManager.csproj
   TaxManager.Aspire.AppHost/
      TaxManager.Aspire.AppHost.csproj
   TaxManager.Aspire.ServiceDefaults/
  modules/
   core/
    TaxManager.Core.Application/
    TaxManager.Core.Domain/
    TaxManager.Core.EntityFrameworkCore/
tests/
  TaxManager.IntegrationTests/
  TaxManager.UnitTests/
tools/
  TaxManager.DbMigrator/
TaxManager.sln
README.md
azure-pipelines.yml
Dockerfile
```

## :electric_plug: Endpoints :
* /swagger/index.html   - [GET] -  Swagger UI - Open API Documentation
* /api/system/info - [GET] - Will return info about service
* /health - [GET] - Will return service health check status

Add more endpoints here...

## :construction_worker: Infrastructure

### Dependencies

* [Mews.Infrastructure.Sdk](https://mews.atlassian.net/wiki/spaces/AP/pages/40337422/SDK)
  
## :building_construction: Developer Experience 

### Prerequisites

#### Rancher Desktop

To be able to run containerized tools locally, you need to have Rancher Desktop installed and running.

For more information, check out the [Confluence page](https://mews.atlassian.net/wiki/x/VACiAQ).

### Running the app locally

We use [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) 
for the local developer experience.

__Important:__ Make sure your dotnet workloads are restored. Please use the following command:
```
dotnet workload restore -s https://api.nuget.org/v3/index.json 
```

The .NET Aspire orchestrator features two launch profiles: one for `HTTP` and one for `HTTPS`.

Please use one of the launch profiles with your IDE to run your Atlas service with Aspire.

When launching a .NET Aspire app host, the Aspire orchestrator initiates all necessary app resources 
and launches a browser window to access the [Aspire dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview?tabs=bash).

__Notes:__

__1) The Aspire dashboard uses token-based authentication to safeguard sensitive data like environment variables.__ 

__When launching the dashboard from Visual Studio or Visual Studio Code with the C# Dev Kit extension, 
automatic login occurs, opening the dashboard instantly.__

__Starting the app host from Rider or the command line prompts a login page, 
displaying a clickable URL in the console window to access the dashboard.__

__2) The `HTTPS` launch profile requires a developer certificate.__

__To generate a developer certificate run 'dotnet dev-certs https'.__

__To trust the certificate (Windows and macOS only) run 'dotnet dev-certs https --trust'.__

### Build and Validate Docker Image used for deploying

To validate the Docker image for deploying the .NET application, run from the root of your repository:
```
./build.sh validate

```

### Integration Tests

Based on https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0

_Elements of the support:_

- Custom WebApplicationFactory
- Sample Integration Test: HealthCheckControllerTests
