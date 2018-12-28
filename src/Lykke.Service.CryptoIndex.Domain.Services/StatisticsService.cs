using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Services.Models;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public class StatisticsService : IIndexHandler
    {
        private readonly object _sync24H = new object();
        // add each value
        private readonly SortedDictionary<DateTime, decimal> _history24H = new SortedDictionary<DateTime, decimal>();
        private decimal _currentValue;
        private decimal _max24H;
        private decimal _min24H;
        private decimal _volatility24H;
        private decimal _return24H;

        private readonly object _sync5D = new object();
        // add value each 5 minutes
        private readonly SortedDictionary<DateTime, decimal> _history5D = new SortedDictionary<DateTime, decimal>();
        private decimal _return5D;

        private readonly object _sync30D = new object();
        // add value each 30 minutes
        private readonly SortedDictionary<DateTime, decimal> _history30D = new SortedDictionary<DateTime, decimal>();
        private decimal _volatility30D;
        private decimal _return30D;

        private readonly ILog _log;

        public StatisticsService(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public Task HandleAsync(IndexHistory indexHistory)
        {
            try
            {
                lock (_sync24H)
                {
                    _currentValue = indexHistory.Value;


                    var oldest = _history24H.Keys.FirstOrDefault();

                    if (oldest != default(DateTime)) // not empty
                        _history24H.Remove(oldest);


                    _history24H[indexHistory.Time] = indexHistory.Value;

                    if (indexHistory.Value > _max24H)
                        _max24H = indexHistory.Value;

                    if (indexHistory.Value < _min24H)
                        _min24H = indexHistory.Value;

                    _volatility24H = Volatility24H();

                    oldest = _history24H.Keys.FirstOrDefault();
                    var newest = _history24H.Keys.LastOrDefault();
                    var oldestValue = _history24H[oldest];
                    var newestValue = _history24H[newest];
                    _return24H = (oldestValue - newestValue) / oldestValue * 100;
                }

                lock (_sync5D)
                {
                    var newest = _history5D.Keys.LastOrDefault();

                    if (newest == default(DateTime) // empty
                        || newest - indexHistory.Time > TimeSpan.FromMinutes(5))
                    {
                        var oldest = _history5D.Keys.FirstOrDefault();
                        _history5D.Remove(oldest);
                        _history5D[indexHistory.Time] = indexHistory.Value;

                        var oldestValue = _history5D[oldest];
                        var newestValue = _history5D[newest];
                        _return5D = (oldestValue - newestValue) / oldestValue * 100;
                    }
                }

                lock (_sync30D)
                {
                    var newest = _history30D.Keys.LastOrDefault();

                    if (newest == default(DateTime) // empty
                        || newest - indexHistory.Time > TimeSpan.FromMinutes(30))
                    {
                        var oldest = _history30D.Keys.FirstOrDefault();
                        _history30D.Remove(oldest);
                        _history30D[indexHistory.Time] = indexHistory.Value;

                        var oldestValue = _history30D[oldest];
                        var newestValue = _history30D[newest];
                        _return30D = (oldestValue - newestValue) / oldestValue * 100;

                        _volatility30D = Volatility30D();
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Warning("Something went wrong in StatisticsService.", ex);
            }

            return Task.CompletedTask;
        }

        public IDictionary<DateTime, decimal> GetIndexHistory24h()
        {
            lock (_sync24H)
                return _history24H;
        }

        public IDictionary<DateTime, decimal> GetIndexHistory5d()
        {
            lock (_sync5D)
                return _history5D;
        }

        public IDictionary<DateTime, decimal> GetIndexHistory30d()
        {
            lock (_sync30D)
                return _history30D;
        }

        public KeyNumbers GetKeyNumbers()
        {
            return new KeyNumbers
            {
                CurrentValue = _currentValue,
                Max24h = _max24H,
                Min24h = _min24H,
                Return24h = _return24H,
                Return5d = _return5D,
                Return30d = _return30D,
                Volatility24h = _volatility24H,
                Volatility30d = _volatility30D
            };
        }

        private void Initialize()
        {
            // Загрузить значения справочников из БД
        }

        private decimal Volatility24H()
        {
            var hourlyReturn = new List<decimal>();

            var hourlyValues = new SortedDictionary<DateTime, decimal>();
            foreach (var time in _history24H.Keys.ToList())
            {
                var newest = _history24H.Keys.LastOrDefault();
                if (newest == default(DateTime) || newest - time > TimeSpan.FromHours(1))
                    hourlyValues[time] = _history24H[time];
            }

            var values = hourlyValues.Values.ToList();
            for (var i = 0; i < values.Count; i++)
            {
                if (i == 0)
                    continue;

                var current = values[i];
                var previous = values[i - 1];

                hourlyReturn.Add((previous - current) / previous * 100);
            }

            if (hourlyReturn.Count == 0)
                return 0;

            var mean = hourlyReturn.Sum() / hourlyReturn.Count;

            double sum = 0;
            foreach (var current in hourlyReturn)
            {
                sum += Math.Pow((double)(current - mean), 2);
            }

            var result = (decimal)Math.Sqrt(sum / hourlyReturn.Count);

            return result;
        }

        private decimal Volatility30D()
        {
            var dailyReturn = new List<decimal>();

            var dailyValues = new SortedDictionary<DateTime, decimal>();
            foreach (var time in _history30D.Keys.ToList())
            {
                var newest = _history30D.Keys.LastOrDefault();
                if (newest == default(DateTime) || newest - time > TimeSpan.FromDays(1))
                    dailyValues[time] = _history30D[time];
            }

            var values = dailyValues.Values.ToList();
            for (var i = 0; i < values.Count; i++)
            {
                if (i == 0)
                    continue;

                var current = values[i];
                var previous = values[i - 1];

                dailyReturn.Add((previous - current) / previous * 100);
            }

            if (dailyReturn.Count == 0)
                return 0;

            var mean = dailyReturn.Sum() / dailyReturn.Count;

            double sum = 0;
            foreach (var current in dailyReturn)
            {
                sum += Math.Pow((double)(current - mean), 2);
            }

            var volatility30D = (decimal)Math.Sqrt(sum / dailyReturn.Count);

            return volatility30D;
        }
    }
}
