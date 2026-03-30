using System.Buffers;
using System.Dynamic;
using System.Text;
using Mews.Job.Scheduler.Common;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Mews.Job.Scheduler.Extensions;

public static class HttpContextExtensions
{
    public static async Task<object> GetApiExceptionDetails(this HttpContext httpContext, int httpCode, CancellationToken cancellationToken)
    {
        var details = (dynamic)new ExpandoObject();
        try
        {
            // We want to sort retrieval of these request properties by their importance.
            // If one of the property getters throw en exception we have more important information already in details object.
            details.Url = httpContext.Request.Path;
            details.Method = httpContext.Request.Method;
            details.ErrorCode = httpCode;
            details.UserHostAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            details.Params = ParamDictionary(httpContext.Request);
            if (httpContext.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent))
            {
                details.UserAgent = userAgent;
            }
            if (httpContext.Request.Headers.TryGetValue(HttpConstants.JobSchedulerCorrelationIdHeaderName, out var correlationId))
            {
                details.CorrelationId = correlationId;
            }

            if (httpContext.Request.Body.CanSeek)
            {
                var bodyData = await ReadFullyAsync(httpContext.Request.Body, seekToBeginning: true, cancellationToken);
                var body = Encoding.UTF8.GetString(bodyData);
                details.Body = body;
            }
        }
        catch (Exception ex)
        {
            details.ErrorHandlerException = ex;
        }

        return details;
    }

    private static Dictionary<string, object?> ParamDictionary(HttpRequest r)
    {
        var formDictionary = new Dictionary<string, object?>();

        CopyNameValueCollection(r.Query, formDictionary);
        if (r is { HasFormContentType: true, Body.CanSeek: true })
        {
            r.Body.Seek(0, SeekOrigin.Begin);
            CopyNameValueCollection(r.Form, formDictionary);
        }

        return formDictionary;
    }

    private static void CopyNameValueCollection(IEnumerable<KeyValuePair<string, StringValues>> collection, IDictionary<string, object?> formDictionary)
    {
        foreach (var (key, values) in collection)
        {
            formDictionary[key] = values.Count > 1 ? values.ToList() : values[0];
        }
    }

    private static async Task<byte[]> ReadFullyAsync(Stream stream, bool seekToBeginning, CancellationToken cancellationToken)
    {
        if (seekToBeginning)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var arrayPool = ArrayPool<byte>.Shared;
        var buffer = arrayPool.Rent(32768);

        try
        {
            using var ms = new MemoryStream();
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (read <= 0)
                {
                    return ms.ToArray();
                }
                ms.Write(buffer, 0, read);
            }
        }
        finally
        {
            arrayPool.Return(buffer, clearArray: true);
        }
    }
}
