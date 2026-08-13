using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class TrapAssignmentHistoryDto
    {
        public Guid Id { get; set; }
        public Guid TrapId { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime AssignedFromDate { get; set; }
        public DateTime? AssignedToDate { get; set; }
        public string? Notes { get; set; }
    }
}
