using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public record ResponseGetUserRoleDTO
    {
        public Role UserRole { get; set; }
    }
}