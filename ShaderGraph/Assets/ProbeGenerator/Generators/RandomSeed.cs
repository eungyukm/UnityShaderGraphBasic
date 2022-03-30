﻿using System;

namespace ycdivfx.ProbeGenerator.Generators
{
    public static class RandomSeed
    {
        static readonly object Lock = new object();

        static readonly System.Random MasterRng = new System.Random();

        /// <summary>
        /// Provides a time-dependent seed value, matching the default behavior of System.Random.
        /// WARNING: There is no randomness in this seed and quick repeated calls can cause
        /// the same seed value. Do not use for cryptography!
        /// </summary>
        public static int Time()
        {
            return Environment.TickCount;
        }

        /// <summary>
        /// Provides a seed based on time and unique GUIDs.
        /// WARNING: There is only low randomness in this seed, but at least quick repeated
        /// calls will result in different seed values. Do not use for cryptography!
        /// </summary>
        public static int Guid()
        {
            return Environment.TickCount ^ System.Guid.NewGuid().GetHashCode();
        }

        /// <summary>
        /// Provides a seed based on an internal random number generator (crypto if available), time and unique GUIDs.
        /// WARNING: There is only medium randomness in this seed, but quick repeated
        /// calls will result in different seed values. Do not use for cryptography!
        /// </summary>
        public static int Robust()
        {
            lock (Lock)
            {
                return MasterRng.NextFullRangeInt32() ^ Environment.TickCount ^ System.Guid.NewGuid().GetHashCode();
            }
        }
    }
}
