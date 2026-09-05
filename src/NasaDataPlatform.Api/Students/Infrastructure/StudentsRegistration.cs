using NasaData.Platform.API.Students.Application;
using NasaData.Platform.API.Students.Application.Ports;
using NasaData.Platform.API.Students.Domain.Repositories;
using NasaData.Platform.API.Students.Infrastructure.Integration;
using NasaData.Platform.API.Students.Infrastructure.Persistence.EFC;
namespace NasaData.Platform.API.Students.Infrastructure;
public static class StudentsRegistration
{
    public static IServiceCollection AddStudents(this IServiceCollection services) => services
        .AddScoped<IStudentRepository, StudentRepository>()
        .AddScoped<IMissionEvaluationPort, MissionEvaluationAdapter>()
        .AddScoped<StudentService>();
}
