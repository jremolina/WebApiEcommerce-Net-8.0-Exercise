using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, Mapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        public IActionResult GetCategory(int id)
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
            return CreatedAtRoute("GetProduct", new { id = product.ProductId }, product);
        }
    }
}
