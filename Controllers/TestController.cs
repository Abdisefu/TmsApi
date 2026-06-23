using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    // Non-translatable helper method
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

    // RESOLUTION 1: Server-Side Evaluation (Highly Preferred)
    [HttpGet("translation-fix-server")]
    public IActionResult TestTranslationFixServer()
    {
        Console.WriteLine("\n>>> [FIX 1] Running server-side evaluation...");
        
        // Logic written inline so the LINQ provider can build a relational AST
        var students = context.Students
            .Where(s => s.GPA >= 3.5m) 
            .ToList();

        Console.WriteLine(">>> Finished server-side filtering.\n");
        return Ok(students);
    }

    // RESOLUTION 2: Client-Side Evaluation (Use cautiously!)
    [HttpGet("translation-fix-client")]
    public IActionResult TestTranslationFixClient()
    {
        Console.WriteLine("\n>>> [FIX 2] Running client-side evaluation...");
        
        var students = context.Students
            .AsEnumerable() // Wires open the pipe: pulls EVERY single row into memory first
            .Where(s => IsHonorRoll(s.GPA)) // Safe to use custom C# now because it's running in RAM
            .ToList();

        Console.WriteLine(">>> Finished client-side filtering.\n");
        return Ok(students);
    }
}