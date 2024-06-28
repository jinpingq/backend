using MarketplaceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web.Resource;
using Microsoft.VisualBasic;

namespace MarketplaceAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ListingProductsController : ControllerBase
{
    private readonly ILogger<ListingProductsController> _logger;

    private readonly MarketplaceContext _context;

    public ListingProductsController(ILogger<ListingProductsController> logger, MarketplaceContext context, IMemoryCache memoryCache)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListingProduct>>> GetListingProducts(int limit = 0, int offset = 0, string? name = null, string? seller = null)
    {
        _logger.LogInformation("test log");

        var t = _context.ListingProducts.Include(p => p.Seller).AsQueryable();
        if (name != null)
        {
            t = t.Where(p => p.Name == name);
        }

        if (seller != null)
        {
            t = t.Where(p => p.Seller.DisplayName == seller);
        }

        return await t.OrderBy(p => p.Id).Skip(offset).Take(limit).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ListingProduct>> ListProduct(ListingProduct listingProduct, int userInventoryProductId)
    {
        var userId = HttpContext.User.Identity?.Name;
        ProductInventory product = await _context.ProductInventory.FirstAsync(p => p.Id == userInventoryProductId);
        if (product.UserId != userId)
        {
            throw new UnauthorizedAccessException();
        }

        _context.ProductInventory.Remove(product);
        _context.ListingProducts.Add(listingProduct);
        await _context.SaveChangesAsync();
        return CreatedAtAction(
            nameof(ListProduct),
            new {id = listingProduct.Id},
            listingProduct);
    }
}

