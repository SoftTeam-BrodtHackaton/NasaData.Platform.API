using Microsoft.EntityFrameworkCore;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using NasaData.Platform.API.Students.Domain.Model;
using NasaData.Platform.API.Students.Domain.Repositories;
namespace NasaData.Platform.API.Students.Infrastructure.Persistence.EFC;
public class StudentRepository(AppDbContext context) : BaseRepository<Student>(context), IStudentRepository
{
    public Task<Student?> FindByStudentIdAsync(string id) => Context.Set<Student>().Include(x => x.Attempts).SingleOrDefaultAsync(x => x.StudentId == id);
}
