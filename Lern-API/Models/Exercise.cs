using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public class Exercise : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        public Guid? ConceptId { get; set; }
        public Guid? LessonId { get; set; }
        public int? LessonVersion { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(10), MaxLength(300)]
        public string Description { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public int Order { get; set; }
        [ReadOnly(true)]
        public List<Question> Questions { get; set; }
    }
}
