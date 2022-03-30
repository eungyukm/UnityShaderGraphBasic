using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = UnityEngine.Random;

namespace ycdivfx.ProbeGenerator.Generators
{
    public class UnityRnd : RandomSource
    {
        public UnityRnd()
        {
            Random.InitState(RandomSeed.Robust());
        }

        public UnityRnd(int seed)
        {
            Random.InitState(seed);
        }

        protected override double DoSample()
        {
            return Random.value;
        }
    }
}
