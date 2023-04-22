using Microsoft.AspNetCore.Mvc;
using OrderService.Models;

namespace OrderService.Eventos
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
