using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaLibCore
{
    public class AntennaQuery
    {
        public IList<BandRange> BandRanges;

        public IList<Polarization> Polarizations;

        public double? Gain;

        public int? _3dBWidth;

        public double? VSWR;

        public double? Efficiency;

        public double? AxialRatio;

        public double? CrossPolarization;
    }

    public class MatchResult
    {
        public bool IsMatch;

        public bool IsBandPartialMatch;
    }
}
