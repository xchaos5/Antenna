using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public static readonly int G = 1000000000;

        public static readonly int M = 1000000;

        public static readonly int K = 1000;

        public double Value { get; set; }

        public FreqUnit Unit { get; set; }

        public double NormalizedFreq
        {
            get
            {
                switch (Unit)
                {
                    case FreqUnit.KHz:
                        return Value * K;
                    case FreqUnit.MHz:
                        return Value * M;
                    case FreqUnit.GHz:
                        return Value * G;
                    default:
                        return Value;
                }
            }
        }
    }

    public class ThetaGainMap
    {
        public Frequency Freq { get; set; }

        public IList<KeyValuePair<double, double>> Phi0Gains { get; set; } = new List<KeyValuePair<double, double>>();

        public IList<KeyValuePair<double, double>> Phi90Gains { get; set; } = new List<KeyValuePair<double, double>>();

        public void LoadFromFile(string fileName)
        {
            var thetaGainMap = Utils.LoadThetaGainMapFromFile(fileName);
            foreach (var pair in thetaGainMap)
            {
                Phi0Gains.Add(new KeyValuePair<double, double>(pair.Key, pair.Value.Item1));
                Phi90Gains.Add(new KeyValuePair<double, double>(pair.Key, pair.Value.Item2));
            }
        }

        private double? _maxGain;
        public double MaxGain
        {
            get
            {
                if (_maxGain == null)
                {
                    _maxGain = Phi0Gains.Max(x => x.Value);
                }
                return _maxGain.Value;
            }
        }

        private double? __3dBWidth;
        public double _3dBWidth
        {
            get
            {
                if (__3dBWidth == null)
                {
                    var gain = MaxGain - 3;
                    var thetaGains = Phi0Gains.OrderBy(x => Math.Abs(x.Value - gain)).ToList();
                    if (thetaGains.Count > 1)
                    {
                        __3dBWidth = Math.Abs(thetaGains[1].Key - thetaGains[0].Key);
                    }
                    else
                    {
                        __3dBWidth = 0.0;
                    }
                }
                return __3dBWidth.Value;
            }
        }
    }

    public class CrossPolarizationMap
    {
        public IList<KeyValuePair<double, double>> Phi0Gains { get; set; } = new List<KeyValuePair<double, double>>();

        public IList<KeyValuePair<double, double>> Phi90Gains { get; set; } = new List<KeyValuePair<double, double>>();

        public void LoadFromFile(string fileName)
        {
            var thetaGainMap = Utils.LoadThetaGainMapFromFile(fileName);
            foreach (var pair in thetaGainMap)
            {
                Phi0Gains.Add(new KeyValuePair<double, double>(pair.Key, pair.Value.Item1));
                Phi90Gains.Add(new KeyValuePair<double, double>(pair.Key, pair.Value.Item2));
            }
        }

        private double? _maxGain;
        public double MaxGain
        {
            get
            {
                if (_maxGain == null)
                {
                    _maxGain = Phi0Gains.Max(x => x.Value);
                }
                return _maxGain.Value;
            }
        }
    }

    public class Antenna
    {
        public string Name { get; set; }

        public string DocumentPath { get; set; }

        public string ImagePath { get; set; }

        public string DimensionsImagePath { get; set; }

        public BandRange BandRange { get; set; }

        public IList<string> Tags { get; set; }

        public IList<ThetaGainMap> ThetaGainMaps { get; set; }

        public IList<KeyValuePair<double, double>> FreqGainMap { get; set; }

        public IList<KeyValuePair<double, double>> VSWRMap { get; set; }

        public CrossPolarizationMap CrossPolarizationMap { get; set; }

        public IList<KeyValuePair<string, double>> Dimensions { get; set; }

        public string Gain
        {
            get
            {
                if (FreqGainMap != null)
                {
                    return (int)FreqGainMap.Max(x => x.Value) + "dBi";
                }
                if (ThetaGainMaps != null && ThetaGainMaps.Count > 0)
                {
                    return (int)ThetaGainMaps[0].MaxGain + "dBi";
                }
                return string.Empty;
            }
        }

        public string _3dBWidth
        {
            get
            {
                if (ThetaGainMaps != null && ThetaGainMaps.Count > 0)
                {
                    return (int)ThetaGainMaps[0]._3dBWidth + "°";
                }
                return string.Empty;
            }
        }

        public string VSWR
        {
            get
            {
                if (VSWRMap != null)
                {
                    return Math.Round(VSWRMap.Max(x => x.Value), 1).ToString();
                }
                return string.Empty;
            }
        }

        public string CrossPolarization
        {
            get
            {
                if (CrossPolarizationMap != null)
                {
                    return (int)CrossPolarizationMap.MaxGain + "dB";
                }
                return string.Empty;
            }
        }

        public MatchResult Match(AntennaQuery query)
        {
            var result = new MatchResult(this);
            foreach (var querybandRange in query.BandRanges)
            {
                // BandWidth
                if (BandRange.BandWidth < querybandRange.BandWidth)
                    return result;

                var f0 = (querybandRange.UpperBound.NormalizedFreq + querybandRange.LowerBound.NormalizedFreq) / 2;
                var thetaGainMap = GetClosesThetaGainMap(f0);

                // Gain
                if (query.Gain != null)
                {
                    if (FreqGainMap != null && FreqGainMap.Count > 0)
                    {
                        if (GetMaxGainByFreq(f0) < query.Gain.Value)
                        {
                            return result;
                        }

                        // Marginal gain
                        if (GetMaxGainByFreq(querybandRange.LowerBound.NormalizedFreq) < query.Gain.Value || GetMaxGainByFreq(querybandRange.UpperBound.NormalizedFreq) < query.Gain.Value)
                        {
                            result.IsMarginMatch = false;
                            return result;
                        }
                    }
                    else if (thetaGainMap != null && thetaGainMap.MaxGain < query.Gain.Value)
                    {
                        return result;
                    }
                }

                // 3dB Width
                if (query._3dBWidth != null && thetaGainMap != null 
                    && Math.Abs(thetaGainMap._3dBWidth - query._3dBWidth.Value) / query._3dBWidth > 0.05)
                {
                    return result;
                }

                // VSWR
                if (query.VSWR != null && VSWRMap != null)
                {
                    var vswr = GetVSWRByFreq(f0);
                    if (vswr > query.VSWR.Value * 1.1)
                    {
                        return result;
                    }
                }
            }

            // Polarization
            if (query.Polarizations != null)
            {
                foreach (var polarization in query.Polarizations)
                {
                    if (!Tags.Contains(polarization, StringComparer.InvariantCultureIgnoreCase))
                    {
                        return result;
                    }
                }
            }

            // Cross Polarization
            if (query.CrossPolarization != null && CrossPolarizationMap != null && CrossPolarizationMap.MaxGain < query.CrossPolarization.Value)
            {
                return result;
            }

            // TODO: Axial Ratio


            result.IsMatch = true;
            return result;
        }

        internal ThetaGainMap GetClosesThetaGainMap(double normalizedFreq)
        {
            if (ThetaGainMaps == null || ThetaGainMaps.Count == 0)
                return null;

            var closest = ThetaGainMaps.First();
            double freq = normalizedFreq / Frequency.G;
            double minDistance = Math.Abs(closest.Freq.NormalizedFreq - freq);

            for (int i = 1; i < ThetaGainMaps.Count; i++)
            {
                var distance = Math.Abs(ThetaGainMaps[i].Freq.NormalizedFreq - freq);
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
            double freq = normalizedFreq / Frequency.G;
            double minDistance = Math.Abs(FreqGainMap.First().Key - freq);

            for (int i = 1; i < FreqGainMap.Count; i++)
            {
                var distance = Math.Abs(FreqGainMap[i].Key - freq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = FreqGainMap[i];
                }
            }

            return closest.Value;
        }

        internal double GetVSWRByFreq(double normalizedFreq)
        {
            var closest = VSWRMap.First();
            double freq = normalizedFreq / Frequency.G;
            double minDistance = Math.Abs(VSWRMap.First().Key - freq);

            for (int i = 1; i < VSWRMap.Count; i++)
            {
                var distance = Math.Abs(VSWRMap[i].Key - freq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = VSWRMap[i];
                }
            }

            return closest.Value;
        }
    }
}
