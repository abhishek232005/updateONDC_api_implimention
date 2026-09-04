using Kuvik.ONDC.Api.Models.Common;
using Kuvik.ONDC.Api.Services;
using Kuvik.ONDC.Api.Services.Signature;
using Kuvik.ONDC.Api.Services.Transaction;
using Kuvik.ONDC.Api.Services.Validation;
using Microsoft.AspNetCore.Mvc;
namespace Kuvik.ONDC.Api.Controllers;
[ApiController, Route("ondc")]
public sealed class OndcController(IFis12Validator validator, ITransactionService transactions, IOndcClient client, IOndcSignatureService signature, ILogger<OndcController> log) : ControllerBase {
 [HttpPost("search")] public Task<IActionResult> Search([FromBody] OndcEnvelope request,CancellationToken ct) => Send(request,"search",TransactionState.SearchInitiated,ct);
 [HttpPost("select")] public Task<IActionResult> Select([FromBody] OndcEnvelope request,CancellationToken ct) => Send(request,"select",TransactionState.SelectInitiated,ct);
 [HttpPost("confirm")] public Task<IActionResult> Confirm([FromBody] OndcEnvelope request,CancellationToken ct) => Send(request,"confirm",TransactionState.ConfirmInitiated,ct);
 [HttpPost("status")] public Task<IActionResult> Status([FromBody] OndcEnvelope request,CancellationToken ct) => Send(request,"status",TransactionState.StatusRequested,ct);
 [HttpPost("on_search")] public Task<IActionResult> OnSearch([FromBody] OndcEnvelope request,CancellationToken ct) => Callback(request,"on_search",TransactionState.SearchReceived,ct);
 [HttpPost("on_select")] public Task<IActionResult> OnSelect([FromBody] OndcEnvelope request,CancellationToken ct) => Callback(request,"on_select",TransactionState.SelectReceived,ct);
 [HttpPost("on_confirm")] public Task<IActionResult> OnConfirm([FromBody] OndcEnvelope request,CancellationToken ct) => Callback(request,"on_confirm",TransactionState.ConfirmReceived,ct);
 [HttpPost("on_status")] public Task<IActionResult> OnStatus([FromBody] OndcEnvelope request,CancellationToken ct) => Callback(request,"on_status",TransactionState.StatusReceived,ct);
 async Task<IActionResult> Send(OndcEnvelope request,string action,TransactionState state,CancellationToken ct) { validator.Validate(request,action); if(action!="search") await transactions.EnsureForActionAsync(request,action,ct); await transactions.RecordAsync(request,state,ct); var raw=Raw(); await client.ForwardAsync(request,raw,ct); log.LogInformation("Forwarded ONDC {Action}; transaction={TransactionId} message={MessageId}",action,request.Context.TransactionId,request.Context.MessageId); return Accepted(new { transaction_id=request.Context.TransactionId,message_id=request.Context.MessageId }); }
 async Task<IActionResult> Callback(OndcEnvelope request,string action,TransactionState state,CancellationToken ct) { try { signature.Verify(Raw(),Request.Headers.Authorization.FirstOrDefault() ?? Request.Headers["X-Gateway-Authorization"].FirstOrDefault()); validator.Validate(request,action); await transactions.RecordAsync(request,state,ct); log.LogInformation("Accepted ONDC {Action}; transaction={TransactionId} message={MessageId}",action,request.Context.TransactionId,request.Context.MessageId); return Ok(Ack(request.Context)); } catch(Exception ex) when(ex is UnauthorizedAccessException or ArgumentException or InvalidOperationException or KeyNotFoundException) { log.LogWarning("Rejected ONDC callback {Action}; transaction={TransactionId}; reason={Reason}",action,request.Context.TransactionId,ex.Message); return Ok(Nack(request.Context,ex)); } }
 string Raw() => HttpContext.Items["OndcRawBody"] as string ?? throw new InvalidOperationException("Raw request body is unavailable.");
 static OndcAcknowledgement Ack(OndcContext context) => new() { Context=context,Message=new AckMessage { Ack=new Ack { Status="ACK" } } };
 static OndcAcknowledgement Nack(OndcContext context,Exception ex) => new() { Context=context,Message=new AckMessage { Ack=new Ack { Status="NACK" } },Error=new OndcError { Type="CORE-ERROR",Code=ex is UnauthorizedAccessException?"30000":"30001",Message=ex.Message } };
}
