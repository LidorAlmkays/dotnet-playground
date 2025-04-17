using Common.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Domain.Models
{
    public record UserModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public required string Email { get; set; }

        public string? Name { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public HashSet<AuthMethodModel> AuthMethods { get; set; } = [];
#pragma warning restore CA2227 // Collection properties should be read only

    }
}