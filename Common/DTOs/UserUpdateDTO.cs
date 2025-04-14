using Common.Enums;
namespace Common.DTOs
{
    public class UpdateUserAuthenticationDTO
    {

        public string? Email { get; set; }
        public string? Password { get; set; }
        public Role? Role { get; set; }
    }
}