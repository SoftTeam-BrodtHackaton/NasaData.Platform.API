using NasaData.Platform.API.Shared.Domain.Repositories;
using NasaData.Platform.API.Students.Domain.Model;
namespace NasaData.Platform.API.Students.Domain.Repositories;
public interface IStudentRepository : IBaseRepository<Student>
{
    Task<Student?> FindByStudentIdAsync(string id);
}
