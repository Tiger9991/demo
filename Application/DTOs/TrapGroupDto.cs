using System;

namespace Application.DTOs
{
    public class TrapGroupDto
    {
        public Guid Id { get; set; }
        public string TrapNumber { get; set; } = string.Empty;
        public string TrapGroup { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
