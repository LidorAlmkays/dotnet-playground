using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public record RefreshTokenModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AccessToken { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}