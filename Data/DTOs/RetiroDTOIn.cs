namespace BankAPI.Data.DTOs;

public class RetiroDTOIn
{
	public int AccountID { get; set; }
	public bool IsCashWithdrawal { get; set; } 
	public decimal Amount { get; set; }
  	public int? ExternalAccount { get; set; }
}
