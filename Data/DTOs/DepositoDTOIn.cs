namespace BankAPI.Data.DTOs;

public class DepositoDTOIn
{
	public int AccountID { get; set; }
	public bool IsCashDeposit { get; set; }
	public decimal Amount { get; set; }
}
