namespace StockService.services
{
    using global::StockService.Bus;
    using System.Threading.Tasks;

    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly EventBus _eventBus;

        public OrderService(IOrderRepository orderRepository, EventBus eventBus)
        {
            _orderRepository = orderRepository;
            _eventBus = eventBus;
        }

        public async Task<int> CreateOrderAsync(int productId, int quantity)
        {
            var order = new Models.Orders
            {
                ProductId = productId,
                Quantity = quantity,
                Status = "Pending"
            };

            await _orderRepository.AddAsync(order);

            _eventBus.Publish(new Eventos.OrderCreatedEvent { OrderId = order.Id, ProductId = productId, Quantity = quantity }, "order_exchange", "order.created");

            return order.Id;
        }

        public async Task HandleStockUpdatedEvent(Eventos.StockUpdatedEvent stockUpdatedEvent)
        {
            var order = await _orderRepository.GetAsync(stockUpdatedEvent.OrderId);
            if (order != null)
            {
                order.Status = "Confirmed";
                await _orderRepository.UpdateAsync(order);
            }
        }

        public async Task HandleStockUpdateFailedEvent(Eventos.StockUpdateFailedEvent stockUpdateFailedEvent)
        {
            var order = await _orderRepository.GetAsync(stockUpdateFailedEvent.OrderId);
            if (order != null)
            {
                order.Status = "Cancelled";
                await _orderRepository.UpdateAsync(order);
            }
        }
    }

}
