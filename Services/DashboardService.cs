using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class DashboardService(TmsDbContext context)
{
    private readonly TmsDbContext _context = context;

    // TODO 1: Paginated Student List
    public async Task<List<Student>> GetPagedStudentsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        int page = pageNumber < 1 ? 1 : pageNumber;
        int size = pageSize < 1 ? 20 : pageSize;
        int rowsToSkip = (page - 1) * size;

        return await _context.Students
            .OrderBy(s => s.Name)
            .Skip(rowsToSkip)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    // TODO 2: Top 5 Courses by Enrollment
    public async Task<List<CourseSummaryDto>> GetTopCoursesAsync(CancellationToken cancellationToken)
    {
        return await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(group => new CourseSummaryDto
            {
                CourseTitle = group.Key,
                EnrollmentCount = group.Count()
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);
    }
}

public class CourseSummaryDto
{
    public string CourseTitle { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}