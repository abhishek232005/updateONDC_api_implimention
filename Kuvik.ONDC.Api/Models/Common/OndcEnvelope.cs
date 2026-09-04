using System.Text.Json;
namespace Kuvik.ONDC.Api.Models.Common;
public sealed class OndcEnvelope { public OndcContext Context { get; init; } = new(); public JsonElement Message { get; init; } public OndcError? Error { get; init; } }
public sealed class OndcContext { public string Domain { get; init; } = ""; public OndcLocation Location { get; init; } = new(); public string Version { get; init; } = ""; public string Action { get; init; } = ""; public string BapId { get; init; } = ""; public string BapUri { get; init; } = ""; public string? BppId { get; init; } public string? BppUri { get; init; } public string TransactionId { get; init; } = ""; public string MessageId { get; init; } = ""; public DateTimeOffset Timestamp { get; init; } public string? Ttl { get; init; } }
public sealed class OndcLocation { public OndcCode Country { get; init; } = new(); public OndcCode City { get; init; } = new(); }
public sealed class OndcCode { public string Code { get; init; } = ""; }
public sealed class OndcError { public string? Type { get; init; } public string? Code { get; init; } public string? Message { get; init; } }
public sealed class OndcAcknowledgement { public OndcContext? Context { get; init; } public AckMessage Message { get; init; } = new(); public OndcError? Error { get; init; } }
public sealed class AckMessage { public Ack Ack { get; init; } = new(); }
public sealed class Ack { public string Status { get; init; } = "ACK"; }
