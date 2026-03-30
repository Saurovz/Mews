# Mews.Job.Scheduler

## :mag: Overview

The Job Scheduler provide APIs for centralized management of schedules for asychronous and potentially long-running workloads, as well as the logic to signal the worker nodes to trigger the processing hosted there.

## :link: Useful links and resources

[Job Framework redesign](https://mews.atlassian.net/wiki/spaces/UX/pages/49152879/Job+Framework+redesign)



## :building_construction: Developer Experience 
[Developer Experience](https://app.getguru.com/card/TMxgznBc/Developer-experience)

### Running the app locally

### .NET Aspire 
We introduced [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
for the local developer experience.

__Important:__ Make sure your dotnet workloads are restored. Please use the following command:
```
dotnet workload restore
```
The .NET Aspire orchestrator features different launch profiles: `HTTP` and one for `HTTPS`.

To Debug .NET Aspire apps with JetBrains Rider, please install the plugin for .NET Aspire. You can read more on [this document](https://mews.atlassian.net/wiki/spaces/AP/pages/351404133).

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

#### Integration Tests

Based on https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0

_Elements of the support:_

- Custom WebApplicationFactory
- Sample Integration Test: HealthCheckControllerTests
