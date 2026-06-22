using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TmsApi.Services
{
    public interface IEnrollmentService
    {
        Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
        Task<EnrollmentRecord?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<System.Collections.Generic.IReadOnlyCollection<EnrollmentRecord>> GetAllAsync();
    }

    public record EnrollmentRecord(string Id, string StudentId, string CourseCode, DateTime EnrolledAt);

    public class EnrollmentService : IEnrollmentService
    {
        private static readonly ConcurrentDictionary<string, EnrollmentRecord> _store = new();
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(ILogger<EnrollmentService> logger)
        {
            _logger = logger;
        }

        public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
        {
            var existing = _store.Values
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

            if (existing is not null)
            {
                _logger.LogWarning(
                    "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                    studentId, courseCode, existing.Id);
                
                return Task.FromResult(existing);
            }

            var id = Guid.NewGuid().ToString("N")[..8];
            var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
            _store[id] = record;

            _logger.LogInformation(
                "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
                studentId, courseCode, id);

            return Task.FromResult(record);
        }

        public Task<EnrollmentRecord?> GetByIdAsync(string id)
        {
            _store.TryGetValue(id, out var record);

            if (record is null)
            {
                _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            }

            return Task.FromResult(record);
        }

        public Task<bool> DeleteAsync(string id)
        {
            var removed = _store.TryRemove(id, out _);

            if (removed)
            {
                _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
            }
            else
            {
                _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
            }

            return Task.FromResult(removed);
        }

        public Task<System.Collections.Generic.IReadOnlyCollection<EnrollmentRecord>> GetAllAsync()
        {
            System.Collections.Generic.IReadOnlyCollection<EnrollmentRecord> list = _store.Values.ToList().AsReadOnly();
            return Task.FromResult(list);
        }
    }
}

public class TmsDatabaseException(string message) : Exception(message);