using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WorkshopBooking.Api.Infrastructure;
using WorkshopBooking.Application.Workshops;

namespace WorkshopBooking.Api.Controllers;

public sealed record CreateWorkshopRequest
{
    [Required]
    public string? Title
    {
        get; init;
    }
    [Required]
    public string? Topic
    {
        get; init;
    }
    [Required]
    public DateTimeOffset? StartsAt
    {
        get; init;
    }
    [Required]
    public DateTimeOffset? EndsAt
    {
        get; init;
    }
    [Required]
    public int? Capacity
    {
        get; init;
    }
}
public sealed record WorkshopResponse(Guid Id, string Title, string Topic, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int Capacity, int AvailableSeats);

[ApiController]
[Route("api/workshops")]
public sealed class WorkshopsController(WorkshopService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(WorkshopResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateWorkshopRequest request, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this,
        await service.CreateAsync(new CreateWorkshop(request.Title, request.Topic, request.StartsAt, request.EndsAt, request.Capacity), cancellationToken),
        value => CreatedAtAction(nameof(Get), new { id = value.Id }, Map(value)));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkshopResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this, await service.GetAsync(id, cancellationToken), value => Ok(Map(value)));

    [HttpGet]
    [ProducesResponseType(typeof(WorkshopResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] string? topic, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this,
        await service.ListAsync(Request.Query.ContainsKey("topic") ? topic ?? string.Empty : null, cancellationToken), values => Ok(values.Select(Map).ToArray()));

    private static WorkshopResponse Map(WorkshopDetails value) => new(value.Id, value.Title, value.Topic, value.StartsAt, value.EndsAt, value.Capacity, value.AvailableSeats);
}
