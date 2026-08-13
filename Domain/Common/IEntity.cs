using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Common
{
    public class IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
