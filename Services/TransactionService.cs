using System.Linq.Expressions;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Services;

public class TransactionService 
{
	private readonly BankContext _context;

	public TransactionService(BankContext context)
	{
		_context = context;
	}

	//** GETS **//
	public async Task<IEnumerable<BankTransactionDTOOut>> GetAll()
	{
		return await _context.BankTransactions.Select(TransactionToDto).ToListAsync();
	}
    
	public async Task<BankTransaction?> GetById(int id)
	{
		return await _context.
			BankTransactions.
			FindAsync(id);
	}

	public async Task<BankTransactionDTOOut?> GetDtoById(int id)
	{
		return await _context
			.BankTransactions
			.Where( x => x.Id == id)
			.Select(TransactionToDto)
			.SingleOrDefaultAsync();
	}
	
	public async Task<IEnumerable<BankTransactionDTOOut>> GetByAccount(int AccountId)
	{
		return await
			_context
			.BankTransactions
			.Where(x => x.AccountId == AccountId)
			.Select(TransactionToDto)
			.ToListAsync();
	}

	//** Creates **//
	public async Task<BankTransactionDTOOut> Create(BankTransactionDTOIn transaction) 
	{
		var newTransaction = new BankTransaction() {
			AccountId = transaction.AccountID, 
			TransactionType = transaction.TransactionType,
			Amount = transaction.Amount,
			ExternalAccount = transaction.ExternalAccount,
		};

		var transactionAccount = await _context.Accounts.FindAsync(transaction.AccountID);	

		if (transactionAccount is null)
			throw new KeyNotFoundException($"No se halló la cuenta {transaction.AccountID}");
		
		if (IsWithdrawal(newTransaction)) {
			if (transactionAccount.Balance < transaction.Amount)
				throw new InvalidOperationException("La cuenta no tiene el saldo necesario para realizar la operación");
			else
				transactionAccount.Balance -= transaction.Amount;
		}
		else {
			transactionAccount.Balance += transaction.Amount;
		}
		
		_context.BankTransactions.Add(newTransaction);
		await _context.SaveChangesAsync();
		return TransactionToDto.Compile()(newTransaction);
	}

	//** Transform **//
	public Expression<Func<BankTransaction, BankTransactionDTOOut>> TransactionToDto = (transaction) => new BankTransactionDTOOut 
	{	
		Id = transaction.Id,
		AccountId = transaction.AccountId,
		TransactionType = transaction.TransactionType,
		Amount = transaction.Amount,
		ExternalAccount = transaction.ExternalAccount,
		RegDate = transaction.RegDate,
	};

	public bool IsWithdrawal(BankTransaction transaction) {
		int[] withdrawalTypes = {2,4};
		return withdrawalTypes.Contains(transaction.TransactionType);
	}
}

