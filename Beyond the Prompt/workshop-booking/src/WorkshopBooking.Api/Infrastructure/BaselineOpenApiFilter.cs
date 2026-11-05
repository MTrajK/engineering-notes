using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using WorkshopBooking.Api.Controllers;

namespace WorkshopBooking.Api.Infrastructure;

public sealed class BaselineOpenApiFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        Type? controller = context.MethodInfo.DeclaringType;
        if (controller != typeof(WorkshopsController) && controller != typeof(ParticipantsController) && controller != typeof(RegistrationsController))
        {
            return;
        }

        string resource = controller == typeof(WorkshopsController) ? "workshop" : controller == typeof(ParticipantsController) ? "participant" : "registration";
        operation.Summary = context.MethodInfo.Name switch
        {
            "Create" => $"Create a {resource}.",
            "Get" => $"Get a {resource} by ID.",
            "Cancel" => "Cancel a registration and free its seat.",
            _ => $"List {resource}s.",
        };
        operation.Description = resource switch
        {
            "workshop" => "Title: 1–200 and topic: 1–100 UTF-16 units after trimming. Capacity: 1–2147483647. Creation start must be strictly future and end after start. Timestamp input requires Z or a numeric offset; output is UTC Z with seven fractional digits. Choose future creation dates when rehearsing; schema examples are illustrative, not stored records.",
            "participant" => "Name: 1–100 UTF-16 units; email: at most 254, both after trimming. Email has one @, no whitespace/control characters or < > , ; delimiters, and passes EmailAddressAttribute. No dotted-domain requirement. Whole-address invariant uppercase uniqueness; display casing is preserved. Duplicate email: participant_email_conflict.",
            _ => "Creation checks workshop then participant existence, then workshop_started, registration_duplicate, workshop_full. Captures time after acquiring the mutation gate. Cancellation is allowed at any time and physically removes the row. Seed examples exist only on a freshly initialized demo host; client-created IDs and timestamps are server-owned.",
        };
        if (operation.Responses is not null)
        {
            if (context.MethodInfo.Name == "Create" && resource != "registration")
            {
                ((OpenApiResponse)operation.Responses["201"]).Headers = new Dictionary<string, IOpenApiHeader>(StringComparer.Ordinal)
                {
                    ["Location"] = new OpenApiHeader { Description = $"URL of the created {resource} detail endpoint.", Schema = new OpenApiSchema { Type = JsonSchemaType.String } },
                };
            }

            foreach ((string status, IOpenApiResponse response) in operation.Responses)
            {
                if (response is OpenApiResponse concrete && int.TryParse(status, out int code) && code >= 400 && concrete.Content is not null)
                {
                    IOpenApiSchema schema = context.SchemaGenerator.GenerateSchema(typeof(ApiProblemDetails), context.SchemaRepository);
                    concrete.Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
                    {
                        ["application/problem+json"] = new OpenApiMediaType { Schema = schema },
                    };
                    concrete.Description = code switch
                    {
                        400 => "Validation failure. Field errors use public camelCase keys; JSON body errors use $. No write occurs.",
                        404 => "The required resource does not exist.",
                        409 => "Business conflict identified by the stable code field; no write occurs.",
                        _ => concrete.Description
                    };
                }
            }
        }
        if (operation.Parameters is null)
        {
            return;
        }

        foreach (IOpenApiParameter parameter in operation.Parameters)
        {
            if (parameter is not OpenApiParameter value)
            {
                continue;
            }

            value.Description = value.Name switch
            {
                "topic" => "Optional trimmed, invariant case-insensitive exact match. Empty/whitespace values are invalid.",
                "workshopId" => "Required nonempty workshop GUID. Unknown workshop returns workshop_not_found.",
                "id" => "Required nonempty GUID. Malformed or empty GUID returns validation_failed.",
                _ => value.Description,
            };
            if (value.Name == "workshopId")
            {
                value.Example = JsonValue.Create("10000000-0000-0000-0000-000000000001");
            }
        }
    }
}

public sealed class DemoSchemaFilter(TimeProvider clock) : ISchemaFilter
{
    private void ApplySchema(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateWorkshopRequest))
        {
            DateTimeOffset now = clock.GetUtcNow();
            schema.Example = new JsonObject { ["title"] = "Designing Useful APIs", ["topic"] = "API Design", ["capacity"] = 12, ["startsAt"] = now.AddDays(7).ToString("O"), ["endsAt"] = now.AddDays(7).AddHours(2).ToString("O") };
        }
        if (context.Type == typeof(CreateParticipantRequest))
        {
            schema.Example = new JsonObject { ["name"] = "Jordan Lake", ["email"] = "Jordan.Lake@example.test" };
        }

        if (context.Type == typeof(CreateRegistrationRequest))
        {
            schema.Example = new JsonObject { ["workshopId"] = "10000000-0000-0000-0000-000000000001", ["participantId"] = "20000000-0000-0000-0000-000000000003" };
        }

        if (context.Type == typeof(ApiProblemDetails))
        {
            schema.Description = "Shared problem contract. code is stable; type is urn:workshop-booking:problem:<code>; instance is the path without query; traceId is opaque. errors is present only for validation, keyed by public field name or $.";
            schema.Example = new JsonObject { ["type"] = "urn:workshop-booking:problem:validation_failed", ["title"] = "Validation failed", ["status"] = 400, ["detail"] = "Correct the invalid fields.", ["instance"] = "/api/registrations", ["code"] = "validation_failed", ["traceId"] = "example-trace-id", ["errors"] = new JsonObject { ["workshopId"] = new JsonArray("A nonempty workshop ID is required.") } };
        }
        if (schema.Properties is null)
        {
            return;
        }

        foreach ((string name, IOpenApiSchema property) in schema.Properties)
        {
            if (property is OpenApiSchema concrete && name is "startsAt" or "endsAt" or "createdAt")
            {
                concrete.Description = "ISO 8601 instant with explicit Z or numeric offset on input. UTC Z output preserves seven fractional digits (100 ns).";
            }
        }
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema concrete)
        {
            ApplySchema(concrete, context);
        }
    }
}
