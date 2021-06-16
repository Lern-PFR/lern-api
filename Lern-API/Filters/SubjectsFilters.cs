using System.Linq;
using Lern_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Filters
{
    public static class SubjectsFilters
    {
        public static IQueryable<Subject> ValidSubjects(IQueryable<Subject> subjects)
        {
            return subjects
                .Include(subject => subject.Author)
                .Include(subject => subject.Modules.Where(module => module.Concepts.Any()))
                .ThenInclude(module => module.Concepts.Where(concept =>
                    concept.Courses.Any() && concept.Exercises.Any(exercise =>
                        exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Courses)
                .ThenInclude(course => course.Exercises.Where(exercise =>
                    exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid))))
                .ThenInclude(exercise => exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Include(subject => subject.Modules.Where(module => module.Concepts.Any()))
                .ThenInclude(module => module.Concepts.Where(concept =>
                    concept.Courses.Any() && concept.Exercises.Any(exercise =>
                        exercise.Questions.Any(question => question.Answers.Any(answer => answer.Valid)))))
                .ThenInclude(concept => concept.Exercises.Where(exercise => exercise.Questions.Any()))
                .ThenInclude(exercise => exercise.Questions.Where(question => question.Answers.Any(answer => answer.Valid)))
                .ThenInclude(question => question.Answers)
                .Where(subject =>
                    subject.Modules.Any() && subject.Modules.All(module =>
                        module.Concepts.Any() && module.Concepts.All(concept => concept.Courses.Any() &&
                                                                                concept.Exercises.Any() &&
                                                                                concept.Exercises.All(exercise =>
                                                                                    exercise.Questions.Any() &&
                                                                                    exercise.Questions.All(question =>
                                                                                        question.Answers.Any(answer =>
                                                                                            answer.Valid))
                                                                                ))));
        }
    }
}
