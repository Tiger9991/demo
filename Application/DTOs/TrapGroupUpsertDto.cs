using System.ComponentModel.DataAnnotations;
using System;

namespace Application.DTOs
{
    /// <summary>بيانات إنشاء أو تعديل مجموعة محطات</summary>
    public class TrapGroupUpsertDto
    {
        public Guid? Id { get; set; } // null = إنشاء جديد، قيمة = تعديل

        [Required(ErrorMessage = "رقم المحطة مطلوب")]
        [MaxLength(50)]
        public string TrapNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم المجموعة مطلوب")]
        public string TrapGroup { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public Guid? CustomerId { get; set; }
    }
}
