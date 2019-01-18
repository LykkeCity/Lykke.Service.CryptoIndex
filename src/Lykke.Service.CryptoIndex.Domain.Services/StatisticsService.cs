using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Domain.Handlers;
using Lykke.Service.CryptoIndex.Domain.Models;
using Lykke.Service.CryptoIndex.Domain.Repositories;

namespace Lykke.Service.CryptoIndex.Domain.Services
{
    public class StatisticsService : IStatisticsService, IIndexHandler
    {
        private readonly object _sync24H = new object();
        private bool _initialized;
        // add each value
        private readonly SortedDictionary<DateTime, decimal> _history24H = new SortedDictionary<DateTime, decimal>();
        private decimal _currentValue;
        private decimal _max24H = decimal.MinValue;
        private decimal _min24H = decimal.MaxValue;
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

        private readonly IIndexHistoryRepository _indexHistoryRepository;
        private readonly IChartHistory5DRepository _chartHistory5DRepository;
        private readonly IChartHistory30DRepository _chartHistory30DRepository;

        private readonly ILog _log;

        public StatisticsService(IIndexHistoryRepository indexHistoryRepository,
            IChartHistory5DRepository chartHistory5DRepository,
            IChartHistory30DRepository chartHistory30DRepository,
            ILogFactory logFactory)
        {
            _indexHistoryRepository = indexHistoryRepository;
            _chartHistory5DRepository = chartHistory5DRepository;
            _chartHistory30DRepository = chartHistory30DRepository;

            _log = logFactory.CreateLog(this);
        }

