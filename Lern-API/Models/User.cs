using System;
using System.Diagnostics.CodeAnalysis;
using PetaPoco;

namespace Lern_API.Models
{
    [ExcludeFromCodeCoverage]
    public class User
    {
        public int Id { get; set; }
        [ResultColumn(IncludeInAutoSelect.Yes)]
        public DateTime CreatedOn { get; set; }
        public string Name { get; set; }
    }
}
