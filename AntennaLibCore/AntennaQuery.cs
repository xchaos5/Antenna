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

        public IList<string> Polarizations = new List<string>();

        public double? Gain;

        public int? _3dBWidth;

        public double? VSWR;

        public double? AxialRatio;

        public double? CrossPolarization;
    }

    public class MatchResult
    {
        public bool IsMatch = false;

        public bool IsMarginMatch = true;

        public Antenna Antenna { get; private set; }

        public MatchResult(Antenna antenna)
        {
            Antenna = antenna;
        }
    }

    public class QueryResult
    {
        public bool HasResult { get; set; }

        public Antenna BestMatch { get; set; }

        public IList<Antenna> OtherMatches { get; set; }
    }
}
