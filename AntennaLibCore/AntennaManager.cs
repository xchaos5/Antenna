using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntennaLibCore
{
    public class AntennaManager
    {
        private Dictionary<string, Antenna> _antennas = new Dictionary<string, Antenna>();

        public IEnumerable<Antenna> Antennas => _antennas.Values;

        public IEnumerable<string> Tags
        {
            get
            {
                return _antennas.Values.SelectMany(x => x.Tags).Distinct();
            }
        } 

        public bool AddAntenna(Antenna antenna)
        {
            if (!_antennas.ContainsKey(antenna.Name))
            {
                _antennas.Add(antenna.Name, antenna);
                return true;
            }
            return false;
        }

        public bool RemoveAntenna(string antennaName)
        {
            if (_antennas.ContainsKey(antennaName))
            {
                _antennas.Remove(antennaName);
                return true;
            }
            return false;
        }

        public Antenna GetAntenna(string antennaName)
        {
            if (_antennas.ContainsKey(antennaName))
            {
                return _antennas[antennaName];
            }
            return null;
        }

        public QueryResult ExecuteAntennaQuery(AntennaQuery query)
        {
            var queryResult = new QueryResult();
            queryResult.HasResult = false;
            var results = _antennas.Values.Select(antenna => antenna.Match(query)).Where(x => x.IsMatch).OrderByDescending(x => x.IsMarginMatch).ToList();
            
            if (results.Count > 0)
            {
                queryResult.HasResult = true;
                queryResult.BestMatch = results.First();
                queryResult.OtherMatches = results.Skip(1).ToList();
            }

            return queryResult;
        }
    }
}
