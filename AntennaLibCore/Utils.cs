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

        public static IList<KeyValuePair<int, Tuple<double, double>>> LoadThetaGainMapFromFile(string fileName)
        {
            var thetaGainMap = new List<KeyValuePair<int, Tuple<double, double>>>();
            using (var reader = new StreamReader(fileName))
            {
                string line = reader.ReadLine();    // skip first line
                while (null != (line = reader.ReadLine()))
                {
                    var vals = line.Split(',');
                    int theta = int.Parse(vals[0]);
                    double phi0Gain = double.Parse(vals[1]);
                    double phi90Gain = double.Parse(vals[2]);
                    thetaGainMap.Add(new KeyValuePair<int, Tuple<double, double>>(theta, new Tuple<double, double>(phi0Gain, phi90Gain)));
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

        public static IEnumerable<Antenna> LoadAntennasFromFile(string fileName)
        {
            try
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
                        BandRange = new BandRange()
                        {
                            LowerBound = new Frequency()
                            {
                                Value = GetDoubleValue(antennaElement, "MinFreq"),
                                Unit = GetFreqUnit(antennaElement, "MinFreqUnit"),
                            },
                            UpperBound = new Frequency()
                            {
                                Value = GetDoubleValue(antennaElement, "MaxFreq"),
                                Unit = GetFreqUnit(antennaElement, "MaxFreqUnit"),
                            },
                        },
                        Tags = antennaElement.Element("Tags").Value.Split(',').ToList(),
                    };

                    if (antennaElement.Descendants("ThetaGainMap").Any())
                    {
                        antenna.ThetaGainMaps = new List<ThetaGainMap>();
                        foreach (var gainMapElement in antennaElement.Descendants("ThetaGainMap"))
                        {
                            var thetaGainMap = new ThetaGainMap();
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

                    if (antennaElement.Descendants("CrossPolarization").Any())
                    {
                        var crossPolarization = new CrossPolarization();
                        crossPolarization.LoadFromFile(antennaElement.Element("CrossPolarization").Value);
                        antenna.CrossPolarization = crossPolarization;
                    }

                    if (antennaElement.Descendants("FreqGainMap").Any())
                    {
                        antenna.FreqGainMap = LoadFreqGainMapFromFile(antennaElement.Element("FreqGainMap").Value);
                    }

                    if (antennaElement.Descendants("VSWR").Any())
                    {
                        antenna.VSWR = LoadFreqGainMapFromFile(antennaElement.Element("VSWR").Value);
                    }

                    if (antennaElement.Descendants("Dimension").Any())
                    {
                        antenna.Dimensions = new List<KeyValuePair<string, double>>();
                        foreach (var dim in antennaElement.Descendants("Dimension"))
                        {
                            string dimName = dim.Element("Name").Value;
                            double dimValue = GetDoubleValue(dim, "Value");
                            antenna.Dimensions.Add(new KeyValuePair<string, double>(dimName, dimValue));
                        }
                    }

                    antennas.Add(antenna);
                }
                return antennas;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
