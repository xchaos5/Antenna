using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntennaLibCore
{
    public class Utils
    {
        public static int GetIntValue(XElement element, string descendant)
        {
            int val = 0;
            int.TryParse(element.Element(descendant).Value, out val);
            return val;
        }

        public static double GetDoubleValue(XElement element, string descendant)
        {
            double val = 0;
            double.TryParse(element.Element(descendant).Value, out val);
            return val;
        }

        public static FreqUnit GetFreqUnit(XElement element, string descendant)
        {
            string freq = element.Element(descendant).Value;
            switch (freq)
            {
                case "Hz":
                    return FreqUnit.Hz;
                case "KHz":
                    return FreqUnit.KHz;
                case "MHz":
                    return FreqUnit.MHz;
                case "GHz":
                    return FreqUnit.GHz;
                default:
                    return FreqUnit.GHz;
            }
        }

        public static Polarization GetPolarization(XElement element, string descendant)
        {
            string polarization = element.Element(descendant).Value;
            switch (polarization)
            {
                case "Single":
                    return Polarization.Single;
                case "Dual":
                    return Polarization.Dual;
                case "Linear":
                    return Polarization.Linear;
                case "Circular":
                    return Polarization.Circular;
                default:
                    return Polarization.Circular;
            }
        }

        public static IList<KeyValuePair<double, Tuple<double, double>>> LoadThetaGainMapFromFile(string fileName)
        {
            var thetaGainMap = new List<KeyValuePair<double, Tuple<double, double>>>();
            using (var reader = new StreamReader(fileName))
            {
                string line = reader.ReadLine();    // skip first line
                while (null != (line = reader.ReadLine()))
                {
                    var vals = line.Split(',');
                    double theta = double.Parse(vals[0]);
                    double phi0Gain = double.Parse(vals[1]);
                    double phi90Gain = double.Parse(vals[2]);
                    thetaGainMap.Add(new KeyValuePair<double, Tuple<double, double>>(theta, new Tuple<double, double>(phi0Gain, phi90Gain)));
                }
            }
            return thetaGainMap;
        }

        public static IList<KeyValuePair<double, double>> LoadFreqGainMapFromFile(string fileName)
        {
            var listVSWR = new List<KeyValuePair<double, double>>();
            using (var reader = new StreamReader(fileName))
            {
                string line = reader.ReadLine();    // skip first line
                while (null != (line = reader.ReadLine()))
                {
                    var vals = line.Split(',');
                    double freq = double.Parse(vals[0]);
                    double value = double.Parse(vals[1]);
                    listVSWR.Add(new KeyValuePair<double, double>(freq, value));
                }
            }
            return listVSWR.OrderBy(x => x.Key).ToList();
        }

        public static IList<KeyValuePair<string, Tuple<double, string>>> LoadDimensionMapFromFile(string fileName)
        {
            var listDimensions = new List<KeyValuePair<string, Tuple<double, string>>>();
            using (var reader = new StreamReader(fileName))
            {
                string line = reader.ReadLine();    // skip first line
                while (null != (line = reader.ReadLine()))
                {
                    var vals = line.Split(',');
                    string name = vals[0];
                    double value = double.Parse(vals[1]);
                    string unit = vals[2];
                    listDimensions.Add(new KeyValuePair<string, Tuple<double, string>>(name, new Tuple<double, string>(value, unit)));
                }
            }
            return listDimensions;
        }

        public static IEnumerable<Antenna> LoadAntennasFromFile(string fileName)
        {
            var antennas = new List<Antenna>();
            var xDoc = XDocument.Load(fileName);
            foreach (var antennaElement in xDoc.Descendants("Antenna"))
            {
                var antenna = new Antenna
                {
                    Name = antennaElement.Element("Name").Value,
                    DocumentPath = antennaElement.Element("Document").Value,
                    ImagePath = antennaElement.Element("Image").Value,
                    Tags = antennaElement.Element("Tags").Value.Split(',').ToList(),
                };

                if (antennaElement.Descendants("ThetaGainMap").Any())
                {
                    antenna.ThetaGainMaps = new List<ThetaValueMap>();
                    foreach (var gainMapElement in antennaElement.Descendants("ThetaGainMap"))
                    {
                        var thetaGainMap = new ThetaValueMap();
                        thetaGainMap.LoadFromFile(gainMapElement.Element("FilePath").Value);
                        thetaGainMap.Freq = new Frequency()
                        {
                            Value = GetDoubleValue(gainMapElement, "Freq"),
                            Unit = GetFreqUnit(gainMapElement, "FreqUnit"),
                        };
                        antenna.ThetaGainMaps.Add(thetaGainMap);
                    }
                    antenna.ThetaGainMaps.OrderBy(x => x.Freq.NormalizedFreq);
                }

                if (antennaElement.Descendants("CrossPolarizationMap").Any())
                {
                    antenna.CrossPolarizationMaps = new List<ThetaValueMap>();
                    foreach (var xpMapElement in antennaElement.Descendants("CrossPolarizationMap"))
                    {
                        var xpMap = new ThetaValueMap();
                        xpMap.LoadFromFile(xpMapElement.Element("FilePath").Value);
                        xpMap.Freq = new Frequency()
                        {
                            Value = GetDoubleValue(xpMapElement, "Freq"),
                            Unit = GetFreqUnit(xpMapElement, "FreqUnit"),
                        };
                        antenna.CrossPolarizationMaps.Add(xpMap);
                    }
                    antenna.CrossPolarizationMaps.OrderBy(x => x.Freq.NormalizedFreq);
                }

                if (antennaElement.Descendants("FreqGainMap").Any())
                {
                    antenna.FreqGainMaps = new List<FreqValueMap>();
                    foreach (var fgMapElemeent in antennaElement.Descendants("FreqGainMap"))
                    {
                        var fgMap = new FreqValueMap();
                        fgMap.LoadFromFile(fgMapElemeent.Element("FilePath").Value);
                        fgMap.Freq = new Frequency()
                        {
                            Value = GetDoubleValue(fgMapElemeent, "Freq"),
                            Unit = GetFreqUnit(fgMapElemeent, "FreqUnit"),
                        };
                        antenna.FreqGainMaps.Add(fgMap);
                    }
                    antenna.FreqGainMaps.OrderBy(x => x.Freq.NormalizedFreq);
                }

                if (antennaElement.Descendants("VSWRMap").Any())
                {
                    antenna.VSWRMaps = new List<FreqValueMap>();
                    foreach (var vswrMapElement in antennaElement.Descendants("VSWRMap"))
                    {
                        var vswrMap = new FreqValueMap();
                        vswrMap.LoadFromFile(vswrMapElement.Element("FilePath").Value);
                        vswrMap.Freq = new Frequency()
                        {
                            Value = GetDoubleValue(vswrMapElement, "Freq"),
                            Unit = GetFreqUnit(vswrMapElement, "FreqUnit"),
                        };
                        antenna.VSWRMaps.Add(vswrMap);
                    }
                    antenna.VSWRMaps.OrderBy(x => x.Freq.NormalizedFreq);
                }

                if (antennaElement.Descendants("DimensionsImage").Any())
                {
                    antenna.DimensionsImagePath = antennaElement.Element("DimensionsImage").Value;
                }

                if (antennaElement.Descendants("DimensionMap").Any())
                {
                    antenna.DimensionMaps = new List<DimensionMap>();
                    foreach (var dimMapElement in antennaElement.Descendants("DimensionMap"))
                    {
                        var dimensionMap = new DimensionMap();
                        dimensionMap.LoadFromFile(dimMapElement.Element("FilePath").Value);
                        dimensionMap.Freq = new Frequency()
                        {
                            Value = GetDoubleValue(dimMapElement, "Freq"),
                            Unit = GetFreqUnit(dimMapElement, "FreqUnit"),
                        };
                        antenna.DimensionMaps.Add(dimensionMap);
                    }
                    antenna.DimensionMaps.OrderBy(x => x.Freq.NormalizedFreq);
                }

                if (antennaElement.Descendants("Efficiency").Any())
                {
                    antenna.Efficiency = GetDoubleValue(antennaElement, "Efficiency");
                }

                antennas.Add(antenna);
            }
            return antennas;
        }
    }
}
