using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WorkshopBooking.Api.Infrastructure;
using WorkshopBooking.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ModelStateProblemFactory.Create;
});

builder.Services.AddBookingPersistence();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<BaselineOpenApiFilter>();
    options.SchemaFilter<DemoSchemaFilter>();
});

WebApplication app = builder.Build();
await app.Services.InitializeBookingDatabaseAsync();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseRouting();
app.UseStatusCodePages(async statusCodeContext =>
{
    HttpContext context = statusCodeContext.HttpContext;

    if (ApiExceptionMiddleware.IsApiRequest(context))
    {
        ApiProblemDetails problem = ApiProblemFactory.ForStatusCode(context, context.Response.StatusCode);
        await ApiProblemResponseWriter.WriteAsync(context, problem);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program
{
}
