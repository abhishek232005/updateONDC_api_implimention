using Kuvik.ONDC.Api.Configuration;
using Kuvik.ONDC.Api.Models.Common;
using Microsoft.Extensions.Options;
namespace Kuvik.ONDC.Api.Services;
public interface IOndcContextService { OndcContext Create(string action,string transactionId,string? bppId=null,string? bppUri=null); }
public sealed class OndcContextService(IOptions<OndcOptions> options) : IOndcContextService { public OndcContext Create(string action,string transactionId,string? bppId=null,string? bppUri=null) { var o=options.Value; return new OndcContext { Domain=o.Domain,Version=o.Version,Action=action,BapId=o.SubscriberId,BapUri=o.SubscriberUrl,BppId=bppId,BppUri=bppUri,TransactionId=transactionId,MessageId=Guid.NewGuid().ToString(),Timestamp=DateTimeOffset.UtcNow,Ttl="PT10M",Location=new OndcLocation { Country=new OndcCode { Code="IND" },City=new OndcCode { Code="*" } } }; } }
