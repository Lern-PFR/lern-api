using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lern_API.Models
{
    public class Purchase : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public Guid UserId { get; set; }
        [ReadOnly(true)]
        public User User { get; set; }
        [ReadOnly(true)]
        public DateTime Date { get; set; }
        [ReadOnly(true)]
        public string Description { get; set; }
        [ReadOnly(true)]
        public double Price { get; set; }
    }
}
