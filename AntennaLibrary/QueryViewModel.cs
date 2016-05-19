using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AntennaLibCore;
using AntennaLibrary.Annotations;

namespace AntennaLibrary
{
    public class QueryViewModel : INotifyPropertyChanged
    {
        private int _numOfBands;
        public int NumOfBands
        {
            get { return _numOfBands; }
            set
            {
                if (_numOfBands != value && value >= 0 && value <= 10)
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

        private double? _gain;

        public double? Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                OnPropertyChanged();
            }
        }

        private int? __3dBWidth;
        public int? _3dBWidth
        {
            get { return __3dBWidth; }
            set
            {
                __3dBWidth = value;
                OnPropertyChanged();
            }
        }

        private double? _VSWR;

        public double? VSWR
        {
            get { return _VSWR; }
            set
            {
                _VSWR = value;
                OnPropertyChanged();
            }
        }

        public double? Efficiency;

        public double? AxialRatio;

        private double? _CrossPolarization;

        public double? CrossPolarization
        {
            get { return _CrossPolarization; }
            set
            {
                _CrossPolarization = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
