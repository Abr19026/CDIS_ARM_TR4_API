using Microsoft.AspNetCore.Mvc;
using BankAPI.Data.BankModels;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
	private readonly ClientService _service;
	private AccountService _accounts;

	public ClientController(ClientService clientService, AccountService accountService)
	{
		_service = clientService;
		_accounts = accountService;
	}

	[HttpGet]
	[Authorize(Policy = "Admin")]
	async public Task<IEnumerable<Client>> Get()
	{
		return await _service.GetAll();
	}

	[HttpGet("{id}")]
	[Authorize(Policy = "Admin")]
	async public Task<ActionResult<Client>> GetById(int id)
	{
		var client = await _service.GetById(id);
		if (client is null) {
			return ClientNotFound(id);
		}
		return client;
	}


	// TODO: No debe aceptar ID ni RegDate
	[HttpPost]
	[Authorize(Policy = "SuperAdmin")]
	async public Task<IActionResult> Create(Client client) 
	{
		var newClient = await _service.Create(client);
		return CreatedAtAction(nameof(GetById), new { id = newClient.Id }, newClient);
	}

	[HttpPut("{id}")]
	[Authorize(Policy = "SuperAdmin")]
	async public Task<IActionResult> Update(int id, Client client)
	{
		if(id != client.Id) {
			return BadRequest(new { message = $"El ID de la url: {id} y el del cliente: {client.Id} no coinciden."});
		}

		var ClientToUpdate = await _service.GetById(id);

		if (ClientToUpdate is not null) {
			await _service.Update(id, client);
			return NoContent();
		}
		return ClientNotFound(id);
	}

	[HttpDelete("{id}")]
	[Authorize(Policy = "SuperAdmin")]
	async public Task<IActionResult> Delete(int id)
	{
		var ClientToDelete = await _service.GetById(id);

		if (ClientToDelete is not null) {
			await _service.Delete(id);
			return Ok();
		}
		return ClientNotFound(id);
	}
	
	// Mensaje personalizado de error
	public NotFoundObjectResult ClientNotFound(int id)
	{
		return NotFound(new { message = $"El cliente con ID = {id} no existe."});
	}
}
