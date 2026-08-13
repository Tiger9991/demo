using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class CaptureEventDto
    {
       
        public string TrapNumber { get; set; } = string.Empty;
        public string trapGroup { get; set; } = string.Empty;
        public DateTime CaptureTime { get; set; }
        public int ir { get; set; }
        public double weight { get; set; }
        public double bWeight { get; set; }
        
        public float SignalStrength { get; set; }
        public string SignalQuality { get; set; } = string.Empty;
        public int NumberOfTransmissions { get; set; }
      

    }
}


