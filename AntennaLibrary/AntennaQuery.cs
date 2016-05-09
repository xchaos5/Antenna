using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AntennaLibrary.Annotations;

namespace AntennaLibrary
{
    public class AntennaQuery
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
                            From = 0,
                            FromUnit = BandWidthUnit.Hz,
                            To = 0,
                            ToUnit = BandWidthUnit.Hz
                        };
                        BandRanges.Add(range);
                    }
                }
            }
        }

        public ObservableCollection<BandRange> BandRanges { get; set; } = new ObservableCollection<BandRange>();
    }

    public enum BandWidthUnit
    {
        Hz,
        KHz,
        MHz,
        GHz
    }

    public class BandRange
    {
        public int No { get; set; }

        public double From { get; set; }
        
        public BandWidthUnit FromUnit { get; set; }

        public double To { get; set; }

        public BandWidthUnit ToUnit { get; set; }
    }
}
