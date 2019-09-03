﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ConferenceDTO
{
    [Serializable]
    public class Track
    {
        public int ID { get; set; }

        [Required]
        public int ConferenceId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
    }
}
