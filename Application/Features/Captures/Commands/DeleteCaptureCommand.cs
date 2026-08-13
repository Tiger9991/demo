using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Captures.Commands
{
    public record DeleteCaptureCommand(Guid Id) : IRequest;
}
