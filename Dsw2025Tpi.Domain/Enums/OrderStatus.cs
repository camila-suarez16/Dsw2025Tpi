using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Enums
{
    public enum OrderStatus
    {
        PENDING, //creada
        PROCESSING, //en proceso
        SHIPPED, //enviada
        DELIVERED, // entregada al cliente
        CANCELLED //cancelada
    }
}
