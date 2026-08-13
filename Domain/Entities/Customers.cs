using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Customers : AuditableEntity<Guid>
    {
        public Customers()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>رقم العميل التلقائي — مثال: CUS-2026-0001</summary>
        [Required]
        [MaxLength(20)]
        public string CustomerNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public CustomerType CustomerType { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Inactive

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        // Navigation — عميل واحد يمكنه امتلاك عدة مجموعات محطات
        public ICollection<TrapGroups> TrapGroups { get; set; } = new List<TrapGroups>();
    }
}
