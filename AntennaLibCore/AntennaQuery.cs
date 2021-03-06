﻿using System;
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

        public double? Efficiency;

        public double? AxialRatio;

        public double? CrossPolarization;
    }

    public class MatchResult
    {
        public bool IsMatch = false;

        public bool IsMarginMatch = true;

        public DimensionMap Dimensions { get; set; }

        public double Scale { get; set; }

        public Antenna Antenna { get; private set; }

        public MatchResult(Antenna antenna)
        {
            Antenna = antenna;
        }
    }

    public class QueryResult
    {
        public bool HasResult { get; set; }

        public MatchResult BestMatch { get; set; }

        public IList<MatchResult> OtherMatches { get; set; }
    }
}
