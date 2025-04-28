using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public record TokenIssuingModel
    {
        public RefreshTokenModel RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}