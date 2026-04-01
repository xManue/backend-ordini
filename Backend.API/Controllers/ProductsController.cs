using Backend.Core.Models;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly AppDbContext _appDbContext;

        public ProductsController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet("GetProducts", Name = "GetAllProducts")]
        public ActionResult<List<ProductModel>> GetAllProducts(bool? disponibile)
        {
            IQueryable<ProductModel> products = _appDbContext.Products;

            if (disponibile != null)
                products = products.Where(p => p.IsAvailable == disponibile.Value);

            var result = products.ToList();

            return Ok(result);
        }

        [HttpGet("GetProductsByCategory", Name = "GetProductsByCategory")]
        public ActionResult<List<ProductModel>> GetProductsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("La categoria non può essere vuota");
            var products = _appDbContext.Products
                .Where(p => p.Category != null && p.Category.ToLower() == category.Trim().ToLower())
                .ToList();
            return Ok(products);
        }

        [HttpGet("GetSingleProduct", Name = "GetProductByID")]
        public ActionResult<ProductModel> GetProduct(int id)
        {
            var product = _appDbContext.Products.FirstOrDefault(o => o.Id == id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }


        [HttpPost("", Name = "InsertProduct")]
        public ActionResult<ProductModel> InsertProduct([FromBody] ProductDTO product)
        {
            if (product == null)
                return BadRequest("Il prodotto non può essere nullo");

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Inserisci il nome del prodotto");

            if (product.Price <= 0)
                return BadRequest("Il prezzo del prodotto non può essere minore di 0");

            var nome = product.Name.Trim().ToLowerInvariant();

            var exists = _appDbContext.Products
                .Any(p => p.Name.ToLower() == nome);

            if (exists)
                return BadRequest("Il nome del prodotto deve essere univoco");

            var nuovoProdotto = new ProductModel
            {
                Name = nome,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category.Trim(),
                IsAvailable = true
            };
            _appDbContext.Products.Add(nuovoProdotto);
            _appDbContext.SaveChanges();

            return CreatedAtRoute("GetProductByID", new { id = nuovoProdotto.Id }, nuovoProdotto);
        }

        [HttpPut("", Name = "UpdateProduct")]
        public ActionResult UpdateProduct(int id, [FromBody] ProductDTO product)
        {
            var existingProduct = _appDbContext.Products.FirstOrDefault(o => o.Id == id);
            if (existingProduct == null)
                return NotFound();

            if (product == null)
                return BadRequest("Il prodotto non può essere nullo");

            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Inserisci il nome del prodotto");

            if (product.Price < 0)
                return BadRequest("Il prezzo del prodotto non può essere minore di 0");

            existingProduct.Name = product.Name.Trim();
            existingProduct.Price = product.Price;
            if(!string.IsNullOrWhiteSpace(product.Category))
                existingProduct.Category = product.Category.Trim();

            existingProduct.Description = product.Description;

            _appDbContext.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteProduct")]
        public ActionResult DeleteProduct(int id)
        {
            var existingProduct = _appDbContext.Products.FirstOrDefault(o => o.Id == id);
            if (existingProduct == null)
                return NotFound();

            _appDbContext.Products.Remove(existingProduct);
            _appDbContext.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id}/availability", Name = "ToggleProductAvailability")]
        public ActionResult AvailabilityProduct(int id)
        {
            var existingProduct = _appDbContext.Products.FirstOrDefault(o => o.Id == id);
            if (existingProduct == null)
                return NotFound();
            existingProduct.IsAvailable = !existingProduct.IsAvailable;
            _appDbContext.SaveChanges();
            return Ok(existingProduct);
        }
    }
}
