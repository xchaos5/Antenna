using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaLibCore
{
    public class AntennaManager
    {
        private Dictionary<string, Antenna> _antennas;

        public bool AddAntenna(string name, Antenna antenna)
        {
            if (!_antennas.ContainsKey(name))
            {
                _antennas.Add(name, antenna);
                return true;
            }
            return false;
        }

        public bool RemoveAntenna(string name)
        {
            if (_antennas.ContainsKey(name))
            {
                _antennas.Remove(name);
                return true;
            }
            return false;
        }

        public Antenna GetAntennaByName(string name)
        {
            if (_antennas.ContainsKey(name))
            {
                return _antennas[name];
            }
            return null;
        }


    }
}
