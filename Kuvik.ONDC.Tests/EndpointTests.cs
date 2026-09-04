using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kuvik.ONDC.Tests;

public sealed class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> factory;
    public EndpointTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=127.0.0.1;Port=3306;Database=kuvik_ondc;User=root;Password=Kuvik@12345;SslMode=Disabled;");
        this.factory = factory;
    }
    [Fact] public async Task Callback_without_signature_is_nacked() { var id=Guid.NewGuid(); var response=await factory.CreateClient().PostAsJsonAsync("/ondc/on_search",new { context=new { domain="ONDC:FIS12",version="2.0.3",action="on_search",bap_id="kuvikloans.com",bap_uri="https://bap.kuvikloans.com",transaction_id=id,message_id=Guid.NewGuid(),timestamp=DateTimeOffset.UtcNow,location=new { country=new { code="IND" },city=new { code="*" } } },message=new { catalog=new { } } }); Assert.Equal(HttpStatusCode.OK,response.StatusCode); Assert.Contains("NACK",await response.Content.ReadAsStringAsync()); }
    [Fact] public async Task Health_is_available() { var response=await factory.CreateClient().GetAsync("/health"); Assert.Equal(HttpStatusCode.OK,response.StatusCode); }
}
