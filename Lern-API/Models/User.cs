﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Lern_API.Models
{
    public class User : IModelBase
    {
        [ReadOnly(true), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ReadOnly(true)]
        public DateTime CreatedAt { get; set; }
        [ReadOnly(true)]
        public DateTime UpdatedAt { get; set; }
        [ReadOnly(true)]
        public Guid? ManagerId { get; set; }
        [ReadOnly(true)]
        public User Manager { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Firstname { get; set; }
        [Required, MinLength(3), MaxLength(100)]
        public string Lastname { get; set; }
        [Required, MinLength(3), MaxLength(50)]
        public string Nickname { get; set; }
        [Required, MaxLength(254)]
        public string Email { get; set; }
        [JsonIgnore, MinLength(8), MaxLength(100)]
        public string Password { get; set; }
        [ReadOnly(true), DefaultValue(0)]
        public int Tokens { get; set; } = 0;
        [ReadOnly(true), DefaultValue(5)]
        public int MaxSubjects { get; set; } = 5;
        [ReadOnly(true), DefaultValue(true)]
        public bool Active { get; set; } = true;
        [ReadOnly(true), DefaultValue(false)]
        public bool Admin { get; set; } = false;
        [ReadOnly(true), DefaultValue(false)]
        public bool VerifiedCreator { get; set; } = false;
    }
}
