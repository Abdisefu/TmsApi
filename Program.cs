using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TmsApi;             
using TmsApi.Services; 
using TmsApi.Workers;
using Scalar.AspNetCore; 

var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "DefaultScheme";
    options.DefaultChallengeScheme = "DefaultScheme";
})
.AddScheme<AuthenticationSchemeOptions, MyCustomHandler>("DefaultScheme", null);

builder.Services.AddAuthorization();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddHostedService<EnrollmentWorker>(); 

builder.Services.AddControllers(); 
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(); // Required for Scalar API metadata tracking

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")       
    .ValidateDataAnnotations()          
    .ValidateOnStart();                  

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;   
    options.ValidateOnBuild = true;  
});

var app = builder.Build(); 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRouting(); 

app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers(); 

app.MapGet("/api/assessments/results", () => 
{
    return Results.Ok(System.Array.Empty<string>()); 
})
.RequireAuthorization(); 

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

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