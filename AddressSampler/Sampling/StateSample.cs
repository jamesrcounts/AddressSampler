using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace AddressSampler.Sampling
{
    public class StateSample
    {
        private readonly ILogger _logger;
        private readonly Random _random = new Random();
        private readonly Aggregator _aggregator;
        private readonly DirectoryInfo _dataDirectory;
        private readonly string _state;
        private readonly RegionSample _regionSample;

        public StateSample(ILogger logger, SamplingOptions options, DirectoryInfo dataDirectory)
        {
            _logger = logger;
            _dataDirectory = dataDirectory;
            _aggregator = new Aggregator(_random, options);
            _state = Path.GetFileNameWithoutExtension(dataDirectory.Name).ToUpperInvariant();
            _regionSample = new RegionSample(_logger, options, _state);
        }

        public List<AddressData> Sample()
        {
            _logger.Information($"Scanning {_state}...");
            var sample = _dataDirectory.EnumerateFiles("*.csv").AsParallel()
                .Select(regionFile => _regionSample.Sample(regionFile))
                .Aggregate((list, datas) => _aggregator.Apply(list, datas));

            _logger.Information($"{_state} done. Sample Size {sample.Count()}");
            return sample;
        }
    }
}