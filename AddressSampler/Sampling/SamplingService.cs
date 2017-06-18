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
  

        public SamplingService(ILogger logger, SamplingOptions options)
        {
            _options = options;
            _logger = logger;
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
            var addresses = sample.Select(FormatAddress).ToList();
            return addresses;
        }

        private IEnumerable<AddressData> CreateSample(DirectoryInfo dataDirectory)
        {
            _logger.Information("Creating Sample... ");
            var sample = dataDirectory.EnumerateDirectories().AsParallel()
                .Select(SampleDirectory)
                .Aggregate((list, datas) => SampleReducer(list, datas, _rng));
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

        public List<AddressData> SampleDirectory(DirectoryInfo dataDirectory)
        {
            //TODO: Move hidden class.
            var rng = new Random();
            var state = Path.GetFileNameWithoutExtension(dataDirectory.Name).ToUpperInvariant();
            _logger.Information($"Scanning {state}...");
            var regionSample = new RegionSample(_logger, _options, state);
            var sample = dataDirectory.EnumerateFiles("*.csv").AsParallel()
                .Select(regionFile => regionSample.Sample(regionFile))
                .Aggregate((list, datas) => SampleReducer(list, datas, rng));

            _logger.Information($"{state} done. Sample Size {sample.Count()}");
            return sample;
        }

        private List<AddressData> SampleReducer(List<AddressData> current, List<AddressData> append, Random rng)
        {
            current.AddRange(append);
            current = new Shuffle(rng).Apply(current);
            return new Reduction(rng, _options).Apply(current);
        }
    }
}