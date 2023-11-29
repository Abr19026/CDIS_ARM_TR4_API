using Microsoft.AspNetCore.Mvc;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace BankAPI.Controllers;

[ApiController]
[Authorize]
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
		var currentClientID = await HttpContext.GetTokenAsync("ClientID");
	 // TODO: Validate authorization
		if(clientID.ToString() != currentClientID)
			return Unauthorized(new {message = $"Usted no es el cliente {clientID}"});

		return Ok(await _accounts.GetByclient(clientID));
	}

  // **Eliminar sus cuentas**. Una cuenta sólo podrá ser eliminada si ya no dispone de dinero.
	[HttpDelete("accounts/{clientID}/{AccountID}")]
	public async Task<ActionResult> DeleteAccount(int clientID, int AccountID)
	{
		var currentClientID = await HttpContext.GetTokenAsync("ClientID");

	 // TODO: Validate authorization
		if(clientID.ToString() != currentClientID)
			return Unauthorized(new {message = $"Usted no es el cliente {AccountID}"});

		var AccountToDelete = await _accounts.GetById(AccountID);

		if( (AccountToDelete is null) || (AccountToDelete.ClientId != clientID))
			return NotFound(new {message = $"La cuenta {AccountID} no es del cliente {clientID}"}); 

		if(AccountToDelete.Balance != 0)
			return BadRequest(new {message = $"Cuenta {AccountID} tiene dinero, no se puede eliminar"});

		await _accounts.Delete(AccountID);
		return Ok();
	}

  // **Realizar retiros de sus cuentas**. Los retiros podrán ser vía transferencia (asociados a cuentas externas o propias) o en efectivo (sin cuentas externas asociadas).
	[HttpPost("retiro")]
	public async Task<ActionResult> AccountWithdrawal(RetiroDTOIn withdrawal)
	{
		if (withdrawal.IsCashWithdrawal && withdrawal.ExternalAccount is not null)
			return BadRequest(new {message = "Retiros en efectivo no pueden tener una cuenta externa"});

		var transaccion = new BankTransactionDTOIn
		{
			AccountID = withdrawal.AccountID,
			TransactionType = withdrawal.IsCashWithdrawal ? 2 : 4,
			Amount = withdrawal.Amount,
			ExternalAccount = withdrawal.ExternalAccount
		};

		await _transactions.Create(transaccion);	
		return  Ok();
	}

  // **Realizar depósitos a sus cuentas**. Los depósitos sólo podrán ser en efectivo (sin cuentas asociadas).
	[HttpPost("deposito")]
	public async Task<ActionResult> AccountDeposit(DepositoDTOIn deposit)
	{		
		var transaccion = new BankTransactionDTOIn
		{
			AccountID = deposit.AccountID,
			TransactionType = deposit.IsCashDeposit ? 1 : 3,
			Amount = deposit.Amount,
		};
		await _transactions.Create(transaccion);
		return  Ok(); 
	}

}
