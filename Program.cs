using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; 
using TmsApi;             
using TmsApi.Data;
using TmsApi.Entities;    
using TmsApi.Services; 
using TmsApi.Workers;
using Scalar.AspNetCore; 
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args); 

// STEP 1: Enable Console SQL Logging & Sensitive Data
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
           .LogTo(Console.WriteLine, LogLevel.Information) 
           .EnableSensitiveDataLogging()); 

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
builder.Services.AddOpenApi(); 

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

// STEP 2: Write an Auto-Seeder (Placed right before app.Run())
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); 

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "sena Gemachu", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Abdi sefu", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "kuma dhara", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Geleta Beri", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Jiru Bira", GPA = 2.5m, IsActive = true }
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to Computer Science", Capacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", Capacity = 40 }
        };
        context.Courses.AddRange(courses);
        
        // Save changes first so PostgreSQL generates IDs for Students and Courses
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, EnrolledAt = System.DateTime.UtcNow },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, EnrolledAt = System.DateTime.UtcNow },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, EnrolledAt = System.DateTime.UtcNow },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, EnrolledAt = System.DateTime.UtcNow }
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

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