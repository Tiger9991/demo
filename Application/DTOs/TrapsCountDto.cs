using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class TrapsCountDto
    {
        public int TotalTraps { get; set; }
        public int ActiveTraps { get; set; }
        public int InactiveTraps { get; set; }
    }
}
