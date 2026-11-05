using Microsoft.AspNetCore.Mvc;
using WorkshopBooking.Application.Outcomes;

namespace WorkshopBooking.Api.Infrastructure;

public static class OutcomeHttpMapper
{
    public static IActionResult Map<TValue>(
        ControllerBase controller,
        Outcome<TValue> outcome,
        Func<TValue, IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return outcome.IsSuccess
            ? onSuccess(outcome.Value)
            : ApiProblemResponseWriter.ToObjectResult(
                ApiProblemFactory.FromFailure(controller.HttpContext, outcome.Failure));
    }
}
