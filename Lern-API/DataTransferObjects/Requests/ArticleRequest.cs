using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Lern_API.Helpers.Validation;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Services.Database;

namespace Lern_API.DataTransferObjects.Requests
{
    public class ArticleRequest
    {
        [Required]
        public Guid ConceptId { get; set; }
        [Required, MinLength(3), MaxLength(150)]
        public string Title { get; set; }
        [Required, MinLength(3), MaxLength(500)]
        public string Description { get; set; }
        [Required, MinLength(3)]
        public string Content { get; set; }
    }
    
    [ExcludeFromCodeCoverage]
    public class ArticleRequestValidator : AbstractValidator<ArticleRequest>
    {
        public ArticleRequestValidator(IDatabaseService<Concept, ConceptRequest> conceptService)
        {
            RuleFor(x => x.ConceptId).NotNull().MustExistInDatabase(conceptService);
            RuleFor(x => x.Title).NotEmpty().Length(3, 150);
            RuleFor(x => x.Description).NotEmpty().Length(3, 500);
            RuleFor(x => x.Content).NotEmpty().MinimumLength(3);
            
            RuleSet("Update", () =>
            {
                RuleFor(x => x.ConceptId).NotNull().MustExistInDatabase(conceptService);
                RuleFor(x => x.Title).NotEmpty().Length(3, 150);
                RuleFor(x => x.Description).NotEmpty().Length(3, 500);
                RuleFor(x => x.Content).NotEmpty().MinimumLength(3);
            });
        }
    }
}
