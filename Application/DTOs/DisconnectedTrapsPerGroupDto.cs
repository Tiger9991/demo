using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class DisconnectedTrapsPerGroupDto
    {
        public string GroupNumber { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<string> TrapNumbers { get; set; } = new();
    }
}
