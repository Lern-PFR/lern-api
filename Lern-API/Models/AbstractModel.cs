using System;
using PetaPoco;

namespace Lern_API.Models
{
    public abstract class AbstractModel
    {
        public Guid Id { get; set; }
        [ResultColumn(IncludeInAutoSelect.Yes)]
        public DateTime CreatedAt { get; set; }
    }
}
