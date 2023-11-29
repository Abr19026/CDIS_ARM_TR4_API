using System.Linq.Expressions;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Services;

public class AccountService
{
	private readonly BankContext _context;

	public AccountService(BankContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<AccountDTOout>> GetAll()
	{
			return await _context.
			Accounts
			.Select(AccountToDto)
			.ToListAsync();
	}
    
	public async Task<Account?> GetById(int id)
	{
		return await _context.Accounts.FindAsync(id);
	}

	public async Task<IEnumerable<AccountDTOout>> GetByclient(int clientID)
	{
		return await _context.Accounts.Where(x => x.ClientId == clientID).Select(AccountToDto).ToListAsync();
	}

	public async Task<AccountDTOout?> GetDtoById(int id)
	{
		return await _context
			.Accounts
			.Where( x => x.Id == id)
			.Select(AccountToDto)
			.SingleOrDefaultAsync();
	}

	public async Task<AccountDTOout> Create(AccountDTOIn account) 
	{
		var newAccount = new Account();
		newAccount.AccountType = account.AccountType;
		newAccount.ClientId = account.ClientId;
		newAccount.Balance = account.Balance;

		_context.Accounts.Add(newAccount);
		await _context.SaveChangesAsync();
		return AccountToDto.Compile()(newAccount);
	}

	public async Task Update(int id, AccountDTOIn account)
	{
		var existingaccount = await GetById(id);

		if(existingaccount is not null) {
			existingaccount.AccountType = account.AccountType;
			existingaccount.ClientId = account.ClientId;
			existingaccount.Balance = account.Balance;

			await _context.SaveChangesAsync();
		}
	}

	public async Task Delete(int id)
	{
		var AccountToDelete = await GetById(id);
		if(AccountToDelete is not null) {
			_context.Accounts.Remove(AccountToDelete);
			await	_context.SaveChangesAsync();
		}
	}

	public Expression<Func<Account, AccountDTOout>> AccountToDto = (account) => new AccountDTOout
	{	
		Id = account.Id,
		AccountName = account.AccountTypeNavigation.Name,
		ClientName = account.Client != null ? account.Client.Name : "",
		Balance = account.Balance,
		RegDate = account.RegDate,
	};


}

