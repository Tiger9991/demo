using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Traps.Commands
{
    public record CreateTrapCommand(
     string TrapNumber,
     float SignalStrength,
     string? TrapGroup = null
    
 ) : IRequest<Guid>;

}