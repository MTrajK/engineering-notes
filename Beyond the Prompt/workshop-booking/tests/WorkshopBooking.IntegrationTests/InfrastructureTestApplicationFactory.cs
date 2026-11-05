using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkshopBooking.Api.Infrastructure;
using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class InfrastructureTestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly BookingDatabase _database = new();
    public ControlledTimeProvider Clock { get; } = new();
    public bool SeedDemoData
    {
        get; init;
    }
    public WorkshopBooking.Application.Persistence.IMutationCoordinator? Coordinator
    {
        get; init;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    public async Task<T> WithDatabaseAsync<T>(Func<WorkshopBookingContext, Task<T>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        WorkshopBookingContext context = scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>();
        return await scope.ServiceProvider.GetRequiredService<BookingDatabase>().ExecuteAsync(() => action(context), TestContext.Current.CancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DatabaseInitializationOptions>();
            services.AddSingleton(new DatabaseInitializationOptions(SeedDemoData));
            if (Coordinator is not null)
            {
                services.RemoveAll<WorkshopBooking.Application.Persistence.IMutationCoordinator>();
                services.AddSingleton(Coordinator);
            }
            services.RemoveAll<BookingDatabase>();
            services.AddSingleton(_database);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services
                .AddControllers()
                .AddApplicationPart(typeof(InfrastructureTestController).Assembly);
        });
    }
}

[ApiController]
[Route("api/__infrastructure-test")]
public sealed class InfrastructureTestController : ControllerBase
{
    private const string SecretFailureText = "private exception detail must not be returned";

    [HttpGet("outcomes/{kind}")]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public IActionResult GetOutcome(string kind)
    {
        Outcome<string> outcome = kind switch
        {
            "success" => Outcome.Success("completed"),
            "validation" => Outcome.Failed<string>(ApplicationFailure.Validation(
                new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    ["participantId"] = ["A participant ID is required."]
                })),
            "missing" => Outcome.Failed<string>(ApplicationFailure.NotFound("workshop_not_found")),
            "conflict" => Outcome.Failed<string>(ApplicationFailure.Conflict("workshop_full")),
            _ => throw new InvalidOperationException("Unknown test outcome.")
        };

        return OutcomeHttpMapper.Map(this, outcome, value => Ok(new { value }));
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] RequiredTextRequest request) => Ok(new { request.Name });

    [HttpPost("convert")]
    public IActionResult Convert([FromBody] IntegerRequest request) => Ok(new { request.Count });

    [HttpGet("query")]
    public IActionResult Query([FromQuery] QueryRequest request) => Ok(new { request.WorkshopId });

    [HttpPost("timestamp")]
    public IActionResult Timestamp([FromBody] TimestampRequest request) =>
        Ok(new TimestampResponse(request.Instant, null, TestStatus.Ready));

    [HttpGet("read-only")]
    public IActionResult ReadOnly() => Ok();

    [HttpPost("media")]
    [Consumes("application/json")]
    public IActionResult Media([FromBody] RequiredTextRequest request) => Ok(new { request.Name });

    [HttpGet("throw")]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Throw() =>
        throw new InvalidOperationException($"{SecretFailureText} at {Request.Path}");
}

public sealed class RequiredTextRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string? Name
    {
        get; init;
    }
}

public sealed class IntegerRequest
{
    public int Count
    {
        get; init;
    }
}

public sealed class QueryRequest
{
    public Guid WorkshopId
    {
        get; init;
    }
}

public sealed class TimestampRequest
{
    public DateTimeOffset Instant
    {
        get; init;
    }
}

public sealed record TimestampResponse(DateTimeOffset Instant, string? Optional, TestStatus Status);

public enum TestStatus
{
    Ready
}
