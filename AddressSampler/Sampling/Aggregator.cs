using System;
using System.Collections.Generic;

namespace AddressSampler.Sampling
{
    public class Aggregator
    {
        private readonly Shuffle _shuffle;
        private readonly Reduction _reduction;

        public Aggregator(Random rng, SamplingOptions options)
        {
            _shuffle = new Shuffle(rng);
            _reduction = new Reduction(rng, options);
        }

        public List<AddressData> Apply(List<AddressData> current, List<AddressData> append)
        {
            current.AddRange(append);
            current = _shuffle.Apply(current);
            return _reduction.Apply(current);
        }
    }
}