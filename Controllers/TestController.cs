using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;          
using Microsoft.EntityFrameworkCore; 
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); 
        
        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> [EXPERIMENT] STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Students
                .Where(s => IsHonorRoll(s.GPA)) 
                .ToList();

            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message, Hint = "EF Core cannot convert custom C# code into SQL dialect." });
        }
    }
    [HttpGet("translation-fix-server")]
    public IActionResult TestTranslationFixServer()
    {
        Console.WriteLine("\n>>> [FIX 1] Running server-side evaluation...");
        
        var students = context.Students
            .Where(s => s.GPA >= 3.5m) 
            .ToList();

        Console.WriteLine(">>> Finished server-side filtering.\n");
        return Ok(students);
    }
    [HttpGet("translation-fix-client")]
    public IActionResult TestTranslationFixClient()
    {
        Console.WriteLine("\n>>> [FIX 2] Running client-side evaluation...");
        
        var students = context.Students
            .AsEnumerable() 
            .Where(s => IsHonorRoll(s.GPA)) 
            .ToList();

        Console.WriteLine(">>> Finished client-side filtering.\n");
        return Ok(students);
    }
    [HttpGet("reporting/active-high-gpa-count")]
    public async Task<IActionResult> GetActiveHighGpaCount()
    {
        Console.WriteLine("\n>>> [REPORT 1] Calculating active students count (GPA >= 3.0)...");
        
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync(); 

        return Ok(new { ActiveHighGpaCount = count });
    }
    [HttpGet("reporting/courses-by-enrollment")]
    public async Task<IActionResult> GetCoursesByEnrollment()
    {
        Console.WriteLine("\n>>> [REPORT 2] Fetching courses sorted by enrollment density...");
        
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync(); 

        return Ok(list);
    }
    [HttpGet("reporting/average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        Console.WriteLine("\n>>> [REPORT 3] Aggregating average student GPA per course...");
        
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync(); 

        return Ok(list);
    }
    [HttpGet("reporting/unenrolled-students-subquery")]
    public async Task<IActionResult> GetUnenrolledStudentsSubquery()
    {
        Console.WriteLine("\n>>> [REPORT 4A] Finding unenrolled students using NOT EXISTS subquery...");
        
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync(); 

        return Ok(list);
    }

    // 4B. Which students have zero enrollments? (Approach B: LeftJoin)
    [HttpGet("reporting/unenrolled-students-leftjoin")]
    public async Task<IActionResult> GetUnenrolledStudentsLeftJoin()
    {
        Console.WriteLine("\n>>> [REPORT 4B] Finding unenrolled students using LeftJoin configuration...");
        
        var list = await context.Students
            .LeftJoin(context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync(); 

        return Ok(list);
    }
}