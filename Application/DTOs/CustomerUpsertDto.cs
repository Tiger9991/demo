using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>بيانات إنشاء أو تعديل عميل</summary>
    public class CustomerUpsertDto
    {
        public Guid? Id { get; set; } // null = إنشاء جديد، قيمة = تعديل

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [MaxLength(100, ErrorMessage = "الاسم لا يتجاوز 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع العميل مطلوب")]
        public CustomerType CustomerType { get; set; } = CustomerType.Individual;

        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20, ErrorMessage = "رقم الهاتف لا يتجاوز 20 خانة")]
        public string? Phone { get; set; }

        [MaxLength(250, ErrorMessage = "العنوان لا يتجاوز 250 حرف")]
        public string? Address { get; set; }
    }
}
