using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public class AuthMethodModel
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public UserModel User { get; set; }
        [Required]
        public AuthProvider Provider { get; set; }
        public string? ProviderUserId { get; set; }
        public string? Password { get; set; }
        public string? PasswordKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}