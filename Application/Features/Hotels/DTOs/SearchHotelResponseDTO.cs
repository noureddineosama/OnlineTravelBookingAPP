namespace Application.Features.Hotels.DTOs
{
    public class SearchHotelResponseDTO
    {
        public long hote_id { get; set; }

        public string hotel_name { get; set; }

        public string Slug { get; set; }

        public string City { get; set; }

        public byte? StarRating { get; set; }

        public decimal LowestPrice { get; set; }

        public string MainImage { get; set; }

        public bool Available { get; set; }
    }
}