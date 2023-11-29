using Microsoft.AspNetCore.Mvc;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BankAPI.Controllers;

[ApiController]
[Authorize(Policy = "Client")]
[Route("api/[controller]")]
public class TransactionController: ControllerBase
{
	private TransactionService _transactions;
	private AccountService _accounts;

	public TransactionController(AccountService accounts, TransactionService transactions) {
		_transactions = transactions;
		_accounts = accounts;
	}

	// **Consultar todas sus cuentas**. Ojo, un cliente sólamente debe poder ver sus cuentas, no las de otros clientes.  
	[HttpGet("accounts/{clientID}")]
	public async Task<ActionResult<IEnumerable<AccountDTOout>>> GetAccounts(int clientID)
	{
		var currentClientID = User.FindFirstValue("ClientID");
	 	// Verifica sea quien diga
		if(clientID.ToString() != currentClientID)
			return Unauthorized(new {message = $"Usted no es el cliente {clientID}"});

		return Ok(await _accounts.GetByclient(clientID));
	}

	// **Eliminar sus cuentas**. Una cuenta sólo podrá ser eliminada si ya no dispone de dinero.
	[HttpDelete("accounts/{clientID}/{AccountID}")]
	public async Task<ActionResult> DeleteAccount(int clientID, int AccountID)
	{
		var currentClientID = User.FindFirstValue("ClientID");;

	 	// Verifica cliente sea quien diga
		if(clientID.ToString() != currentClientID)
			return Unauthorized(new {message = $"Usted no es el cliente {clientID}"});
		
		// Solo puede eliminar cuentas que le pertenecen
		var AccountToDelete = await _accounts.GetById(AccountID);

		if( (AccountToDelete is null) || (AccountToDelete.ClientId != clientID))
			return NotFound(new {message = $"La cuenta {AccountID} no es del cliente {clientID}"}); 

		// Solo puede eliminar cuentas sin dinero
		if(AccountToDelete.Balance != 0)
			return BadRequest(new {message = $"Cuenta {AccountID} tiene dinero, no se puede eliminar"});

		await _accounts.Delete(AccountID);
		return Ok();
	}

	// **Realizar retiros de sus cuentas**. Los retiros podrán ser vía transferencia (asociados a cuentas externas o propias) o en efectivo (sin cuentas externas asociadas).
	[HttpPost("retiro")]
	public async Task<ActionResult> AccountWithdrawal(RetiroDTOIn withdrawal)
	{
		var currentClientID = int.Parse(User.FindFirstValue("ClientID"));

		// Verifica cuenta exista y sea del cliente
		var AccountToModify = await _accounts.GetById(withdrawal.AccountID);
		
		if (( AccountToModify is null) || (currentClientID != AccountToModify.ClientId) )
			return NotFound(new {message = $"El cliente {currentClientID} no tiene ninguna cuenta con ID {withdrawal.AccountID}"});

		// Retiros en efectivo no pueden tener una cuenta externa
		if (withdrawal.IsCashWithdrawal && withdrawal.ExternalAccount is not null)
			return BadRequest(new {message = "Retiros en efectivo no pueden tener una cuenta externa"});
		
		// Crea transaccion
		var transaccion = new BankTransactionDTOIn
		{
			AccountID = withdrawal.AccountID,
			TransactionType = withdrawal.IsCashWithdrawal ? 2 : 4,
			Amount = withdrawal.Amount,
			ExternalAccount = withdrawal.ExternalAccount
		};

		try {
			await _transactions.Create(transaccion);
			return  Ok();
		} catch (InvalidOperationException ex) {
			return BadRequest(ex.Message);
		}
	}

  // **Realizar depósitos a sus cuentas**. Los depósitos sólo podrán ser en efectivo (sin cuentas asociadas).
	[HttpPost("deposito")]
	public async Task<ActionResult> AccountDeposit(DepositoDTOIn deposit)
	{
		var currentClientID = int.Parse(User.FindFirstValue("ClientID"));

		// Verifica cuenta exista y sea del cliente
		var AccountToModify = await _accounts.GetById(deposit.AccountID);
		
		if (( AccountToModify is null) || (currentClientID != AccountToModify.ClientId) )
			return NotFound(new {message = $"El cliente {currentClientID} no tiene ninguna cuenta con ID {deposit.AccountID}"});
		
		// Crea transaccion
		var transaccion = new BankTransactionDTOIn
		{
			AccountID = deposit.AccountID,
			TransactionType = deposit.IsCashDeposit ? 1 : 3,
			Amount = deposit.Amount,
		};
		try {
			await _transactions.Create(transaccion);
			return  Ok();
		} catch (InvalidOperationException ex) {
			return BadRequest(ex.Message);
		}
	}

}
