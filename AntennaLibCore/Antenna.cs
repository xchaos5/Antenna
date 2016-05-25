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
                return (UpperBound.NormalizedFreq - LowerBound.NormalizedFreq) / 2 / (UpperBound.NormalizedFreq + LowerBound.NormalizedFreq);
            }
        }

        public double F0_Normalized
        {
            get
            {
                return (UpperBound.NormalizedFreq + LowerBound.NormalizedFreq) / 2;
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

        public int Scale
        {
            get
            {
                switch (Unit)
                {
                    case FreqUnit.KHz:
                        return K;
                    case FreqUnit.MHz:
                        return M;
                    case FreqUnit.GHz:
                        return G;
                    default:
                        return 1;
                }
            }
        }

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

    public class ThetaValueMap
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

        private double? _maxValue;
        public double MaxValue
        {
            get
            {
                if (_maxValue == null)
                {
                    _maxValue = Phi0Gains.Max(x => x.Value);
                }
                return _maxValue.Value;
            }
        }

        private int? __3dBWidth;
        public int _3dBWidth
        {
            get
            {
                if (__3dBWidth == null)
                {
                    var gain = MaxValue - 3;
                    var thetaGains = Phi0Gains.OrderBy(x => Math.Abs(x.Value - gain)).ToList();
                    if (thetaGains.Count > 1)
                    {
                        __3dBWidth = (int)Math.Abs(thetaGains[1].Key - thetaGains[0].Key);
                    }
                    else
                    {
                        __3dBWidth = 0;
                    }
                }
                return __3dBWidth.Value;
            }
        }
    }

    public class FreqValueMap
    {
        public Frequency Freq { get; set; }

        public IList<KeyValuePair<double, double>> Map { get; set; } = new List<KeyValuePair<double, double>>();

        public void LoadFromFile(string fileName)
        {
            Map = Utils.LoadFreqGainMapFromFile(fileName);
        }

        private double? _bandWidth;
        public double BandWidth
        {
            get
            {
                if (_bandWidth == null)
                {
                    double f_max = Map.Max(x => x.Key);
                    double f_min = Map.Min(x => x.Key);
                    _bandWidth = f_max == f_min ? 0 : (f_max - f_min) / 2 / (f_max + f_min);
                }
                return _bandWidth.Value;
            }
        }

        private double? _f0_normalized;
        public double F0_Normalized
        {
            get
            {
                if (_f0_normalized == null)
                {
                    double f_max = Map.Max(x => x.Key);
                    double f_min = Map.Min(x => x.Key);
                    _f0_normalized = (f_max + f_min) / 2 * Freq.Scale;
                }
                return _f0_normalized.Value;
            }
        }

        public double GetValueByFreq(double normalizedFreq)
        {
            var closest = Map.First();
            double freq = normalizedFreq / Freq.Scale;
            double minDistance = Math.Abs(Map.First().Key - freq);

            for (int i = 1; i < Map.Count; i++)
            {
                var distance = Math.Abs(Map[i].Key - freq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = Map[i];
                }
            }
            return closest.Value;
        }
    }

    public class Antenna
    {
        public string Name { get; set; }

        public string DocumentPath { get; set; }

        public string ImagePath { get; set; }

        public string DimensionsImagePath { get; set; }

        public IList<string> Tags { get; set; }

        public IList<ThetaValueMap> ThetaGainMaps { get; set; }

        public IList<ThetaValueMap> CrossPolarizationMaps { get; set; }

        public IList<FreqValueMap> FreqGainMaps { get; set; }

        public IList<FreqValueMap> VSWRMaps { get; set; }

        public IList<KeyValuePair<string, double>> Dimensions { get; set; }

        public double? Efficiency { get; set; }

        public string Gain
        {
            get
            {
                if (FreqGainMaps != null && FreqGainMaps.Count > 0)
                {
                    return (int)FreqGainMaps[0].Map.Max(x => x.Value) + "dBi";
                }
                if (ThetaGainMaps != null && ThetaGainMaps.Count > 0)
                {
                    return (int)ThetaGainMaps[0].MaxValue + "dBi";
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
                if (VSWRMaps != null && VSWRMaps.Count > 0)
                {
                    return Math.Round(VSWRMaps[0].Map.Max(x => x.Value), 1).ToString();
                }
                return string.Empty;
            }
        }

        public string CrossPolarization
        {
            get
            {
                if (CrossPolarizationMaps != null && CrossPolarizationMaps.Count > 0)
                {
                    return (int)CrossPolarizationMaps[0].MaxValue + "dB";
                }
                return string.Empty;
            }
        }

        public MatchResult Match(AntennaQuery query)
        {
            var result = new MatchResult(this);
            var freqValueMap = GetClosestFreqValueMap(VSWRMaps, query.BandRanges[0].F0_Normalized);
            if (freqValueMap == null)
            {
                freqValueMap = GetClosestFreqValueMap(FreqGainMaps, query.BandRanges[0].F0_Normalized);
            }
            var f0_lib = freqValueMap.F0_Normalized;
            var f0_input = query.BandRanges[0].F0_Normalized;
            result.Scale = f0_input == 0 ? 0 : f0_lib / f0_input;

            foreach (var querybandRange in query.BandRanges)
            {
                var vswrMap = GetClosestFreqValueMap(VSWRMaps, querybandRange.F0_Normalized);
                var freqGainMap = GetClosestFreqValueMap(FreqGainMaps, querybandRange.F0_Normalized);

                // BandWidth
                if (vswrMap != null)
                {
                    if (vswrMap.BandWidth < querybandRange.BandWidth)
                        return result;
                }
                else if (freqGainMap != null)
                {
                    if (freqGainMap.BandWidth < querybandRange.BandWidth)
                        return result;
                }

                var thetaGainMap = GetClosesThetaValueMap(ThetaGainMaps, querybandRange.F0_Normalized);

                // Gain
                if (query.Gain != null)
                {
                    if (freqGainMap != null)
                    {
                        if (freqGainMap.GetValueByFreq(querybandRange.F0_Normalized) < query.Gain.Value)
                        {
                            return result;
                        }

                        // Marginal gain
                        if (freqGainMap.GetValueByFreq(querybandRange.LowerBound.NormalizedFreq) < query.Gain.Value || freqGainMap.GetValueByFreq(querybandRange.UpperBound.NormalizedFreq) < query.Gain.Value)
                        {
                            result.IsMarginMatch = false;
                            return result;
                        }
                    }
                    else if (thetaGainMap != null && thetaGainMap.MaxValue < query.Gain.Value)
                    {
                        return result;
                    }
                }

                // 3dB Width
                if (query._3dBWidth != null && thetaGainMap != null && (double)Math.Abs(thetaGainMap._3dBWidth - query._3dBWidth.Value) / (double)query._3dBWidth > 0.05)
                {
                    return result;
                }

                // VSWR
                if (query.VSWR != null && vswrMap != null && vswrMap.GetValueByFreq(querybandRange.F0_Normalized) > query.VSWR.Value * 1.1)
                {
                    return result;
                }

                // Cross Polarization
                var crossPolarizationMap = GetClosesThetaValueMap(CrossPolarizationMaps, querybandRange.F0_Normalized);
                if (query.CrossPolarization != null && crossPolarizationMap != null && crossPolarizationMap.MaxValue < query.CrossPolarization.Value)
                {
                    return result;
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

            if (query.Efficiency != null && Efficiency != null && Efficiency < query.Efficiency)
            {
                return result;
            }


            // TODO: Axial Ratio


            result.IsMatch = true;
            return result;
        }

        internal ThetaValueMap GetClosesThetaValueMap(IList<ThetaValueMap> maps, double normalizedFreq)
        {
            if (maps == null || maps.Count == 0)
                return null;

            var closest = maps.First();
            double freq = normalizedFreq;
            double minDistance = Math.Abs(closest.Freq.NormalizedFreq - freq);

            for (int i = 1; i < maps.Count; i++)
            {
                var distance = Math.Abs(maps[i].Freq.NormalizedFreq - freq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = maps[i];
                }
            }
            return closest;
        }

        internal FreqValueMap GetClosestFreqValueMap(IList<FreqValueMap> maps, double normalizedFreq)
        {
            if (maps == null || maps.Count == 0)
                return null;

            var closest = maps.First();
            double freq = normalizedFreq;
            double minDistance = Math.Abs(closest.Freq.NormalizedFreq - freq);

            for (int i = 1; i < maps.Count; i++)
            {
                var distance = Math.Abs(maps[i].Freq.NormalizedFreq - freq);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = maps[i];
                }
            }
            return closest;
        }

        //internal double GetMaxGainByFreq(double normalizedFreq)
        //{
        //    var closest = FreqGainMap.First();
        //    double freq = normalizedFreq / Frequency.G;
        //    double minDistance = Math.Abs(FreqGainMap.First().Key - freq);

        //    for (int i = 1; i < FreqGainMap.Count; i++)
        //    {
        //        var distance = Math.Abs(FreqGainMap[i].Key - freq);
        //        if (distance < minDistance)
        //        {
        //            minDistance = distance;
        //            closest = FreqGainMap[i];
        //        }
        //    }

        //    return closest.Value;
        //}

        //internal double GetVSWRByFreq(double normalizedFreq)
        //{
        //    var closest = VSWRMap.First();
        //    double freq = normalizedFreq / Frequency.G;
        //    double minDistance = Math.Abs(VSWRMap.First().Key - freq);

        //    for (int i = 1; i < VSWRMap.Count; i++)
        //    {
        //        var distance = Math.Abs(VSWRMap[i].Key - freq);
        //        if (distance < minDistance)
        //        {
        //            minDistance = distance;
        //            closest = VSWRMap[i];
        //        }
        //    }

        //    return closest.Value;
        //}
    }
}
