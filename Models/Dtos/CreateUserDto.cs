using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class CreateUserDto
{

    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Name { get; set; }
    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Username { get; set; }
    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Password { get; set; }
    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Role { get; set; }

}
