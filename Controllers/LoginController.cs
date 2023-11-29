
using Microsoft.AspNetCore.Mvc;
using BankAPI.Data.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BankAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController: ControllerBase
{
	private readonly LoginService _loginService;
	private IConfiguration _config;
	public LoginController(LoginService loginService, IConfiguration config)
	{
		_loginService = loginService;
		_config = config;
	}

	[HttpPost("AdminAuthenticate")]
	public async Task<ActionResult> AdminLogin(AdminLoginDTO admindto)
	{
		var admin = await _loginService.GetAdmin(admindto);
		if(admin is null)
			return BadRequest(new { message = "Credenciales Inválidas"});
		
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, admin.Name),
			new Claim(ClaimTypes.Email, admin.Email),
			new Claim("AdminType", admin.AdminType),
		};
		string jwtToken = GenerateToken(claims, 60);

		return Ok(new {token = jwtToken});
	}

	[HttpPost("ClientAuthenticate")]
	public async Task<ActionResult> ClientLogin(ClientLoginDTO clientdto)
	{
		// Verify password
		var client = await _loginService.GetClient(clientdto);
		if(client is null) 
			return BadRequest(new {message = "Credenciales Inválidas"});

		// Create claims
		var claims = new[]
		{
			new Claim("ClientID", client.Id.ToString()),
			new Claim(ClaimTypes.Name, client.Name),
			new Claim(ClaimTypes.Email, client.Email ?? ""),
		};

		// Generate token
		string jwtToken = GenerateToken(claims, 60);
		return Ok(new {token = jwtToken});
	}


	private string GenerateToken(Claim[] claims, int MinutesExpires)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JWT:Key").Value));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
		var securityToken = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.Now.AddMinutes(MinutesExpires),
				signingCredentials: creds);
		string token = new JwtSecurityTokenHandler().WriteToken(securityToken);
		return token;
	}
}
