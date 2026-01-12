namespace Core.Entities
{
    public class CurrencyRate(string currency, decimal rate, DateTime rateDate)
    {
        public string Currency { get; private set; } = currency;
        public decimal Rate { get; private set; } = rate;
        public DateTime RateDate { get; private set; } = rateDate;

        private CurrencyRate() : this(string.Empty, 0, default) { }
    }
}
