using System.Net.Http.Json;
using Aspire.Hosting.Testing;
using Mews.Job.Scheduler.Aspire.AppHost;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Configuration;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;

internal static class HttpClientHelpers
{
    internal static async Task<T> AuthorizedHttpClientActionAsync<T>(
        Func<HttpClient, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var httpClient = IntegrationTests.AspireDistributedApp.CreateHttpClient(AspireAppHostConfiguration.ProjectResourceName);
        httpClient.DefaultRequestHeaders.Add(
            "x-mews-job-scheduler-access-token",
            TestConfiguration.LocalAuthorizationHeaderValue
        );
        return await action(httpClient, cancellationToken);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string requestUri, JsonContent payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Content = payload;

        return await client.SendAsync(request, cancellationToken);
    }
}
