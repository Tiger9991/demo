using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapUpsertDto
    {
        public string TrapNumber { get; set; } = string.Empty;
        public string? TrapGroup { get; set; }
        public float? SignalStrength { get; set; }
        
       
    }
}
