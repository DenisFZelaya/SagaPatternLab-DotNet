﻿namespace StockService.Eventos
{
    public class OrderCancelledEvent
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
