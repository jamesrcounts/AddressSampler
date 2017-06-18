using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using Serilog;

namespace AddressSampler.Sampling
{
    public class RegionSample
    {
        private readonly SamplingOptions _options;
        private readonly ILogger _logger;
        private readonly string _state;
        private readonly Reduction _reduction;
        private readonly Shuffle _shuffle;

        public RegionSample(ILogger logger, SamplingOptions options, string state)
        {
            _options = options;
            _state = state;
            _logger = logger;
            var random = new Random();
            _shuffle = new Shuffle(random);
            _reduction = new Reduction(random, _options);
        }

        public List<AddressData> Sample(FileInfo dataFile)
        {
            _logger.Information($"Scanning {_state}/{dataFile.Name}... ");
            using (var dataStream = dataFile.OpenText())
            {
                var csv = new CsvReader(dataStream);
                csv.Configuration.IsHeaderCaseSensitive = false;

                var records = csv.GetRecords<AddressData>();
                var count = 0;
                var sample = new List<AddressData>();
                foreach (var record in records)
                {
                    count++;
                    if (string.IsNullOrWhiteSpace(record.Street))
                        continue;
                    if(string.IsNullOrWhiteSpace(record.City))
                        continue;
                    if(string.IsNullOrWhiteSpace(record.Number))
                        continue;
                    

                    record.Region = _state;
                    sample.Add(record);
                    if (count % _options.BatchSize != 0)
                        continue;

                    sample = _shuffle.Apply(sample);
                    sample = _reduction.Apply(sample);
                    _logger.Information($"{_state}/{dataFile.Name}: {count}");
                }

                _logger.Information($"{_state}/{dataFile.Name} done. Sample {sample.Count}/{count}");
                return sample;
            }
        }
    }
}