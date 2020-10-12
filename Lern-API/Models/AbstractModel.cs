using System;
using Lern_API.Helpers.Swagger;
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
    }
}
