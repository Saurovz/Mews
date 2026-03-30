    using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace TaxManager.Features.Dummy;

public class DummyCreatedEvent
{
    public string Value { get; set; } = string.Empty;
}

public static class ActivityExtensions
{
    // We should store the event name as a constant, but we can source it from other projects
    // if we wanted to share the event name across multiple projects for creating
    // dashboards in IaC for example
    private const string DummyCreatedEventName = "dummy.created";
    
    // We don't need to store the attribute name unless we wanted to use it in multiple places
    // for a use case like the above
    private const string AttributeDummyCreatedEventValue  = "example.value";

    public static Activity? AddEventDummyCreated(this Activity? source, DummyCreatedEvent dummyCreatedEvent)
    {
        // When adding events, consider the semantic conventions
        // https://opentelemetry.io/docs/specs/semconv/general/events/
        source?.AddEvent(new ActivityEvent(
            DummyCreatedEventName, // This is the event.name attribute, make sure to follow attribute semantics
            tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
            {
                new(AttributeDummyCreatedEventValue, dummyCreatedEvent.Value),
            })
        ));

        return source;
    }
}

/// <summary>
/// DummyController is a demonstration controller.
/// </summary>
/// <remarks>
/// This controller provides example actions for demonstration purposes.
/// It's intended to showcase routing and Swagger documentation.
/// Remember to remove or replace it with actual implementation in a production application.
/// </remarks>
[ApiExplorerSettings(IgnoreApi = true)] //will hide from swagger UI
[ApiController]
[Route("[controller]")]
public class DummyController : ControllerBase
{
    private readonly ILogger<DummyController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly Instrumentation _instrumentation;


    // The ActivitySource name should be unique to the project, it will appear
    // as the name of the instrumenting library when viewing spans. It allows us
    // to easily differentiate between different libraries that may be instrumented
    // in the same service.
    internal const string ActivitySourceName = "TaxManager.Dummy";
    private readonly ActivitySource _dummySource = new(ActivitySourceName, Assembly.GetExecutingAssembly().GetName().Version?.ToString());
    
    private const string AttributeDummyHello  = "dummy.hello";
    private const string AttributeDummyWorld  = "dummy.world";

    
    public DummyController(
        ILogger<DummyController> logger,
        IWebHostEnvironment environment,
        IServiceProvider serviceProvider,
        Instrumentation instrumentation)
    {
        _environment = environment;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;
    }
    
    // GET: /Dummy
    [HttpGet]
    [OpenApiOperation("Demo GET endpoint",
        "This is a dummy GET endpoint for demonstration purposes. Should be deleted in production.")]
    public IActionResult Get()
    {
        // Example use of a semantic attribute
        // We should opt to only ever use a constant for the attribute key
        // to avoid typos and inconsistencies.
        // If one doesn't exist but others may benefit, consider adding it to a shared constants file.
        
        // Here we can attach to the root span, created automatically by the ASP.NET Core instrumentation
        // This is useful for attaching attributes that are relevant to the whole request
        // or if we don't want to or need to create a new span
        Activity.Current?.SetTag(AttributeDummyHello, "world");
        
        // ReSharper disable once ExplicitCallerInfoArgument
        using var activity = _dummySource.StartActivity("hello_world_span"); // Explicitly set a span name
        
        // Use the span we've created and attach an attribute to it
        activity?.SetTag(AttributeDummyWorld, "hello");
        
        // Sleep for a second and a half for an interesting trace
        Thread.Sleep(1500);
        
        return Ok("Dummy GET response saying Hello");
    }
    
    // GET: /Dummy/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    [OpenApiOperation("Demo GET endpoint with ID",
        "This is a dummy GET endpoint with [Authorize] attribute for demonstration purposes. Should be deleted in production.")]
    public IActionResult Get(int id)
    {
        return Ok($"Dummy GET response for ID {id} - for demonstration only.");
    }

    // POST: /Dummy
    [HttpPost]
    [OpenApiOperation("Demo POST endpoint",
        "This is a dummy POST endpoint for demonstration purposes. Should be deleted in production.")]
    public IActionResult Post([FromBody] JsonElement  value)
    {
        using var activity = _dummySource.StartActivity(); // If a name isn't provided, it uses the current function name as the span name

        // Let's pretend we're inserting a resource into a database.
        
        // We can log the value of the resource for debugging purposes.
        _logger.LogInformation("Created a dummy resource with value: {Value}", value);
        
        // But that's an antipattern in otel, we should opt for events instead
        // as they are more flexible and can be used for more than just
        // showing something happened, we can include attributes and easily
        // visualise them on a trace timeline, as well as specifically search for them
        activity?.AddEventDummyCreated(new DummyCreatedEvent { Value = "hello world"});

        return Ok($"Dummy POST response with value: {value} - for demonstration only.");
    }

    // PUT: /Dummy/{id}
    [HttpPut("{id:int}")]
    [OpenApiOperation("Demo PUT endpoint",
        "This is a dummy PUT endpoint for demonstration purposes. Should be deleted in production.")]
    public IActionResult Put(int id, [FromBody] JsonElement value)
    {
        // Pass in the id to the instrumentation method
        // this will increment the counter for the specific id dimension
        // which can cause high cardinality if we don't use a
        // reasonable number of dimensions and quantity of values
        _instrumentation.IncreaseDummyPutCounter(id);
        
        return Ok($"Dummy PUT response for ID {id} with value: {value} - for demonstration only.");
    }

    // DELETE: /Dummy/{id}
    [HttpDelete("{id:int}")]
    [OpenApiOperation("Demo DELETE endpoint",
        "This is a dummy DELETE endpoint for demonstration purposes. Should be deleted in production.")]
    public IActionResult Delete(int id)
    {
        // We can increment a counter to track how many times the dummy is deleted
        _instrumentation.DummyDeleteCounter.Add(1);
        return Ok($"Dummy DELETE response for ID {id} - for demonstration only.");
    }
}

