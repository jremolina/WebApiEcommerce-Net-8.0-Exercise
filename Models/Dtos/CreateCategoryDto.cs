using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "Nombre no puede ser mayor a 50 caracteres")]
    [MinLength(3, ErrorMessage = "Nombre debe ser de minimo 3 caracteres")]
    public string Name { get; set; } = string.Empty;

}
