﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services.Database;

namespace Lern_API.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasWriteAccess<T>(User user, T entity, CancellationToken token = default);
        Task<bool> HasAuthorship<T>(User user, T entity, CancellationToken token = default);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserService _users;
        private readonly IDatabaseService<Subject, SubjectRequest> _subjects;
        private readonly IDatabaseService<Module, ModuleRequest> _modules;
        private readonly IDatabaseService<Concept, ConceptRequest> _concepts;
        private readonly IDatabaseService<Lesson, LessonRequest> _lessons;
        private readonly IDatabaseService<Exercise, ExerciseRequest> _exercises;

        public AuthorizationService(
            IUserService users,
            IDatabaseService<Subject, SubjectRequest> subjects,
            IDatabaseService<Module, ModuleRequest> modules,
            IDatabaseService<Concept, ConceptRequest> concepts,
            IDatabaseService<Lesson, LessonRequest> lessons,
            IDatabaseService<Exercise, ExerciseRequest> exercises
        )
        {
            _users = users;
            _subjects = subjects;
            _modules = modules;
            _concepts = concepts;
            _lessons = lessons;
            _exercises = exercises;
        }
        
        public async Task<bool> HasWriteAccess<T>(User user, T entity, CancellationToken token = default)
        {
            if (user == null)
                return false;

            if (user.Admin)
                return true;

            return entity switch
            {
                User targetUser => targetUser.Id == user.Id,
                Subject subject => subject.AuthorId == user.Id,
                Module module => await HasWriteAccess(user, await _subjects.Get(module.SubjectId, token), token),
                Concept concept => await HasWriteAccess(user, await _modules.Get(concept.ModuleId, token), token),
                Lesson lesson => await HasWriteAccess(user, await _concepts.Get(lesson.ConceptId, token), token),
                Exercise exercise when exercise.ConceptId.HasValue => await HasWriteAccess(user, await _concepts.Get(exercise.ConceptId.Value, token), token),
                Exercise exercise when exercise.LessonId.HasValue => await HasWriteAccess(user, await _lessons.Get(exercise.LessonId.Value, token), token),
                Question question => await HasWriteAccess(user, await _exercises.Get(question.ExerciseId, token), token),
                _ => throw new InvalidOperationException($"The provided entity ({entity.GetType().FullName}) cannot be checked for write access")
            };
        }

        public async Task<bool> HasAuthorship<T>(User user, T entity, CancellationToken token = default)
        {
            // Temporary measure, to be able to handle Authorship separately from write access in the future
            return await HasWriteAccess(user, entity, token);
        }
    }
}
