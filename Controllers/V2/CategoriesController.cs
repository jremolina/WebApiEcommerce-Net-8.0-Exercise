using ApiEcommerce.Constants;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers.V2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    // [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]// exige autorizacion
                                //[EnableCors(PolicyNames.AllowSpecificOrigin)]

    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        // [MapToApiVersion("2.0")]
        public IActionResult GetCategoriesOrderbyId()
        {
            var categories = _categoryRepository.GetCategories().OrderBy(cat => cat.Id);
            var categoriesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categoriesDto);
        }


        [HttpGet("{id:int}", Name = "GetCategory")]
        // [ResponseCache(Duration = 10)]
        //[ResponseCache(CacheProfileName = "Default10")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetCategory(int id)
        {
            var category = _categoryRepository.GetCategory(id);
            if (category == null)
            {
                return NotFound($"La Categoria con el id {id} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCategory([FromBody] CreateCategoryDto createcategoryDto)
        {
            if (createcategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExists(createcategoryDto.Name))
            {
                ModelState.AddModelError("Custom Error", $"La categoria  {createcategoryDto.Name}  ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(createcategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"No se pudo crear la categoria {category.Name} ");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
        }

        [HttpPatch("{id:int}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La Categoria con el id {id} no existe");
            }

            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("Custom Error", $"La categoria  {updateCategoryDto.Name}  ya existe");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id;
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"No se pudo actualizar la categoria {category.Name} ");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteCategory(int id)
        {
            if (!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La Categoria con el id {id} no existe");
            }
            var category = _categoryRepository.GetCategory(id);
            if (category == null)
            {
                return NotFound($"La Categoria con el id {id} no existe");
            }
            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("Custom Error", $"No se pudo eliminiar la categoria {category.Name} ");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
