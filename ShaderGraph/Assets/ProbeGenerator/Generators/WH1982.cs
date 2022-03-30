using System.Collections.Generic;

namespace ycdivfx.ProbeGenerator.Generators
{

    public class WH1982 : RandomSource
    {
        private const uint Modx = 30269;
        private const double ModxRecip = 1.0/Modx;
        private const uint Mody = 30307;
        private const double ModyRecip = 1.0/Mody;
        private const uint Modz = 30323;
        private const double ModzRecip = 1.0/Modz;

        uint _xn;
        uint _yn = 1;
        uint _zn = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class using
        /// a seed based on time and unique GUIDs.
        /// </summary>
        public WH1982() : this(RandomSeed.Robust())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WH1982"/> class.
        /// </summary>
        /// <param name="seed">The seed value.</param>
        /// <remarks>If the seed value is zero, it is set to one. Uses the
        /// value of <see cref="Control.ThreadSafeRandomNumberGenerators"/> to
        /// set whether the instance is thread safe.</remarks>
        public WH1982(int seed)
        {
            if (seed == 0)
                seed = 1;

            _xn = (uint)seed%Modx;
        }

        /// <summary>
        /// Returns a random double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        protected override double DoSample()
        {
            _xn = (171*_xn)%Modx;
            _yn = (172*_yn)%Mody;
            _zn = (170*_zn)%Modz;

            double w = _xn*ModxRecip + _yn*ModyRecip + _zn*ModzRecip;
            w -= (int)w;
            return w;
        }

        /// <summary>
        /// Fills an array with random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        private static void Doubles(IList<double> values, int seed)
        {
            if (seed == 0)
                seed = 1;

            uint xn = (uint)seed%Modx;
            uint yn = 1;
            uint zn = 1;

            for (int i = 0; i < values.Count; i++)
            {
                xn = (171*xn)%Modx;
                yn = (172*yn)%Mody;
                zn = (170*zn)%Modz;

                var w = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip;
                values[i] = w - (int)w;
            }
        }

        /// <summary>
        /// Returns an array of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads.</remarks>
        public static double[] Doubles(int length, int seed)
        {
            var data = new double[length];
            Doubles(data, seed);
            return data;
        }

        /// <summary>
        /// Returns an infinite sequence of random numbers greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <remarks>Supports being called in parallel from multiple threads, but the result must be enumerated from a single thread each.</remarks>
        public static IEnumerable<double> DoubleSequence(int seed)
        {
            if (seed == 0)
            {
                seed = 1;
            }

            uint xn = (uint)seed%Modx;
            uint yn = 1;
            uint zn = 1;

            while (true)
            {
                xn = (171*xn)%Modx;
                yn = (172*yn)%Mody;
                zn = (170*zn)%Modz;

                double w = xn*ModxRecip + yn*ModyRecip + zn*ModzRecip;
                yield return w - (int)w;
            }
        }
    }
}
