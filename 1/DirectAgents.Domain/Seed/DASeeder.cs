using System.Linq;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities;

namespace DirectAgents.Domain.Seed
{
    public class DASeeder
    {
        // Caller is responsible for instantiating and disposing of the context(s)
        public DAContext context { get; set; }

        public DASeeder(DAContext context)
        {
            this.context = context;
        }

        public void SeedCurrencies()
        {
            AddCurrencyIfNotExist(CurrencyId.NONE, CurrencyAbbr.NONE);
            AddCurrencyIfNotExist(CurrencyId.USD, CurrencyAbbr.USD);
            AddCurrencyIfNotExist(CurrencyId.GBP, CurrencyAbbr.GBP);
            AddCurrencyIfNotExist(CurrencyId.EUR, CurrencyAbbr.EUR);
            AddCurrencyIfNotExist(CurrencyId.CAD, CurrencyAbbr.CAD);
            AddCurrencyIfNotExist(CurrencyId.AUD, CurrencyAbbr.AUD);
        }

        public void AddCurrencyIfNotExist(int id, string abbrev)
        {
            if (!context.Currencies.Any(x => x.Id == id))
            {
                var currency = new Currency
                {
                    Id = id,
                    Abbr = abbrev
                };
                context.Currencies.Add(currency);
                context.SaveChanges();
            }
        }
    }
}
