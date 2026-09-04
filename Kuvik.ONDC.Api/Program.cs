using Kuvik.ONDC.Api.Configuration;
using Kuvik.ONDC.Api.Data;
using Kuvik.ONDC.Api.Middleware;
using Kuvik.ONDC.Api.Services;
using Kuvik.ONDC.Api.Services.Signature;
using Kuvik.ONDC.Api.Services.Transaction;
using Kuvik.ONDC.Api.Services.Validation;
using Microsoft.EntityFrameworkCore;

DotEnv.LoadForLocalDevelopment();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<OndcOptions>().Bind(builder.Configuration.GetSection(OndcOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
var databaseConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration.GetConnectionString("Ondc") ?? throw new InvalidOperationException("DefaultConnection is not configured.");
builder.Services.AddDbContext<OndcDbContext>(o => o.UseMySQL(databaseConnection));
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient<IOndcClient, OndcClient>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IOndcContextService, OndcContextService>();
builder.Services.AddSingleton<ILocalMockKeyStore, LocalMockKeyStore>();
builder.Services.AddSingleton<ISubscriberKeyResolver, ConfigurationSubscriberKeyResolver>();
builder.Services.AddSingleton<IOndcSignatureService, OndcSignatureService>();
builder.Services.AddSingleton<IOndcEncryptionService, OndcEncryptionService>();
builder.Services.AddSingleton<IFis12Validator, Fis12Validator>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OndcDbContext>();
    await db.Database.EnsureCreatedAsync();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RawBodyMiddleware>();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("PreProduction")) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapGet("/health", async (OndcDbContext db, CancellationToken ct) => await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "Healthy", database = "Connected" }) : Results.Json(new { status = "Unhealthy", database = "Disconnected" }, statusCode: 503));
app.MapControllers();
app.Run();
public partial class Program { }
