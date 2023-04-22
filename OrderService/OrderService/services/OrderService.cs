
using Microsoft.EntityFrameworkCore;
using OrderService.Bus;
using OrderService.Eventos;
using OrderService.Models;
using System.Threading.Tasks;

namespace OrderService.services
{


    public class OrderServices : IOrderRepository
    {
   
        private readonly EventBus _eventBus;
        private readonly SagaPatternLabContext _context;

        public OrderServices(SagaPatternLabContext context, EventBus eventBus)
        {
            _context = context; 
            _eventBus = eventBus;
        }

        public async Task<int> CreateOrderAsync(int productId, int quantity)
        {
            var order = new Orders
            {
                ProductId = productId,
                Quantity = quantity,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            int resultado = await _context.SaveChangesAsync();

            if(resultado > 0)
            {
                _eventBus.Publish(new OrderCreatedEvent { OrderId = order.Id, ProductId = productId, Quantity = quantity }, "order_exchange", "order.created");
            }

            return resultado;
        }

        public Task<Orders> GetAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Orders order)
        {
            throw new NotImplementedException();
        }

        // Eventos Suscritos

        public async Task HandleStockUpdatedEvent(StockUpdatedEvent stockUpdatedEvent)
        {
            Console.WriteLine("HANDLE_STOCK_UPDATED_EVENT");
            Console.WriteLine("OrderId: ", 44);
            using (var db = new SagaPatternLabContext())
            {
                var order = await db.Orders.Where(_ => _.Id.Equals(stockUpdatedEvent.OrderId)).FirstOrDefaultAsync();
                if (order != null)
                {
                    order.Status = "Confirmed";

                    db.Entry(order).State = EntityState.Modified;

                    int resultado = await db.SaveChangesAsync();

                    if(resultado > 0)
                    {
                        Console.WriteLine("PEDIDO CONFIRMADO");
                    }
                }
            }
               
        }

        public async Task HandleStockUpdateFailedEvent(StockUpdateFailedEvent stockUpdateFailedEvent)
        {
            using (var db = new SagaPatternLabContext())
            {
                var order = await db.Orders.Where(_ => _.Id.Equals(stockUpdateFailedEvent.OrderId)).FirstOrDefaultAsync();
                if (order != null)
                {
                    order.Status = "Cancelled";
      
                    db.Entry(order).State = EntityState.Modified;

                    int resultado = await db.SaveChangesAsync();

                    if (resultado > 0)
                    {
                        Console.WriteLine("PEDIDO CANCELADO");
                    }
                }

            }
               
        }


    }


}
