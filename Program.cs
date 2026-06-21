using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TmsApi;             // Pulls in PaymentOptions from your root folder
using TmsApi.Services; 
using TmsApi.Workers;

var builder = WebApplication.CreateBuilder(args); 

// ==========================================
// SERVICES CONFIGURATION (DI Container)
// ==========================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "DefaultScheme";
    options.DefaultChallengeScheme = "DefaultScheme";
})
.AddScheme<AuthenticationSchemeOptions, MyCustomHandler>("DefaultScheme", null);

builder.Services.AddAuthorization();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddHostedService<EnrollmentWorker>(); // Registers as a Singleton background service

// 1. REGISTER CONTROLLER SERVICES
builder.Services.AddControllers(); 

// --- Exercise 3: Strongly-Typed Options Startup Validation ---
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")       // Binds to the "Payments" section of appsettings.json
    .ValidateDataAnnotations()          // Evaluates the [Required] and [Range] attributes
    .ValidateOnStart();                  // Forces the application to crash if validation fails!

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;   // Catches Singletons capturing Scoped services
    options.ValidateOnBuild = true;  // Forces the app to crash IMMEDIATELY during 'dotnet run'
});
var app = builder.Build(); 
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Then Exception Handler (Points to our /api/error route below)
app.UseExceptionHandler("/api/error"); 

app.UseRouting(); 

app.UseAuthentication(); 

app.UseAuthorization(); 
app.MapControllers(); 

app.MapGet("/api/assessments/results", () => 
{
    return Results.Ok(System.Array.Empty<string>()); 
})
.RequireAuthorization(); 

app.Map("/api/error", () => Results.Problem(
    detail: "An unexpected error occurred.",
    statusCode: 500,
    title: "Internal Server Error"
));

app.Run();
public class MyCustomHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MyCustomHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        System.Text.Encodings.Web.UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override System.Threading.Tasks.Task<AuthenticateResult> HandleAuthenticateAsync() 
        => System.Threading.Tasks.Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
}