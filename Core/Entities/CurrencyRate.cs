namespace Core.Entities
{
    public class CurrencyRate
    {
        public required string Currency { get; set; } = default!;
        public required decimal Rate { get; set; } 
        public required DateTime RateDate { get; set; }
    }

}