        public Task HandleAsync(IndexHistory indexHistory)
        {
            try
            {
                lock (_sync24H)
                {
                    if (!_initialized)
                    {
                        Initialize();
                        _initialized = true;
                    }

                    _currentValue = indexHistory.Value;

                    _history24H[indexHistory.Time] = indexHistory.Value;

                    // remove old
                    foreach (var time in _history24H.Keys.ToList())
                        if (time < DateTime.UtcNow.AddDays(-1))
                            _history24H.Remove(time);

                    CalculateKeyNumbers24H();
                }

                lock (_sync5D)
                {
                    var newest = _history5D.Keys.LastOrDefault();

                    if (newest == default(DateTime) // empty
                        || indexHistory.Time - newest > TimeSpan.FromMinutes(5))
                    {
                        _chartHistory5DRepository.InsertOrReplaceAsync(indexHistory.Time, indexHistory.Value).GetAwaiter().GetResult();
                        _history5D[indexHistory.Time] = indexHistory.Value;

                        // remove old
                        foreach (var time in _history5D.Keys.ToList())
                            if (time < DateTime.UtcNow.AddDays(-5))
                                _history5D.Remove(time);

                        CalculateKeyNumbers5D();
                    }
                }

                lock (_sync30D)
                {
                    var newest = _history30D.Keys.LastOrDefault();

                    if (newest == default(DateTime) // empty
                        || indexHistory.Time - newest > TimeSpan.FromMinutes(30))
                    {
                        _chartHistory30DRepository.InsertOrReplaceAsync(indexHistory.Time, indexHistory.Value).GetAwaiter().GetResult();
                        _history30D[indexHistory.Time] = indexHistory.Value;

                        // remove old
                        foreach (var time in _history30D.Keys.ToList())
                            if (time < DateTime.UtcNow.AddDays(-30))
                                _history30D.Remove(time);

                        CalculateKeyNumbers30D();
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Warning("Something went wrong in StatisticsService.", ex);
            }

            return Task.CompletedTask;
        }

        private void CalculateKeyNumbers24H()
        {
            var oldest = _history24H.Keys.FirstOrDefault();
            var newest = _history24H.Keys.LastOrDefault();
            var oldestValue = _history24H[oldest];
            var newestValue = _history24H[newest];
            _return24H = CalculateReturn(oldestValue, newestValue);
            _return24H = Math.Round(_return24H, 2);

            _max24H = _history24H.Values.Max();
            _min24H = _history24H.Values.Min();

            _volatility24H = Volatility24H();
            _volatility24H = Math.Round(_volatility24H, 2);
        }

        private void CalculateKeyNumbers5D()
        {
            var oldest = _history5D.Keys.FirstOrDefault();
            var newest = _history5D.Keys.LastOrDefault();

            if (oldest == default(DateTime))
                return;

            var oldestValue = _history5D[oldest];
            var newestValue = _history5D[newest];
            _return5D = CalculateReturn(oldestValue, newestValue);
            _return5D = Math.Round(_return5D, 2);
        }

        private void CalculateKeyNumbers30D()
        {
            var oldest = _history30D.Keys.FirstOrDefault();
            var newest = _history30D.Keys.LastOrDefault();

            if (oldest == default(DateTime))
                return;

            var oldestValue = _history30D[oldest];
            var newestValue = _history30D[newest];
            _return30D = CalculateReturn(oldestValue, newestValue);
            _return30D = Math.Round(_return30D, 2);

            _volatility30D = Volatility30D();
            _volatility30D = Math.Round(_volatility30D, 2);
        }

        public IDictionary<DateTime, decimal> GetIndexHistory24H()
        {
            lock (_sync24H)
                return _history24H;
        }

        public IDictionary<DateTime, decimal> GetIndexHistory5D()
        {
            lock (_sync5D)
                return _history5D;
        }

        public IDictionary<DateTime, decimal> GetIndexHistory30D()
        {
            lock (_sync30D)
                return _history30D;
        }

        public KeyNumbers GetKeyNumbers()
        {
            return new KeyNumbers
            {
                CurrentValue = _currentValue,
                Max24H = _max24H,
                Min24H = _min24H,
                Return24H = _return24H,
                Return5D = _return5D,
                Return30D = _return30D,
                Volatility24H = _volatility24H,
                Volatility30D = _volatility30D
            };
        }

        private void Initialize()
        {
            var history24H = _indexHistoryRepository.GetAsync(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow).GetAwaiter().GetResult();
            if (history24H.Any())
                lock (_sync24H)
                {
                    foreach (var point in history24H)
                        _history24H[point.Time] = point.Value;
                    
                    CalculateKeyNumbers24H();
                }

            var history5D = _chartHistory5DRepository.GetAsync(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow).GetAwaiter().GetResult();
            if (history5D.Any())
                lock (_sync5D)
                {
                    foreach (var point5D in history5D)
                        _history5D[point5D.Key] = point5D.Value;

                    CalculateKeyNumbers5D();
                }

            var history30D = _chartHistory30DRepository.GetAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow).GetAwaiter().GetResult();
            if (history30D.Any())
                lock (_sync30D)
                {
                    foreach (var point30D in history30D)
                        _history30D[point30D.Key] = point30D.Value;

                    CalculateKeyNumbers30D();
                }
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

                var _return = CalculateReturn(previous, current);
                hourlyReturn.Add(_return);
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

        public static decimal CalculateReturn(decimal oldestValue, decimal newestValue)
        {
            var result = (newestValue - oldestValue) / oldestValue * 100;

            return result;
        }

        public static decimal CalculateVolatility(SortedDictionary<DateTime, decimal> absoluteValues, TimeSpan returnInterval)
        {
            var returns = new List<decimal>();

            var intervalValues = new SortedDictionary<DateTime, decimal>();
            foreach (var time in absoluteValues.Keys.ToList())
            {
                var newest = absoluteValues.Keys.LastOrDefault();
                if (newest == default(DateTime) || newest - time > returnInterval)
                    intervalValues[time] = absoluteValues[time];
            }

            var values = intervalValues.Values.ToList();
            for (var i = 0; i < values.Count; i++)
            {
                if (i == 0)
                    continue;

                var current = values[i];
                var previous = values[i - 1];

                returns.Add((previous - current) / previous * 100);
            }

            if (returns.Count == 0)
                return 0;

            var mean = returns.Sum() / returns.Count;

            double sum = 0;
            foreach (var current in returns)
            {
                sum += Math.Pow((double)(current - mean), 2);
            }

            var result = (decimal)Math.Sqrt(sum / returns.Count);

            return result;
        }
    }
}
