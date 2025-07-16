using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticateController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized("Usuario o contraseña incorrectos.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Usuario o contraseña incorrectos.");

        var token = await _jwtTokenService.GenerateTokenAsync(user);
        return Ok(new { token });
    }

   
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        
        if (string.IsNullOrWhiteSpace(model.Username))
            return BadRequest("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(model.Email) || !Regex.IsMatch(model.Email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$", RegexOptions.IgnoreCase))
            return BadRequest("El correo debe ser una cuenta válida de Gmail.");

        var validRoles = new[] { "Admin", "Customer" };
        if (string.IsNullOrWhiteSpace(model.Role) || !validRoles.Contains(model.Role))
            return BadRequest("Rol inválido. Los roles válidos son: Admin o Customer.");

        
        var user = new IdentityUser { UserName = model.Username.Trim(), Email = model.Email.Trim() };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

       
        await _userManager.AddToRoleAsync(user, model.Role);

        return Ok("Usuario registrado correctamente.");
    }
}
