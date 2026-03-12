using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class UserLoginDto
{
    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Username { get; set; }
    [Required(ErrorMessage = "Campo Reqeuerido")] public string? Password { get; set; }

}
