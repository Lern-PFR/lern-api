using System;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;

namespace Lern_API.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasWriteAccess<T>(User user, T entity, CancellationToken token = default);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserService _users;
        private readonly IDatabaseService<Subject, SubjectRequest> _subjects;
        private readonly IDatabaseService<Module, ModuleRequest> _modules;
        private readonly IDatabaseService<Concept, ConceptRequest> _concepts;
        private readonly IDatabaseService<Course, CourseRequest> _courses;
        private readonly IDatabaseService<Exercise, ExerciseRequest> _exercises;

        public AuthorizationService(
            IUserService users,
            IDatabaseService<Subject, SubjectRequest> subjects,
            IDatabaseService<Module, ModuleRequest> modules,
            IDatabaseService<Concept, ConceptRequest> concepts,
            IDatabaseService<Course, CourseRequest> courses,
            IDatabaseService<Exercise, ExerciseRequest> exercises
        )
        {
            _users = users;
            _subjects = subjects;
            _modules = modules;
            _concepts = concepts;
            _courses = courses;
            _exercises = exercises;
        }
        
        public async Task<bool> HasWriteAccess<T>(User user, T entity, CancellationToken token = default)
        {
            if (user.Admin)
                return true;

            return entity switch
            {
                User targetUser => targetUser.Id == user.Id,
                Subject subject => subject.AuthorId == user.Id,
                Module module => await HasWriteAccess(user, await _subjects.Get(module.SubjectId, token), token),
                Concept concept => await HasWriteAccess(user, await _modules.Get(concept.ModuleId, token), token),
                Course course => await HasWriteAccess(user, await _concepts.Get(course.ConceptId, token), token),
                Exercise exercise when exercise.ConceptId.HasValue => await HasWriteAccess(user, await _concepts.Get(exercise.ConceptId.Value, token), token),
                Exercise exercise when exercise.CourseId.HasValue => await HasWriteAccess(user, await _courses.Get(exercise.CourseId.Value, token), token),
                Question question => await HasWriteAccess(user, await _exercises.Get(question.ExerciseId, token), token),
                _ => throw new InvalidOperationException($"The provided entity ({entity.GetType().FullName}) cannot be checked for write access")
            };
        }
    }
}
