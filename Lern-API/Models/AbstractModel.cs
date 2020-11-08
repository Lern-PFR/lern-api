using System;
using Lern_API.Helpers.Database;
using PetaPoco;

namespace Lern_API.Models
{
    public abstract class AbstractModel
    {
        [ReadOnly]
        public Guid Id { get; set; }
        [ReadOnly]
        [ResultColumn(IncludeInAutoSelect.Yes)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly]
        [ResultColumn(IncludeInAutoSelect.Yes)]
        public DateTime UpdatedAt { get; set; }
    }
}
