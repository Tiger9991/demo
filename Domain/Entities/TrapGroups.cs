using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TrapGroups : AuditableEntity<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string TrapNumber { get; set; } = string.Empty;
        public string TrapGroup { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        // FK → Customer (nullable — مجموعة قد لا تكون مرتبطة بعميل)
        public Guid? CustomerId { get; set; }
        public Customers? Customer { get; set; }
    }
}
