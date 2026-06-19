using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication; // 👈 Add this using directive at the top

var builder = WebApplication.CreateBuilder(args); 

// Services: Register authentication with a fallback policy so it doesn't throw a scheme error
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "DefaultScheme";
    options.DefaultChallengeScheme = "DefaultScheme";
})
.AddScheme<AuthenticationSchemeOptions, MyCustomHandler>("DefaultScheme", null); // Helper dummy scheme

builder.Services.AddAuthorization();

var app = builder.Build(); 

app.UseRouting(); // The server finds where the user wants to go.

app.UseAuthentication(); // The server asks: "Who are you?"
app.UseAuthorization(); //The server asks: "Do you have permission to come inside?"

app.MapGet("/api/assessments/results", () => 
{
    return Results.Ok(System.Array.Empty<string>()); 
})
.RequireAuthorization(); 

app.Run();

// 💡 A small custom dummy handler class placed at the very bottom of the file to stop the crash
public class MyCustomHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MyCustomHandler(Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder) : base(options, logger, encoder) { }
    protected override System.Threading.Tasks.Task<AuthenticateResult> HandleAuthenticateAsync() => System.Threading.Tasks.Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
}