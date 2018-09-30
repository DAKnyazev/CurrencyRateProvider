using System.Collections.Generic;
using System.Linq;

namespace CurrencyRateProvider.Common.Models.Report
{
    /// <summary>
    /// Статистика по курсу валюты
    /// </summary>
    public class RateStatistic
    {
        private readonly List<decimal> _costs;

        public RateStatistic()
        {
            _costs = new List<decimal>();
        }

        /// <summary>
        /// Добавить стоимость
        /// </summary>
        /// <param name="cost">Стоимость</param>
        /// <param name="amount">Количество</param>
        public void AddCost(decimal cost, int amount)
        {
            _costs.Add(cost / amount);
        }
        
        /// <summary>
        /// Максимальная цена
        /// </summary>
        public decimal MaxCost => _costs.Max();

        /// <summary>
        /// Минимальная цена
        /// </summary>
        public decimal MinCost => _costs.Min();

        /// <summary>
        /// Медианная цена
        /// </summary>
        public decimal MedianCost => GetMedian();

        /// <summary>
        /// Получить медианную цену
        /// </summary>
        /// <returns></returns>
        private decimal GetMedian()
        {
            var halfIndex = _costs.Count / 2;
            var sortedCosts = _costs.OrderBy(n => n).ToList();

            if (_costs.Count % 2 == 0)
            {
                return (sortedCosts.ElementAt(halfIndex) + sortedCosts.ElementAt(halfIndex - 1))/2;
            }

            return sortedCosts.ElementAt(halfIndex);
        }
    }
}