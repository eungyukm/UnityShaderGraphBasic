using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ycdivfx.ProbeGenerator.Generators
{
    public class RandomSystemSource : RandomSource
    {
        private readonly Random _rnd;

        public RandomSystemSource()
        {
            _rnd = new Random(RandomSeed.Robust());
        }

        public RandomSystemSource(int seed)
        {
            _rnd = new Random(seed);
        }

        protected override double DoSample()
        {
            return _rnd.NextDouble();
        }
    }
}
