using System;
using System.ComponentModel.DataAnnotations;
using Lern_API.Helpers.Models;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Responses
{
    public class ProgressionResponse
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public Subject Subject { get; set; }
        public Concept Concept { get; set; }
        public bool Suspended { get; set; }
        public bool Completed { get; set; }
        [Range(0, 100)]
        public double Completion { get; set; }

        public ProgressionResponse(Progression progression)
        {
            this.CloneFrom(progression);
        }
    }
}
