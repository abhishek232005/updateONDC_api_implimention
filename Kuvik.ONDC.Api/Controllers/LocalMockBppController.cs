using System.Text;
using System.Text.Json;
using Kuvik.ONDC.Api.Configuration;
using Kuvik.ONDC.Api.Models.Common;
using Kuvik.ONDC.Api.Services.Signature;
using Kuvik.ONDC.Api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kuvik.ONDC.Api.Controllers;

/// <summary>Development-only BPP simulator. It never represents an ONDC participant or uses production credentials.</summary>
[ApiController, Route("mock/ondc")]
public sealed class LocalMockBppController(IOptions<OndcOptions> options, IOndcSignatureService signature, IHttpClientFactory clients, ITransactionRepository transactions) : ControllerBase
{
    static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    [HttpPost("search")] public Task<IActionResult> Search([FromBody] OndcEnvelope request, CancellationToken ct) => Process(request, "on_search", SearchMessage(), ct);
    [HttpPost("select")] public Task<IActionResult> Select([FromBody] OndcEnvelope request, CancellationToken ct) => Process(request, "on_select", new { order = new { provider = new { id = "local-provider-001" }, items = new[] { new { id = "local-personal-loan-001", xinput = new { form = new { id = "LOCAL-F01" } } } } } }, ct);
    [HttpPost("confirm")] public Task<IActionResult> Confirm([FromBody] OndcEnvelope request, CancellationToken ct) => Process(request, "on_confirm", new { order = new { id = $"local-application-{request.Context.TransactionId}", status = "ACTIVE", provider = new { id = "local-provider-001" }, items = new[] { new { id = "local-personal-loan-001" } } } }, ct);
    [HttpPost("status")] public Task<IActionResult> Status([FromBody] OndcEnvelope request, CancellationToken ct) => Process(request, "on_status", new { order = new { id = "local-application-001", status = "ACTIVE", provider = new { id = "local-provider-001" }, items = new[] { new { id = "local-personal-loan-001" } } } }, ct);
    [HttpGet("transactions/{transactionId}")]
    public async Task<IActionResult> Transaction(string transactionId, CancellationToken ct)
    {
        if (!options.Value.LocalMock.Enabled) return NotFound();
        var transaction = await transactions.GetAsync(transactionId, ct);
        return transaction is null ? NotFound() : Ok(new { transaction.TransactionId, transaction.MessageId, transaction.Action, state = transaction.State.ToString(), transaction.ProviderId, transaction.ItemId, transaction.ApplicationReference, transaction.CreatedAt, transaction.UpdatedAt });
    }

    async Task<IActionResult> Process(OndcEnvelope request, string callbackAction, object message, CancellationToken ct)
    {
        var o = options.Value;
        if (!o.LocalMock.Enabled) return NotFound();
        signature.Verify(Raw(), Request.Headers.Authorization.FirstOrDefault());
        var context = new OndcContext { Domain = o.Domain, Version = o.Version, Action = callbackAction, BapId = request.Context.BapId, BapUri = request.Context.BapUri, BppId = o.LocalMock.BppId, BppUri = o.LocalMock.BppUri, TransactionId = request.Context.TransactionId, MessageId = Guid.NewGuid().ToString(), Timestamp = DateTimeOffset.UtcNow, Ttl = "PT10M", Location = request.Context.Location };
        var raw = JsonSerializer.Serialize(new OndcEnvelope { Context = context, Message = JsonSerializer.SerializeToElement(message, Json) }, Json);
        using var callbackRequest = new HttpRequestMessage(HttpMethod.Post, CallbackUri(context.BapUri, callbackAction)) { Content = new StringContent(raw, Encoding.UTF8, "application/json") };
        callbackRequest.Headers.TryAddWithoutValidation("Authorization", signature.CreateMockAuthorization(raw));
        using var response = await clients.CreateClient().SendAsync(callbackRequest, ct);
        response.EnsureSuccessStatusCode();
        return Ok(new { message = new { ack = new { status = "ACK" } } });
    }
    string Raw() => HttpContext.Items["OndcRawBody"] as string ?? throw new InvalidOperationException("Raw request body is unavailable.");
    static Uri CallbackUri(string bapUri, string action) => new(new Uri(bapUri.EndsWith('/') ? bapUri : bapUri + "/"), action);
    static object SearchMessage() => new { catalog = new { descriptor = new { name = "LOCAL MOCK BPP — NOT ONDC" }, providers = new[] { new { id = "local-provider-001", descriptor = new { name = "Local Test Lender" }, items = new[] { new { id = "local-personal-loan-001", descriptor = new { code = "PERSONAL_LOAN", name = "Mock Personal Loan" } } } } } } };
}
