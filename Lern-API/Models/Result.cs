using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lern_API.Models
{
    public class Result : ITimestamp
    {
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [Required, ReadOnly(true)]
        public Guid QuestionId { get; set; }
        [Required, ReadOnly(true)]
        public Question Question { get; set; }
        [Required, ReadOnly(true)]
        public Guid UserId { get; set; }
        [Required, ReadOnly(true)]
        public User User { get; set; }
        [Required, ReadOnly(true)]
        public Guid AnswerId { get; set; }
        [Required, ReadOnly(true)]
        public Answer Answer { get; set; }
    }
}
