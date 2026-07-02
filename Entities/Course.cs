using System.Collections.Generic;

namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    
    // Ensure this is updated for your exercise name change
    public int MaxCapacity { get; set; } 
    
    // This line was crashing because it couldn't find 'Enrollment'
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}