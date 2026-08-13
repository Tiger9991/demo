using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Customers.Queries
{
    /// <summary>جلب محطات (Traps) المرتبطة بعميل محدد عبر مجموعاته</summary>
    public record GetCustomerTrapsQuery(Guid CustomerId) : IRequest<List<TrapDto>>;
}
