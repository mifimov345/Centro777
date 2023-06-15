using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Centro777.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Centro777.Pages
{
    public class ConvertModel : PageModel
    {


        /// <summary>
        /// А У ЦЕНТРОБАНКА НА 14.06.2023 ГОД НЕТ РУБЛЕЙ В ПЕРЕВОДАХ, Я ДАЖЕ НЕ РОФЛЮ
        /// </summary>
        private readonly IHttpClientFactory _clientFactory;

        public ConvertModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public CurrencyConversion Conversion { get; set; }

        public decimal? ConvertedAmount { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.cbr-xml-daily.ru/daily_json.js");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var currencyRates = JsonSerializer.Deserialize<CurrencyRates>(json);

                var sourceCurrency = Conversion.SourceCurrency.ToUpperInvariant();
                var targetCurrency = Conversion.TargetCurrency.ToUpperInvariant();

                // Проверка наличия исходной валюты
                if (!currencyRates.Valute.ContainsKey(sourceCurrency))
                {
                    ModelState.AddModelError(nameof(Conversion.SourceCurrency), $"Исходная валюта '{Conversion.SourceCurrency}' не найдена.");
                }

                // Проверка наличия целевой валюты
                if (!currencyRates.Valute.ContainsKey(targetCurrency))
                {
                    ModelState.AddModelError(nameof(Conversion.TargetCurrency), $"Целевая валюта '{Conversion.TargetCurrency}' не найдена.");
                }

                if (ModelState.IsValid)
                {
                    var sourceCurrencyRate = currencyRates.Valute[sourceCurrency].Value;
                    var targetCurrencyRate = currencyRates.Valute[targetCurrency].Value;

                    ConvertedAmount = Conversion.Amount / sourceCurrencyRate * targetCurrencyRate;
                }
            }

            return Page();
        }

    }
}
public class CurrencyRates
{
    public DateTime Date { get; set; }
    public string PreviousDate { get; set; }
    public string PreviousURL { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, CurrencyRate> Valute { get; set; }
}

public class CurrencyRate
{
    public string ID { get; set; }
    public string NumCode { get; set; }
    public string CharCode { get; set; }
    public int Nominal { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public decimal Previous { get; set; }
}
