using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TmsApi.Services;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
    {
        // GET /api/enrollments -> returns all enrollment records
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await enrollmentService.GetAllAsync();
            return Ok(enrollments);
        }

        // GET /api/enrollments/{id} -> returns one record or a 404 Not Found
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var record = await enrollmentService.GetByIdAsync(id);
            return record is not null ? Ok(record) : NotFound();
        }
    }
}