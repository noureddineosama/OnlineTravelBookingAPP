using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Rooms.DTOs
{
    public class CreateRoomExtraRequestDTO
    {
        [Required(ErrorMessage = "Name field is requeired. ")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Price field is required. "), Range(1, int.MaxValue)]
        public decimal Price { get; set; }
    }
}
