using BankAPI.Data;
using BankAPI.Data.BankModels;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Services;

public class ClientService
{
	private readonly BankContext _context;

	public ClientService(BankContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<Client>> GetAll()
	{
		return await _context.Clients.ToListAsync();
	}
    
	public async Task<Client?> GetById(int id)
	{
		return await _context.Clients.FindAsync(id);
	}

	public async Task<Client> Create(Client client) 
	{
		_context.Clients.Add(client);
		await _context.SaveChangesAsync();
		return client;
	}

	public async Task Update(int id, Client client)
	{
		var existingclient = await GetById(id);

		if(existingclient is not null) {
			existingclient.Name = client.Name;
			existingclient.PhoneNumber = client.PhoneNumber;
			existingclient.Email = client.Email;

			await _context.SaveChangesAsync();
		}
	}

	public async Task Delete(int id)
	{
		var ClientToDelete = await GetById(id);
		if(ClientToDelete is not null) {
			_context.Clients.Remove(ClientToDelete);
			await	_context.SaveChangesAsync();
		}
	}
}

