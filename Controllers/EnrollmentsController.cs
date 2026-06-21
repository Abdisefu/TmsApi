using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TmsApi.Services;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
    {
        // GET /api/enrollments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await enrollmentService.GetAllAsync();
            return Ok(enrollments);
        }

        // GET /api/enrollments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var record = await enrollmentService.GetByIdAsync(id);
            return record is not null ? Ok(record) : NotFound();
        }

        // POST /api/enrollments -> Creates a resource and returns 201 Created with Location Header
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
        {
            // Call our structured logging-enabled enrollment service layer
            var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
            
            // This automagically constructs the URI string: /api/enrollments/{record.Id}
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }
    }

    // --- Request Payload Structure ---
    public record CreateEnrollmentRequest(string StudentId, string CourseCode);
}