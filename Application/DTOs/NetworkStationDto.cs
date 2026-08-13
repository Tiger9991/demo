using System;

namespace Application.DTOs
{
    public class NetworkStationDto
    {
        public Guid Id { get; set; }
        public string TrapGroup { get; set; } = string.Empty;
        public string TrapNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNumber { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public int? BatteryPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
