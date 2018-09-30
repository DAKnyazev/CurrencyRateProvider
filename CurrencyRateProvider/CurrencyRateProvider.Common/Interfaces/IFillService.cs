using System;
using System.Threading.Tasks;

namespace CurrencyRateProvider.Common.Interfaces
{
    public interface IFillService
    {
        /// <summary>
        /// Загрузить в БД данные о курсах валют за выбранные годы
        /// </summary>
        /// <param name="startYear">Начальный год (включительно)</param>
        /// <param name="endYear">Конечный год (включительно)</param>
        /// <returns></returns>
        Task<bool> TryFill(int startYear, int endYear);

        /// <summary>
        /// Загрузить в БД данные о курсах валют за выбранный день
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<bool> TryFill(DateTime date);
    }
}
