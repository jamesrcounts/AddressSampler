using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace AddressSampler.Sampling
{
    public class SamplingService : ISamplingService
    {
        private readonly ILogger _logger;
        private readonly SamplingOptions _options;
        private readonly Random _rng = new Random();
        private readonly Aggregator _aggregator;


        public SamplingService(ILogger logger, SamplingOptions options)
        {
            _options = options;
            _logger = logger;
            _aggregator = new Aggregator(_rng, _options);
        }

        public void Run()
        {
            var dataDirectory = GetDataDirectory();
            var sample = CreateSample(dataDirectory);
            var addresses = FormatAddressData(sample);
            WriteSample(addresses);
        }

        private void WriteSample(IEnumerable<string> addresses)
        {
            _logger.Information("Writing Sample... ");
            File.Delete(_options.OutputPath);
            File.AppendAllLines(_options.OutputPath, addresses);
        }

        private IEnumerable<string> FormatAddressData(IEnumerable<AddressData> sample)
        {
            _logger.Information("Formatting address data... ");
            return sample.Select(FormatAddress).ToList();
        }

        private IEnumerable<AddressData> CreateSample(DirectoryInfo dataDirectory)
        {
            _logger.Information("Creating Sample... ");
            var sample = dataDirectory.EnumerateDirectories().AsParallel()
                .Select(stateDirectory => new StateSample(_logger, _options, stateDirectory))
                .Select(stateSample => stateSample.Sample())
                .Aggregate((list, datas) => _aggregator.Apply(list, datas));
            _logger.Information($"Done. Sample Size {sample.Count()}");
            return sample;
        }

        private DirectoryInfo GetDataDirectory()
        {
            var path = _options.DataPath;
            var dataDirectory = new DirectoryInfo(path);
            _logger.Information($"Data source: {dataDirectory.FullName}");
            return dataDirectory;
        }

        private static string FormatAddress(AddressData data)
        {
            var s = $"{data.Number} {data.Street}";
            if (!string.IsNullOrWhiteSpace(data.Unit))
                s += $" #{data.Unit}";
            s += $", {data.City}, {data.Region} {data.PostCode}";
            return s;
        }
    }
}