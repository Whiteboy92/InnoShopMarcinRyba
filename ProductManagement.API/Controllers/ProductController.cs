using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products.Queries;
using ProductManagement.Application.Interfaces;
using Shared.Logging;

namespace ProductManagement.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;
    private readonly ILoggerService<ProductController> logger;
    private readonly IGetProductsQueryHandler getProductsQueryHandler;

    public ProductController(
        IProductService productService,
        ILoggerService<ProductController> logger,
        IGetProductsQueryHandler getProductsQueryHandler)
    {
        this.productService = productService ?? throw new ArgumentNullException(nameof(productService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.getProductsQueryHandler = getProductsQueryHandler ?? throw new ArgumentNullException(nameof(getProductsQueryHandler)); 
    }

    // Get all products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await productService.GetAllAsync();
        return Ok(products);
    }

    // Get product by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Product Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = $"Product with ID {id} was not found.",
            });
        }

        return Ok(product);
    }

    // Create a new product
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "Provided data is not valid.",
            });
        }

        try
        {
            var product = await productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating product", ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An error occurred while creating the product.",
            });
        }
    }

    // Delete a product
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await productService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Product Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = $"Product with ID {id} was not found.",
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("Error deleting product", ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An error occurred while deleting the product.",
            });
        }
    }
    
    // filter products
    [HttpGet("search")]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQuery query)
    {
        try
        {
            var products = await getProductsQueryHandler.HandleAsync(query);
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching products with query", ex);
            return StatusCode((int)HttpStatusCode.InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An error occurred while fetching products with query.",
            });
        }
    }
}
