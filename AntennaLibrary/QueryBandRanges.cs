using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AntennaLibCore;
using AntennaLibrary.Annotations;

namespace AntennaLibrary
{
    public class QueryBandRanges
    {
        private uint _numOfBands;
        public uint NumOfBands
        {
            get { return _numOfBands; }
            set
            {
                if (_numOfBands != value)
                {
                    _numOfBands = value;
                    BandRanges.Clear();
                    for (int i = 0; i < _numOfBands; i++)
                    {
                        var range = new BandRange
                        {
                            No = i + 1,
                            LowerBound = new Frequency()
                            {
                                Value = 0,
                                Unit = FreqUnit.GHz,
                            },
                            UpperBound = new Frequency()
                            {
                                Value = 0,
                                Unit = FreqUnit.GHz,
                            },
                        };
                        BandRanges.Add(range);
                    }
                }
            }
        }

        public ObservableCollection<BandRange> BandRanges { get; set; } = new ObservableCollection<BandRange>();
    }
}
