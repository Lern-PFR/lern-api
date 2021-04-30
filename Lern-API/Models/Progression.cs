using System;
using System.ComponentModel;

namespace Lern_API.Models
{
    public class Progression
    {
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public Guid UserId { get; set; }
        [ReadOnly(true)]
        public User User { get; set; }
        [ReadOnly(true)]
        public Guid SubjectId { get; set; }
        [ReadOnly(true)]
        public Subject Subject { get; set; }
        [ReadOnly(true)]
        public Guid ConceptId { get; set; }
        [ReadOnly(true)]
        public Concept Concept { get; set; }
        [ReadOnly(true)]
        public bool Suspended { get; set; }
    }
}
