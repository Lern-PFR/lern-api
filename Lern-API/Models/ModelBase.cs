using System;

namespace Lern_API.Models
{
    public interface ITimestamp
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public interface IModelBase : ITimestamp
    {
        public Guid Id { get; set; }
    }
}
