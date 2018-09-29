using System;
using System.Threading.Tasks;

namespace CurrencyRateProvider.Common.Interfaces
{
    public interface IFillService
    {
        Task<bool> TryFill(int startYear, int endYear);
        Task<bool> TryFill(DateTime day);
    }
}
