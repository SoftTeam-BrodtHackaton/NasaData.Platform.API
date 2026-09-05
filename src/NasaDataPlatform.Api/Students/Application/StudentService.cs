using NasaData.Platform.API.Shared.Domain.Repositories;
using NasaData.Platform.API.Students.Application.Ports;
using NasaData.Platform.API.Students.Domain.Model;
using NasaData.Platform.API.Students.Domain.Repositories;
namespace NasaData.Platform.API.Students.Application;
public sealed record AnswerResult(bool Correct, int PointsEarned, string Feedback);
public class StudentService(IStudentRepository repository, IMissionEvaluationPort evaluation, IUnitOfWork unitOfWork)
{
    public async Task<Student> GetAsync(string studentId) => await repository.FindByStudentIdAsync(studentId) ?? throw new KeyNotFoundException("Estudiante no encontrado. Se registra al enviar su primera respuesta válida.");
    public async Task<AnswerResult> AnswerAsync(string studentId, string missionId, string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId) || optionId.Length > 20) throw new ArgumentException("optionId inválido.");
        var result = await evaluation.EvaluateAsync(missionId, optionId);
        var student = await repository.FindByStudentIdAsync(studentId);
        if (student == null)
        {
            student = new Student(studentId);
            await repository.AddAsync(student);
        }
        var attempt = student.Answer(missionId, optionId, result.Correct, result.Skills);
        await unitOfWork.CompleteAsync();
        return new(result.Correct, attempt.PointsEarned, result.Feedback);
    }
}
