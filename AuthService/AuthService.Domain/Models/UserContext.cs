using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Domain.Models
{
    public class UserContext
    {
        public string NameIdentifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string GivenName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}