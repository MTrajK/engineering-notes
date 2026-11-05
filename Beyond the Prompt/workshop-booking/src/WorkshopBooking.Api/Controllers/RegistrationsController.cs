using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WorkshopBooking.Api.Infrastructure;
using WorkshopBooking.Application.Registrations;

namespace WorkshopBooking.Api.Controllers;

public sealed record CreateRegistrationRequest
{
    [Required]
    public Guid? WorkshopId
    {
        get; init;
    }
    [Required]
    public Guid? ParticipantId
    {
        get; init;
    }
}
public sealed record RegistrationResponse(Guid Id, Guid WorkshopId, Guid ParticipantId, DateTimeOffset CreatedAt);

[ApiController]
[Route("api/registrations")]
public sealed class RegistrationsController(RegistrationService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateRegistrationRequest request, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this,
        await service.CreateAsync(request.WorkshopId, request.ParticipantId, cancellationToken), value => StatusCode(StatusCodes.Status201Created, Map(value)));

    [HttpGet]
    [ProducesResponseType(typeof(RegistrationResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List([FromQuery, Required] Guid? workshopId, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this,
        await service.ListAsync(workshopId, cancellationToken), values => Ok(values.Select(Map).ToArray()));

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) => OutcomeHttpMapper.Map(this, await service.CancelAsync(id, cancellationToken), _ => NoContent());

    private static RegistrationResponse Map(RegistrationDetails value) => new(value.Id, value.WorkshopId, value.ParticipantId, value.CreatedAt);
}
