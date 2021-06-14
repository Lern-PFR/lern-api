using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public class Answer : IModelBase
    {
        [ReadOnly(true), Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ReadOnly(true), Required]
        public Guid QuestionId { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [Required, MinLength(3), MaxLength(300)]
        public string Text { get; set; }
        [Required]
        public bool Valid { get; set; }
    }
}
