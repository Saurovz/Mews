using System.Net.Http.Json;
using System.Text.Json;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;
using Mews.Job.Scheduler.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;

public static class SerializationHelpers
{
    private static JsonSerializerOptions? _cachedSerializerOptions;

    public static JsonContent Serialize<TDto>(TDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var options = GetSerializerOptions();
        return JsonContent.Create(dto, options: options);
    }

    public static async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new InvalidOperationException("Response content is empty.");
        }

        try
        {
            var options = GetSerializerOptions();
            var responseContent = JsonSerializer.Deserialize<T>(responseString, options);
            return responseContent!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to deserialize response content.", ex);
        }
    }

    private static JsonSerializerOptions GetSerializerOptions()
    {
        if (_cachedSerializerOptions == null)
        {
            var services = new ServiceCollection();
            services.AddJsonConfiguration();
            var serviceProvider = services.BuildServiceProvider();
            var jsonOptions = serviceProvider.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>().Value;
            JsonSerializerOptionsConfiguration.Configure(jsonOptions.SerializerOptions);
            _cachedSerializerOptions = jsonOptions.SerializerOptions;
        }

        return _cachedSerializerOptions;
    }
}
