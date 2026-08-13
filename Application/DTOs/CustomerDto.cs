using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }

        /// <summary>رقم العميل التلقائي — مثال: CUS-2026-0001</summary>
        public string CustomerNumber { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public CustomerType CustomerType { get; set; }

        /// <summary>نص نوع العميل بالعربية</summary>
        public string CustomerTypeDisplay => CustomerType == CustomerType.Individual ? "فرد" : "شركة";

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        /// <summary>عدد مجموعات المحطات المرتبطة بهذا العميل</summary>
        public int TrapGroupCount { get; set; }

        /// <summary>قائمة أرقام المجموعات المرتبطة</summary>
        public List<string> TrapGroupNumbers { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
