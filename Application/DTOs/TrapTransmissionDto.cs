using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapTransmissionDto
    {
        public Guid TrapId { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public int NumberOfTransmissions { get; set; }   // عدد مرات الارسال
        public int OperatingDays { get; set; }           // optional, but useful for battery calculation
    }

}
