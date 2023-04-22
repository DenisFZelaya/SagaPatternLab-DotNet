
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

        public int AddAsync(Orders order)
        {
            order.Id = 3;
            _eventBus.Publish(new OrderCreatedEvent { OrderId = order.Id, ProductId = order.ProductId, Quantity = order.Quantity }, "order_exchange", "order.created");

             // return  1;
            return order.Id;
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

        public async Task HandleStockUpdatedEvent(StockUpdatedEvent stockUpdatedEvent)
        {
            var order = await _context.Orders.Where(_ => _.Equals(stockUpdatedEvent.OrderId)).FirstOrDefaultAsync();
            if (order != null)
            {
                order.Status = "Confirmed";
                _context.Orders.Update(order);
            }
        }

        public async Task HandleStockUpdateFailedEvent(StockUpdateFailedEvent stockUpdateFailedEvent)
        {
            var order = await _context.Orders.Where(_ => _.Equals(stockUpdateFailedEvent.OrderId)).FirstOrDefaultAsync();
            if (order != null)
            {
                order.Status = "Cancelled";
                _context.Orders.Update(order);
            }
        }

        public Task UpdateAsync(Orders order)
        {
            throw new NotImplementedException();
        }
    }


}
