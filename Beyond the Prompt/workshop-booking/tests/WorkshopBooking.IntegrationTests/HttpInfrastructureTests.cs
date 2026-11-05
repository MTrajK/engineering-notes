using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class HttpInfrastructureTests
{
    [Theory]
    [InlineData("validation", HttpStatusCode.BadRequest, "validation_failed")]
    [InlineData("missing", HttpStatusCode.NotFound, "workshop_not_found")]
    [InlineData("conflict", HttpStatusCode.Conflict, "workshop_full")]
    public async Task ApplicationFailuresMapToTheDocumentedProblemContract(
        string kind,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync($"/api/__infrastructure-test/outcomes/{kind}?source=infra", TestContext.Current.CancellationToken);
        JsonElement problem = await ReadJsonAsync(response);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
        Assert.Equal($"urn:workshop-booking:problem:{expectedCode}", problem.GetProperty("type").GetString());
        Assert.Equal($"/api/__infrastructure-test/outcomes/{kind}", problem.GetProperty("instance").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        Assert.True(problem.GetProperty("status").GetInt32() == (int)expectedStatus);

        if (kind == "validation")
        {
            Assert.True(problem.GetProperty("errors").TryGetProperty("participantId", out _));
        }
    }

    [Fact]
    public async Task SuccessfulApplicationOutcomeUsesTheSuppliedSuccessResponse()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/__infrastructure-test/outcomes/success", TestContext.Current.CancellationToken);
        JsonElement body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("completed", body.GetProperty("value").GetString());
    }

    [Fact]
    public async Task AutomaticValidationUsesPublicCamelCaseFieldNames()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/validate", JsonContent("{}"), TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");

        Assert.True(problem.GetProperty("errors").TryGetProperty("name", out _));
    }

    [Fact]
    public async Task QueryBindingErrorsUsePublicCamelCaseFieldNames()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/__infrastructure-test/query?workshopId=not-a-guid", TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");

        Assert.True(problem.GetProperty("errors").TryGetProperty("workshopId", out _));
    }

    [Theory]
    [InlineData("{\"name\":", "$")]
    [InlineData("{\"count\":\"not-a-number\"}", "$")]
    public async Task MalformedJsonAndIncompatibleValuesUseTheBodyErrorKey(
        string body,
        string expectedErrorKey)
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/convert", JsonContent(body), TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");

        Assert.True(problem.GetProperty("errors").TryGetProperty(expectedErrorKey, out _));
    }

    [Fact]
    public async Task UnexpectedApiExceptionReturnsGenericProblemWithoutLeakingDetails()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/__infrastructure-test/throw", TestContext.Current.CancellationToken);
        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("internal_error", body, StringComparison.Ordinal);
        Assert.DoesNotContain("private exception detail", body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownApiRouteReturnsProblemDetails()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/no-such-route?query=ignored", TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.NotFound, "route_not_found");

        Assert.Equal("/api/no-such-route", problem.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task UnsupportedMethodReturnsProblemAndPreservesAllowHeader()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/read-only", content: null, cancellationToken: TestContext.Current.CancellationToken);
        await AssertProblemAsync(response, HttpStatusCode.MethodNotAllowed, "method_not_allowed");

        string allowValue = response.Headers.NonValidated
            .Concat(response.Content.Headers.NonValidated)
            .Where(header => string.Equals(header.Key, "Allow", StringComparison.OrdinalIgnoreCase))
            .Select(header => header.Value.ToString())
            .SingleOrDefault() ?? string.Empty;

        Assert.Contains("GET", allowValue, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnsupportedContentTypeReturnsProblemDetails()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using StringContent content = new("{}", Encoding.UTF8, "text/plain");
        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/media", content, TestContext.Current.CancellationToken);

        await AssertProblemAsync(response, HttpStatusCode.UnsupportedMediaType, "unsupported_media_type");
    }

    [Fact]
    public async Task ExplicitOffsetInputIsWrittenInUtcWithTickPrecisionAndNulls()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/timestamp", JsonContent("{\"instant\":\"2040-03-04T05:06:07.1234567+02:00\"}"), TestContext.Current.CancellationToken);
        JsonElement body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("2040-03-04T03:06:07.1234567Z", body.GetProperty("instant").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("optional").ValueKind);
        Assert.Equal("ready", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task TimestampWithoutOffsetIsRejectedAsBodyValidation()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync("/api/__infrastructure-test/timestamp", JsonContent("{\"instant\":\"2040-03-04T05:06:07\"}"), TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest, "validation_failed");

        Assert.True(problem.GetProperty("errors").TryGetProperty("$", out _));
    }

    [Fact]
    public async Task SwaggerUiAndOpenApiDocumentAreAvailableInDevelopment()
    {
        using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage uiResponse = await client.GetAsync("/swagger", TestContext.Current.CancellationToken);
        string ui = await uiResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using HttpResponseMessage documentResponse = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
        Assert.Contains("swagger-ui", ui, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, documentResponse.StatusCode);
        Assert.Equal("application/json", documentResponse.Content.Headers.ContentType?.MediaType);
        Assert.True(documentResponse.Content.Headers.ContentLength > 0);

        await using Stream documentStream = await documentResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(documentStream, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .TryGetProperty("ApiProblemDetails", out _));
        Assert.True(document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/__infrastructure-test/outcomes/{kind}")
            .GetProperty("get")
            .GetProperty("responses")
            .TryGetProperty("409", out _));

        using HttpResponseMessage missingAssetResponse = await client.GetAsync("/swagger/not-a-real-asset.js", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missingAssetResponse.StatusCode);
        Assert.NotEqual("application/problem+json", missingAssetResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerDocumentIsNotEnabledOutsideDevelopment()
    {
        using WebApplicationFactory<Program> baseFactory = new();
        using WebApplicationFactory<Program> productionFactory = baseFactory.WithWebHostBuilder(
            builder => builder.UseEnvironment("Production"));
        using HttpClient client = productionFactory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task TestOnlyControllersAreNotDiscoveredByTheNormalHost()
    {
        using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/api/__infrastructure-test/outcomes/success", TestContext.Current.CancellationToken);
        JsonElement problem = await AssertProblemAsync(response, HttpStatusCode.NotFound, "route_not_found");

        Assert.Equal("/api/__infrastructure-test/outcomes/success", problem.GetProperty("instance").GetString());
    }

    private static StringContent JsonContent(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task<JsonElement> AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        JsonElement problem = await ReadJsonAsync(response);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedCode, problem.GetProperty("code").GetString());
        Assert.Equal((int)expectedStatus, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        return problem;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        await using Stream content = await response.Content.ReadAsStreamAsync();
        using JsonDocument document = await JsonDocument.ParseAsync(content);
        return document.RootElement.Clone();
    }
}
