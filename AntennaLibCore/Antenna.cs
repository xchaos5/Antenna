using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaLibCore
{
    public enum FreqUnit
    {
        Hz,
        KHz,
        MHz,
        GHz,
    }

    public enum Polarization
    {
        Single,
        Dual,
        Linear,
        Circular,
    }

    public class BandRange
    {
        public int No { get; set; }

        public Frequency LowerBound { get; set; }

        public Frequency UpperBound { get; set; }

        public double BandWidth
        {
            get
            {
                return (UpperBound.NormalizedFreq + LowerBound.NormalizedFreq) / 2 / (UpperBound.NormalizedFreq - LowerBound.NormalizedFreq);
            }
        }
    }

    public class Frequency
    {
        public double Value { get; set; }

        public FreqUnit Unit { get; set; }

        public double NormalizedFreq
        {
            get
            {
                switch (Unit)
                {
                    case FreqUnit.KHz:
                        return Value * 1000;
                    case FreqUnit.MHz:
                        return Value * 1000000;
                    case FreqUnit.GHz:
                        return Value * 1000000000;
                    default:
                        return Value;
                }
            }
        }
    }

    public class ThetaGainMap
    {
        public Frequency Freq { get; set; }

        public IList<KeyValuePair<int, double>> Phi0Gains { get; set; } = new List<KeyValuePair<int, double>>();

        public IList<KeyValuePair<int, double>> Phi90Gains { get; set; } = new List<KeyValuePair<int, double>>();

        public void LoadFromFile(string fileName)
        {
            var thetaGainMap = Utils.LoadThetaGainMapFromFile(fileName);
            foreach (var pair in thetaGainMap)
            {
                Phi0Gains.Add(new KeyValuePair<int, double>(pair.Key, pair.Value.Item1));
                Phi90Gains.Add(new KeyValuePair<int, double>(pair.Key, pair.Value.Item2));
            }
        }

        public double MaxGain
        {
            get
            {
                return Phi0Gains.Max(x => x.Value);
            }
        }
    }

    public class CrossPolarization
    {
        public IList<KeyValuePair<int, double>> Phi0Gains { get; set; } = new List<KeyValuePair<int, double>>();

        public IList<KeyValuePair<int, double>> Phi90Gains { get; set; } = new List<KeyValuePair<int, double>>();

        public void LoadFromFile(string fileName)
        {
            var thetaGainMap = Utils.LoadThetaGainMapFromFile(fileName);
            foreach (var pair in thetaGainMap)
            {
                Phi0Gains.Add(new KeyValuePair<int, double>(pair.Key, pair.Value.Item1));
                Phi90Gains.Add(new KeyValuePair<int, double>(pair.Key, pair.Value.Item2));
            }
        }
    }

    public class Antenna
    {
        public string Name { get; set; }

        public string DocumentPath { get; set; }

        public string ImagePath { get; set; }

        public BandRange BandRange { get; set; }

        public IList<string> Tags { get; set; }

        public IList<ThetaGainMap> ThetaGainMaps { get; set; }

        public IList<KeyValuePair<double, double>> FreqGainMap { get; set; }

        public IList<KeyValuePair<double, double>> VSWR { get; set; }

        public CrossPolarization CrossPolarization { get; set; }

        public IList<KeyValuePair<string, double>> Dimensions { get; set; }

        public MatchResult Match(AntennaQuery query)
        {
            var result = new MatchResult();
            foreach (var querybandRange in query.BandRanges)
            {
                // BandWidth
                if (BandRange.BandWidth < querybandRange.BandWidth)
                    return result;

                // Gain
                if (query.Gain != null)
                {
                    var f0 = (querybandRange.UpperBound.NormalizedFreq + querybandRange.LowerBound.NormalizedFreq) / 2;

                    if (FreqGainMap != null && FreqGainMap.Count > 0)
                    {
                        if (GetMaxGainByFreq(f0) < query.Gain.Value)
                        {
                            return result;
                        }

                        // TODO: Match marginal gain
                    }
                    else if (ThetaGainMaps != null && ThetaGainMaps.Count > 0)
                    {
                        var thetaGainMap = GetClosesThetaGainMap(f0);
                        if (thetaGainMap.MaxGain < query.Gain.Value)
                            return result;
                    }

                }
            }

            result.IsMatch = true;
            return result;
        }

        internal ThetaGainMap GetClosesThetaGainMap(double normalizedFreq)
        {
            var closest = ThetaGainMaps.First();
            double minDistance = Math.Abs(closest.Freq.NormalizedFreq - normalizedFreq);

            for (int i = 1; i < ThetaGainMaps.Count; i++)
            {
                var distance = Math.Abs(ThetaGainMaps[i].Freq.NormalizedFreq - normalizedFreq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = ThetaGainMaps[i];
                }
            }

            return closest;
        }

        internal double GetMaxGainByFreq(double normalizedFreq)
        {
            var closest = FreqGainMap.First();
            double minDistance = Math.Abs(FreqGainMap.First().Key - normalizedFreq);

            for (int i = 1; i < FreqGainMap.Count; i++)
            {
                var distance = Math.Abs(FreqGainMap[i].Key - normalizedFreq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = FreqGainMap[i];
                }
            }

            return closest.Value;
        }
    }
}
