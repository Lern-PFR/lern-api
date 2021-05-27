using System;
using System.ComponentModel;

namespace Lern_API.Models
{
    public class Result : ITimestamp
    {
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public Guid QuestionId { get; set; }
        [ReadOnly(true)]
        public Question Question { get; set; }
        [ReadOnly(true)]
        public Guid UserId { get; set; }
        [ReadOnly(true)]
        public User User { get; set; }
        [ReadOnly(true)]
        public Guid AnswerId { get; set; }
        [ReadOnly(true)]
        public Answer Answer { get; set; }
    }
}
