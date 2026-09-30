using Microsoft.AspNetCore.Mvc;
using OrderPdf.Application.Interfaces;
using OrderPdf.Domain.Entities;

namespace OrderPdf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderPdfService _orderPdfService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        IOrderPdfService orderPdfService,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _orderPdfService = orderPdfService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all available orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Order>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves a specific order by ID or Order Number.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetOrderById(string id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
                    ?? await _orderRepository.GetByOrderNumberAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order with ID or OrderNumber '{OrderId}' was not found.", id);
            return NotFound(new { message = $"Order '{id}' not found." });
        }

        return Ok(order);
    }

    /// <summary>
    /// Generates and downloads a professional Order Confirmation PDF for the specified order.
    /// </summary>
    [HttpGet("{id}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadOrderPdf(string id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
                    ?? await _orderRepository.GetByOrderNumberAsync(id, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("PDF generation requested for non-existent order '{OrderId}'.", id);
            return NotFound(new { message = $"Order '{id}' not found." });
        }

        try
        {
            var pdfBytes = await _orderPdfService.GenerateAsync(order, cancellationToken);
            var fileName = $"Order-{order.OrderNumber}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for order {OrderNumber}", order.OrderNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An error occurred while generating the Order Confirmation PDF.",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Adds a new order to the system.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order, CancellationToken cancellationToken)
    {
        if (order == null || string.IsNullOrWhiteSpace(order.OrderNumber))
        {
            return BadRequest(new { message = "Order payload is invalid." });
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }
}
