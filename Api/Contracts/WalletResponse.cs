namespace Api.Contracts
{
    public class WalletResponse
    {
        public required long Id { get; set; }
        public required string Currency { get; set; }
        public decimal Balance { get; set; }

    }
}