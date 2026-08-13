using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Common.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Trap, TrapDto>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.status == "Active"))
                .ForMember(d => d.IndicatorStatus, opt => opt.MapFrom(s => Trap.CalculateIndicatorStatus(s.LastEntryDate)))
                .ForMember(d => d.OperatingDays, opt => opt.MapFrom(s => Math.Max(0, (int)(DateTime.UtcNow - s.StartTime).TotalDays)))
                .ReverseMap();
            CreateMap<CaptureEvent, CaptureEventDto>().ReverseMap();
            CreateMap<BaitMeasurement, BaitMeasurementDto>().ReverseMap();
        }
    }
}
