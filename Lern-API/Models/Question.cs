using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public enum QuestionType
    {
        SingleChoice = 1,
        MultipleChoice = 2,
    }
    
    public class Question : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [Required]
        public Guid ExerciseId { get; set; }
        [Required]
        public Exercise Exercise { get; set; }
        [Required]
        public QuestionType Type { get; set; }
        [Required, MinLength(3), MaxLength(300)]
        public string Statement { get; set; }
        [Required, MinLength(10), MaxLength(3000)]
        public string Explanation { get; set; }
        [Required]
        public List<Answer> Answers { get; set; }
    }
}
