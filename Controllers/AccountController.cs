using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
	private readonly AccountService _accountservice;
	private readonly AccountTypeService _accounttypeservice;
	private readonly ClientService _clientservice;

	public AccountController(AccountService accountService,
													 AccountTypeService accounttypeService,
													 ClientService clientService)
	{
		_accountservice = accountService;
		_accounttypeservice = accounttypeService;
		_clientservice = clientService;
	}


	[HttpGet]
	async public Task<IEnumerable<AccountDTOout>> Get()
	{
		return await _accountservice.GetAll();
	}


	[HttpGet("{id}")]
	async public Task<ActionResult<AccountDTOout>> GetById(int id)
	{
		var account = await _accountservice.GetDtoById(id);

		if (account is null) 
			return AccountNotFound(id);
		
		return account;
	}


	// TODO: No debe aceptar ID ni RegDate
	[HttpPost]
	[Authorize(Policy = "SuperAdmin")]
	async public Task<IActionResult> Create(AccountDTOIn account) 
	{
		string validationResult = await ValidateAccount(account);

		if(! validationResult.Equals("Valid"))
			return BadRequest(new {message=validationResult});
		
		var newAccount = await _accountservice.Create(account);

		return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, newAccount);
	}


	[HttpPut("{id}")]
	[Authorize(Policy = "SuperAdmin")]
	async public Task<IActionResult> Update(int id, AccountDTOIn account)
	{
		if(id != account.Id)
			return BadRequest(new { message = $"El ID de la url: {id} y el de la cuenta: {account.Id} no coinciden."});
		

		var AccountToUpdate = await _accountservice.GetById(id);

		if (AccountToUpdate is not null) {
			string validationResult = await ValidateAccount(account);

			if(! validationResult.Equals("Valid"))
				return BadRequest(new {message=validationResult});
	
			await _accountservice.Update(id, account);

			return NoContent();
		}

		return AccountNotFound(id);
	}


	[HttpDelete("{id}")]
	[Authorize(Policy = "SuperAdmin")]
	public async Task<IActionResult> Delete(int id)
	{
		var AccountToDelete = await _accountservice.GetById(id);

		if (AccountToDelete is not null) {
			await _accountservice.Delete(id);
			return Ok();
		}
		return AccountNotFound(id);
	}
	
	// Mensaje personalizado de error
	public NotFoundObjectResult AccountNotFound(int id)
	{
		return NotFound(new { message = $"La cuenta con ID = {id} no existe."});
	}

	public async Task<string> ValidateAccount(AccountDTOIn account)
	{
		string result = "Valid";
		var accounttype = await _accounttypeservice.GetById(account.AccountType);

		if (accounttype is null)
			result = $"El tipo de cuenta {accounttype} no existe";

		var ClientID = account.ClientId.GetValueOrDefault();
		var client = await _clientservice.GetById(ClientID);
		if(client is null)
			result = $"El cliente {ClientID} no existe";

		return result;
	}
}
