using Backend.Core.Models;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]


    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public OrdersController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet("getorders", Name = "GetAllOrders")]
        public ActionResult<List<OrderModel>> GetAllOrders(OrderStatus? stato)
        {
            IQueryable<OrderModel> query = _appDbContext.Orders
        .Include(o => o.OrderItems)
        .ThenInclude(i => i.Product);

            if (stato != null)
                query = query.Where(o => o.Status == stato.Value);

            var orders = query.ToList();

            return Ok(orders);
        }

        [HttpGet("{id}/getbyid", Name = "GetOrderByID")]
        public ActionResult<OrderModel> GetOrder(int id)
        {
            var order = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);

            if(order == null)
                return NotFound();
            return Ok(order);
        }

        [HttpPost("createorder", Name = "CreateOrder")]
        public ActionResult<OrderModel> CreateOrder(OrderDTO orderDto)
        {
            if (orderDto == null || orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                return BadRequest("L'ordine deve contenere almeno un prodotto.");

            var orderItems = new List<OrderItemModel>();

            decimal totalPrice = 0;
            var productIds = orderDto.OrderItems.Select(q => q.ProductId).ToList();

            var listaProduct = _appDbContext.Products.Where(q => productIds.Contains(q.Id)).ToList();

            var productDict = listaProduct.ToDictionary(p => p.Id);

            foreach (var item in orderDto.OrderItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product))
                    return BadRequest($"Il prodotto con questo id {item.ProductId} non esiste.");
                else
                    if (!product.IsAvailable)
                        return BadRequest($"Il prodotto con questo id {item.ProductId} non è disponibile");
                if (item.Quantity <= 0)
                    return BadRequest("La quantita deve essere maggiore di 0");
                if (orderItems.Any(q => q.ProductId == item.ProductId))
                    return BadRequest($"Il prodotto con questo id {item.ProductId} è gia presente nell'ordine");

                var unitPrice = product.Price;
                totalPrice += item.Quantity * unitPrice;

                orderItems.Add(new OrderItemModel
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                });
            }

            var order = new OrderModel
            {
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalPrice = totalPrice,
                TableNumber = orderDto.TableNumber,
                OrderItems = orderItems
            };
            _appDbContext.Orders.Add(order);

            _appDbContext.SaveChanges();
            return CreatedAtRoute("GetOrderByID", new { id = order.Id }, order);
        }

        [HttpPatch("{id}/cancel", Name = "CancelOrder")]
        public ActionResult CancelOrder(int id)
        {
            var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
                return NotFound();

            if (existingOrder.Status == OrderStatus.Paid)
                return BadRequest("L'ordine è gia in cucina, per cambiarlo o cancellare chiedere in cucina");

            if (existingOrder.Status == OrderStatus.Cancelled)
                return Ok("Gia cancellato");

            existingOrder.Status = OrderStatus.Cancelled;
            existingOrder.CancelledAt = DateTime.UtcNow;
            _appDbContext.SaveChanges();

            return Ok();
        }

        [HttpPatch("{id}/pay", Name = "PayOrder")]
        public ActionResult PayOrder(int id)
        {
            var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
                return NotFound();
            if (existingOrder.Status == OrderStatus.Paid)
                return Ok("Gia Pagato");
            if (existingOrder.Status != OrderStatus.Pending)
                return BadRequest("L'ordine non è in stato pending, non può essere pagato");
            existingOrder.Status = OrderStatus.Paid;
            existingOrder.PaidAt = DateTime.UtcNow;
            _appDbContext.SaveChanges();
            return Ok();
        }

        [HttpPatch("{id}/Preparing", Name = "PreparingOrder")]
        public ActionResult PreparingOrder(int id)
        {
            var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
                return NotFound();
            if (existingOrder.Status == OrderStatus.Preparing)
                return Ok("Gia in preparazione");
            if (existingOrder.Status != OrderStatus.Paid)
                return BadRequest("L'ordine non è stato ancora pagato");
            existingOrder.Status = OrderStatus.Preparing;
            existingOrder.PreparingAt = DateTime.UtcNow;
            _appDbContext.SaveChanges();
            return Ok();
        }

        [HttpPatch("{id}/Ready", Name = "ReadyOrder")]
        public ActionResult ReadyOrder(int id)
        {
            var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
                return NotFound();
            if (existingOrder.Status == OrderStatus.Ready)
                return Ok("Gia pronto");
            if (existingOrder.Status != OrderStatus.Preparing)
                return BadRequest("L'ordine non è ancora in preparazione");
            existingOrder.Status = OrderStatus.Ready;
            existingOrder.ReadyAt = DateTime.UtcNow;
            _appDbContext.SaveChanges();
            return Ok();
        }

        [HttpPatch("{id}/Completed", Name = "CompleteOrder")]
        public ActionResult CompleteOrder(int id)
        {
            var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
                return NotFound();
            if (existingOrder.Status == OrderStatus.Completed)
                return Ok("Gia completato");
            if (existingOrder.Status != OrderStatus.Ready)
                return BadRequest("L'ordine non è ancora pronto");
            existingOrder.Status = OrderStatus.Completed;
            existingOrder.CompletedAt = DateTime.UtcNow;
            _appDbContext.SaveChanges();
            return Ok();
        }
    }
}
