using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Application.Features.Hotels.DTOs
{
    public class LocationHotelRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string country { get; set; }

        [Required]
        [StringLength(100)]
        public string city { get; set; }

        [StringLength(255)]
        public string address_line { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? latitude { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? longitude { get; set; }
    }
}
