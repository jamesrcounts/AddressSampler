using System;
using System.Collections.Generic;

namespace AddressSampler.Sampling
{
    public class Reduction
    {
        private readonly Random _random;
        private readonly SamplingOptions _options;

        public Reduction(Random random, SamplingOptions options)
        {
            _random = random;
            _options = options;
        }

        public List<TItem> Apply<TItem>(List<TItem> sample)
        {
            var excess = sample.Count - _options.SampleSize;
            while (0 < excess)
            {
                var target = _random.Next(sample.Count);
                sample.RemoveAt(target);
                excess--;
            }
            return sample;
        }
    }
}