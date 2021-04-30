using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public class Module : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [Required]
        public Guid SubjectId { get; set; }
        [Required]
        public Subject Subject { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Title { get; set; }
        [Required, MinLength(10), MaxLength(300)]
        public string Description { get; set; }
        [Required]
        public int Order { get; set; }
    }
}
