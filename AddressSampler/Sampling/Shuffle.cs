using System;
using System.Collections.Generic;

namespace AddressSampler.Sampling
{
    public class Shuffle
    {
        private readonly Random _random;

        public Shuffle(Random random)
        {
            _random = random;
        }

        public  List<TItem> Apply<TItem>(List<TItem> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}