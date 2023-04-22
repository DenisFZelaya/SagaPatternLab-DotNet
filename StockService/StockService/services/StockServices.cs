using Microsoft.EntityFrameworkCore;
using StockService.Bus;
using StockService.Eventos;
using StockService.Models;
using System.Threading.Tasks;
using static NuGet.Packaging.PackagingConstants;

namespace StockService.services
{

    public class StockServices : IOrderRepository
    {
        
        private readonly EventBus _eventBus;
        private readonly SagaPatternLabContext _context;

        public StockServices(SagaPatternLabContext context, EventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public async Task<int?> GetProductStockAsync(int productId)
        {
            var product = await _context.Products.Where(_ => _.Id.Equals(productId)).FirstOrDefaultAsync();
            return product?.Stock;
        }

        // Cuando se crea una orden en el ms de orders envia un evento que dispara este handler
        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
         
            try
            {
                Console.WriteLine("HANDLE_ORDER_CREATED_EVENT");
                Console.WriteLine("ProductId: " + orderCreatedEvent.ProductId);
                Console.WriteLine("Quantity: " + orderCreatedEvent.Quantity);
                Console.WriteLine("OrderId: " + orderCreatedEvent.OrderId);

                using(var db = new SagaPatternLabContext())
                {
                    var product = await db.Products.Where(_ => _.Id.Equals(orderCreatedEvent.ProductId)).FirstOrDefaultAsync();
                   Console.WriteLine("Nombre producto: " + product?.Name);
                    if (product != null && product.Stock >= orderCreatedEvent.Quantity)
                    {
                        product.Stock -= orderCreatedEvent.Quantity;

                        db.Entry(product).State = EntityState.Modified;

                        int resultado = await db.SaveChangesAsync();

                        if (resultado > 0)
                        {
                            _eventBus.Publish(new StockUpdatedEvent { OrderId = orderCreatedEvent.OrderId }, "stock_exchange", "stock.updated");
                        } else
                        {
                            _eventBus.Publish(new StockUpdateFailedEvent { OrderId = orderCreatedEvent.OrderId }, "stock_exchange", "stock.update_failed");
                        }
                    } 
                    else
                    {
                        //
                        _eventBus.Publish(new StockUpdateFailedEvent { OrderId = orderCreatedEvent.OrderId }, "stock_exchange", "stock.update_failed");
                    }
                }

             
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent)
        {
            var product = await _context.Products.Where(_ => _.Id.Equals(orderCancelledEvent.ProductId)).FirstOrDefaultAsync();

            if (product != null)
            {
                product.Stock += orderCancelledEvent.Quantity;
                 _context.Update(product);
                _context.SaveChangesAsync();
            }
        }

        public Task<Orders> GetAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Orders order)
        {
            
            Console.WriteLine(order.Id);

            throw new NotImplementedException();
        }

        public Task UpdateAsync(Orders order)
        {
            throw new NotImplementedException();
        }
    }

}
