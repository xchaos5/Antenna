using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaLibCore
{
    public class Antenna
    {
        public string Name { get; set; }

        public List<string> Tags { get; set; }

        public double BandWidth { get; set; }

        public List<string> Polarization { get; set; }

        public double Gain { get; set; }

        public int _3dBWidth { get; set; }

        public double VSWR { get; set; }

        public double Efficiency { get; set; }

        public double AxialRatio { get; set; }

        public int CrossPolarization { get; set; }
    }
}
