using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WorkshopBooking.Api.Infrastructure;
using WorkshopBooking.Application.Participants;

namespace WorkshopBooking.Api.Controllers;

public sealed record CreateParticipantRequest
{
    [Required]
    public string? Name
    {
        get; init;
    }
    [Required]
    public string? Email
    {
        get; init;
    }
}
public sealed record ParticipantResponse(Guid Id, string Name, string Email);

[ApiController]
[Route("api/participants")]
public sealed class ParticipantsController(ParticipantService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ParticipantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateParticipantRequest request, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this,
        await service.CreateAsync(request.Name, request.Email, cancellationToken), value => CreatedAtAction(nameof(Get), new { id = value.Id }, Map(value)));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ParticipantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this, await service.GetAsync(id, cancellationToken), value => Ok(Map(value)));

    [HttpGet]
    [ProducesResponseType(typeof(ParticipantResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this, await service.ListAsync(cancellationToken), values => Ok(values.Select(Map).ToArray()));

    private static ParticipantResponse Map(ParticipantDetails value) => new(value.Id, value.Name, value.Email);
}
