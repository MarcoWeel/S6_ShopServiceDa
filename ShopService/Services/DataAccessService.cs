using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ShopService.Models;
using ShopServiceDA.Data;
using ShopServiceDA.Services.Interfaces;

namespace ShopServiceDA.dataaccess.Services;

public interface IDataAccessService
{
    void SubscribeToPersistence();
}

public class DataAccessService : IDataAccessService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessagingService _messagingService;

    public DataAccessService(IServiceProvider serviceProvider, IMessagingService messagingService)
    {
        _serviceProvider = serviceProvider;
        _messagingService = messagingService;

    }

    public void SubscribeToPersistence()
    {
        _messagingService.Subscribe("order-data",
            (BasicDeliverEventArgs ea, string queue, string request) => RouteCallback(ea, queue, request),
            ExchangeType.Topic, "*.*.request");
        _messagingService.Subscribe("product-data",
            (BasicDeliverEventArgs ea, string queue, string request) => RouteCallback(ea, queue, request),
            ExchangeType.Topic, "*.*.request");
        _messagingService.Subscribe("material-data",
            (BasicDeliverEventArgs ea, string queue, string request) => RouteCallback(ea, queue, request),
            ExchangeType.Topic, "*.*.request");
        _messagingService.Subscribe("gdprExchange",
            (BasicDeliverEventArgs ea, string queue, string request) => RouteCallback(ea, queue, request),
            ExchangeType.Topic, "*");
    }

    private async void RouteCallback(BasicDeliverEventArgs ea, string queue, string request)
    {
        using ShopServiceContext context =
            _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ShopServiceContext>();

        string route = ea.RoutingKey.Replace("request", "response");

        string data = Encoding.UTF8.GetString(ea.Body.ToArray());
        string exchange = ea.Exchange;

        switch (request)
        {
            case "getAllMaterials":
                {
                    var materials = await context.Material.ToListAsync();
                    var json = JsonConvert.SerializeObject(materials);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "getMaterialById":
                {
                    Guid id = Guid.Parse(data);
                    var material = await context.Material.SingleOrDefaultAsync(m => m.Id == id);
                    var json = JsonConvert.SerializeObject(material);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "addMaterial":
                {
                    var material = JsonConvert.DeserializeObject<Material>(data);
                    if (material == null)
                        break;

                    context.Add(material);
                    await context.SaveChangesAsync();

                    var newMaterial =
                        await context.Material.SingleOrDefaultAsync(m => m.Name == material.Name || m.Id == material.Id);
                    if (newMaterial == null)
                        break;
                    var json = JsonConvert.SerializeObject(newMaterial);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "deleteMaterial":
                {
                    Guid id = Guid.Parse(data);

                    var material = await context.Material.SingleOrDefaultAsync(m => m.Id == id);
                    if (material == null)
                        return;

                    context.Material.Remove(material);
                    await context.SaveChangesAsync();
                    var json = JsonConvert.SerializeObject(material);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "updateMaterial":
                {
                    var updatedMaterial = JsonConvert.DeserializeObject<Material>(data);
                    if (updatedMaterial == null)
                        break;

                    var oldMaterial = await context.Material.SingleOrDefaultAsync(m => m.Id == updatedMaterial.Id);
                    if (oldMaterial == null)
                        break;

                    oldMaterial.Name = updatedMaterial.Name;
                    oldMaterial.Price = updatedMaterial.Price;
                    oldMaterial.StockAmount = updatedMaterial.StockAmount;

                    await context.SaveChangesAsync();

                    var json = JsonConvert.SerializeObject(updatedMaterial);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "deleteProduct":
                {
                    Guid id = Guid.Parse(data);

                    var product = await context.Product.SingleOrDefaultAsync(m => m.Id == id);
                    if (product == null)
                        return;

                    context.Product.Remove(product);
                    await context.SaveChangesAsync();
                    var json = JsonConvert.SerializeObject(product);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "updateProduct":
                {
                    var updatedProduct = JsonConvert.DeserializeObject<Product>(data);
                    if (updatedProduct == null)
                        break;

                    var oldProduct = await context.Product.SingleOrDefaultAsync(m => m.Id == updatedProduct.Id);
                    if (oldProduct == null)
                        break;

                    oldProduct.Name = updatedProduct.Name;
                    oldProduct.MaterialId = updatedProduct.MaterialId;
                    oldProduct.Description = updatedProduct.Description;
                    oldProduct.StockAmount = updatedProduct.StockAmount;

                    await context.SaveChangesAsync();

                    var json = JsonConvert.SerializeObject(updatedProduct);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }

            case "addProduct":
                {
                    var product = JsonConvert.DeserializeObject<Product>(data);
                    if (product == null)
                        break;

                    context.Add(product);
                    await context.SaveChangesAsync();

                    var newProduct = await context.Product.SingleOrDefaultAsync(m => m.Id == product.Id);
                    if (newProduct == null)
                        break;
                    var json = JsonConvert.SerializeObject(newProduct);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
                ;
            case "getAllProducts":
                {
                    var products = await context.Product.ToListAsync();
                    var json = JsonConvert.SerializeObject(products);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);
                    break;
                }
            case "getProductById":
                {
                    Guid id = Guid.Parse(data);
                    var product = await context.Product.SingleOrDefaultAsync(m => m.Id == id);
                    var json = JsonConvert.SerializeObject(product);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);
                    break;
                }
            case "deleteOrder":
            {
                Guid id = Guid.Parse(data);
                Order order = await context.Order.Include(x=>x.Products).SingleOrDefaultAsync(m => m.Id == id);
                if (order == null)
                    return;
                context.Order.Remove(order);
                await context.SaveChangesAsync();
                break;
            };
            case "updateOrder":
                {
                    var updatedOrder = JsonConvert.DeserializeObject<Order>(data);
                    if (updatedOrder == null)
                        break;

                    var oldOrder = await context.Order.SingleOrDefaultAsync(m => m.Id == updatedOrder.Id);
                    if (oldOrder == null)
                        break;

                    oldOrder.TotalPrice = updatedOrder.TotalPrice;
                    oldOrder.Products = updatedOrder.Products;
                    oldOrder.UserGuid = updatedOrder.UserGuid;

                    await context.SaveChangesAsync();

                    var json = JsonConvert.SerializeObject(updatedOrder);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }

            case "addOrder":
                {
                    var order = JsonConvert.DeserializeObject<Order>(data);
                    if (order == null)
                        break;

                    context.Add(order);
                    await context.SaveChangesAsync();

                    var newOrder = await context.Order.SingleOrDefaultAsync(m => m.Id == order.Id);
                    if (newOrder == null)
                        break;
                    var json = JsonConvert.SerializeObject(newOrder);
                    byte[] message = Encoding.UTF8.GetBytes(json);
                    _messagingService.Publish(exchange, queue, route, request, message);

                    break;
                }
            case "getAllOrders":
                {
                    var orders = await context.Order.ToListAsync();
                    var json = JsonConvert.SerializeObject(orders);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);
                    break;
                }
            case "getOrderById":
                {
                    Guid id = Guid.Parse(data);
                    var order = await context.Order.SingleOrDefaultAsync(m => m.Id == id);
                    var json = JsonConvert.SerializeObject(order);
                    byte[] message = Encoding.UTF8.GetBytes(json);

                    _messagingService.Publish(exchange, queue, route, request, message);
                    break;
                }
            case "gdprDelete":
                {
                    var orders = await context.Order.Where(m => m.UserGuid == Guid.Parse(data)).ToListAsync();
                    foreach (var order in orders)
                    {
                        context.Order.Remove(order);
                    }
                    await context.SaveChangesAsync();
                    break;
                }
            default:
                Console.WriteLine($"Request {request} Not Found");
                break;

        }
    }
}

