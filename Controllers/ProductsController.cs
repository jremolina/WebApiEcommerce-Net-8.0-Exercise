using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize(Roles = "Admin")]// exige autorizacion
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetProductos()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            //var productsDto = new List<ProductDto>();
            // foreach (var product in products)
            // {
            //     productsDto.Add(_mapper.Map<ProductDto>(product));
            // }
            return Ok(productsDto);
        }

        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]

        public IActionResult GetProductbyId(int id)
        {
            var product = _productRepository.GetProduct(id);
            if (product == null)
            {
                return NotFound($"El producto con el id {id} no existe");
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createproductDto)
        {
            if (createproductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExist(createproductDto.Name))
            {
                ModelState.AddModelError("Custom Error", $" El producto  {createproductDto.Name}  ya existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(createproductDto.CategoryId))
            {
                ModelState.AddModelError("Custom Error", $" La categoria  con Id {createproductDto.CategoryId}  no existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createproductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"No se pudo crear el producto {product.Name} ");
                return StatusCode(500, ModelState);
            }
            var createProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createProduct);
            return CreatedAtRoute("GetProduct", new { id = product.ProductId }, productDto);
        }


        [HttpGet("SearchProductByCategory/{categoryid:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryid)
        {
            var products = _productRepository.GetProductsForCategory(categoryid);
            if (products.Count == 0)
            {
                return NotFound($"Los productos con el Category ID :  {categoryid} no existen");
            }
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }


        [HttpGet("SearchProductByDescription/{searchTerm}", Name = "SearchProductsByName")]// cuando el paraametro es string, se omite especificar el tipo de dato
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProductsByName(string searchTerm)
        {
            var products = _productRepository.SearchProductsByName(searchTerm);
            if (products.Count == 0)
            {
                return NotFound($"Los productos con el nombre :  {searchTerm} no existen");
            }
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpPatch("BuyProduct/{name}/{quantity:int}", Name = "BuyProduct")]// cuando el paraametro es string, se omite especificar el tipo de dato
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || quantity <= 0)
            {
                return BadRequest("nombre o cantidad no validos");
            }
            var foundProduct = _productRepository.ProductExist(name);
            if (!foundProduct)
            {
                return NotFound($"El producto con el nomnre {name} no existe");
            }
            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"no se pudo comprar el producto {name} o la cantidad es mayor al stock disponible");
                return BadRequest(ModelState);
            }
            var units = quantity == 1 ? "Unidad" : "Unidades";
            return Ok($" Se realizo la compra de {quantity} {units} del producto '{name}' ");

        }
        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (!_productRepository.ProductExist(productId))
            {
                ModelState.AddModelError("Custom Error", $" El producto  {updateProductDto.Name}  no existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("Custom Error", $" La categoria  con Id {updateProductDto.CategoryId}  no existe");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"algo salio mal al actualizar el producto {product.Name} ");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }


        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteProduct(int id)
        {
            if (id == 0)
            {
                return BadRequest(ModelState);
            }

            var product = _productRepository.GetProduct(id);
            if (product == null)
            {
                ModelState.AddModelError("Custom Error", $" El producto  {id}  no existe");
                return BadRequest(ModelState);
            }
            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("Custom Error", $"algo salio mal al actualizar el producto {product.Name} ");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }


    }
}
