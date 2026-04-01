using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Core.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Preparing,
        Ready,
        Completed,
        Cancelled
    }
}
