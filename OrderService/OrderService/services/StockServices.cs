

using StockService.Bus;
using StockService.Eventos;
using System.Threading.Tasks;

namespace OrderService.services
{

   public class StockServices
    {
        private readonly IProductRepository _productRepository;
        private readonly EventBus _eventBus;

        public StockServices(IProductRepository productRepository, EventBus eventBus)
        {
            _productRepository = productRepository;
            _eventBus = eventBus;
        }

        public async Task<int?> GetProductStockAsync(int productId)
        {
            var product = await _productRepository.GetAsync(productId);
            return product?.Stock;
        }

        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            var product = await _productRepository.GetAsync(orderCreatedEvent.ProductId);

            if (product != null && product.Stock >= orderCreatedEvent.Quantity)
            {
                product.Stock -= orderCreatedEvent.Quantity;
                await _productRepository.UpdateAsync(product);

                _eventBus.Publish(new StockUpdatedEvent { OrderId = orderCreatedEvent.OrderId }, "stock_exchange", "stock.updated");
            }
            else
            {
                _eventBus.Publish(new StockUpdateFailedEvent { OrderId = orderCreatedEvent.OrderId }, "stock_exchange", "stock.update_failed");
            }
        }

        public async Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent)
        {
            var product = await _productRepository.GetAsync(orderCancelledEvent.ProductId);

            if (product != null)
            {
                product.Stock += orderCancelledEvent.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }
    }

}
