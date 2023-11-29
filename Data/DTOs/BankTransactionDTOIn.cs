namespace BankAPI.Data.DTOs;

public class BankTransactionDTOIn
{
	public int AccountID { get; set; }
	public int TransactionType { get; set; }
	public decimal Amount { get; set; }
	public int? ExternalAccount { get; set; }
}
