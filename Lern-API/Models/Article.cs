using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public class Article : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true), Required]
        public Guid ConceptId { get; set; }
        [ReadOnly(true), Required]
        public Concept Concept { get; set; }
        [Required, MinLength(3), MaxLength(150)]
        public string Title { get; set; }
        [Required, MinLength(3), MaxLength(500)]
        public string Description { get; set; }
        [Required, MinLength(3)]
        public string Content { get; set; }
    }
}
