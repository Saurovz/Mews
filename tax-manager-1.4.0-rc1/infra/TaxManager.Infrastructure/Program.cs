using TaxManager.Infrastructure;
using Pulumi;

await Deployment.RunAsync(Infrastructure.Create!);
