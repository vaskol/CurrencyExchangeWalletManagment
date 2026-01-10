namespace Api.Contracts
{
    public class CreateWalletRequest
    {
        public required long Id { get; set; }
        public required string Currency { get; set; }
        public decimal InitialBalance { get; set; }
    }
}