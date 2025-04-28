namespace Common.DTOs
{
    public record TokenIssuingResponseDTO
    {
        public string AccessToken { get; set; }
        public string RefreshTokenId { get; set; }
    }
}