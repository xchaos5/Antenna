using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using AntennaLibCore;
using AntennaLibrary.Annotations;

namespace AntennaLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class AntennaTag : INotifyPropertyChanged
        {
            public string Name { get; set; }

            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    _isChecked = value;
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

        public class AntennaViewModel : INotifyPropertyChanged
        {
            public Antenna Antenna { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    _isSelected = value;
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

        private AntennaManager _antennaManager = new AntennaManager();

        public QueryBandRanges BandRanges { get; set; } = new QueryBandRanges();
        public ObservableCollection<AntennaViewModel> AntennaViewModels { get; set; }
        public ObservableCollection<AntennaTag> Tags { get; set; }

        public MainWindow()
        {
            DataContext = this;
            BandRanges.NumOfBands = 1;
            InitializeComponent();
        }

        public void Initialize()
        {
            Directory_Load();

            var antennas = Utils.LoadAntennasFromFile("Antennas.xml");
            foreach (var antenna in antennas)
            {
                _antennaManager.AddAntenna(antenna);
            }

            var antennaViewModels = new ObservableCollection<AntennaViewModel>();
            foreach (var antenna in _antennaManager.Antennas)
            {
                var antennaViewModel = new AntennaViewModel
                {
                    Antenna = antenna,
                    IsSelected = true,
                };
                antennaViewModels.Add(antennaViewModel);
            }
            AntennaViewModels = antennaViewModels;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AntennasPanel.ItemsSource = AntennaViewModels;
            }));

            Tags = new ObservableCollection<AntennaTag>();
            foreach (var tag in _antennaManager.Tags)
            {
                var antennaTag = new AntennaTag() { Name = tag, IsChecked = false };
                antennaTag.PropertyChanged += AntennaTagOnPropertyChanged;
                Tags.Add(antennaTag);
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TagsPanel.ItemsSource = Tags;
            }));
        }

        private void AntennaTagOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!Tags.Any(x => x.IsChecked))
            {
                foreach (var antennaViewModel in AntennaViewModels)
                {
                    antennaViewModel.IsSelected = true;
                }
                return;
            }

            foreach (var antennaViewModel in AntennaViewModels)
            {
                var selected = false;
                foreach (var antennaTag in Tags)
                {
                    if (antennaTag.IsChecked && antennaViewModel.Antenna.Tags.Contains(antennaTag.Name))
                    {
                        selected = true;
                        break;
                    }
                }
                antennaViewModel.IsSelected = selected;
            }
        }

        private void Directory_Load()
        {
            var directory = new ObservableCollection<DirectoryRecord>();

            directory.Add(
                new DirectoryRecord
                {
                    Info = new DirectoryInfo("Antennas")
                }
            );

            Dispatcher.BeginInvoke(new Action(() =>
            {
                AntennasTreeView.ItemsSource = directory;
            }));
        }

        private void DirectoryTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var file = e.NewValue as FileInfo;
            if (null != file)
            {
                var xpsDoc = new XpsDocument(file.FullName, FileAccess.Read);
                DocumentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            }
        }

        private void TbNumOfBands_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbNumOfBands.Text))
            {
                BandRanges.NumOfBands = 0;
                return;
            }

            uint numOfBands;
            if (!uint.TryParse(tbNumOfBands.Text, out numOfBands))
            {
                MessageBox.Show("请输入正整数");
            }
            else
            {
                BandRanges.NumOfBands = numOfBands;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var antennaTag in Tags)
            {
                antennaTag.IsChecked = false;
            }
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var antennaViewModel = element.DataContext as AntennaViewModel;
            var file = new FileInfo(antennaViewModel.Antenna.DocumentPath);
            if (null != file)
            {
                var xpsDoc = new XpsDocument(file.FullName, System.IO.FileAccess.Read);
                DocumentViewer.Document = xpsDoc.GetFixedDocumentSequence();
                DocumentViewer.Visibility = Visibility.Visible;
                AntennasViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void TabItemHeader_OnClick(object sender, RoutedEventArgs e)
        {
            if (TabContent.IsSelected)
            {
                AntennasViewer.Visibility = Visibility.Collapsed;
                DocumentViewer.Visibility = Visibility.Visible;
                QueryResultViewer.Visibility = Visibility.Collapsed;
            }
            else if (TabDesign.IsSelected)
            {
                AntennasViewer.Visibility = Visibility.Visible;
                DocumentViewer.Visibility = Visibility.Collapsed;
                QueryResultViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnFind_OnClick(object sender, RoutedEventArgs e)
        {
            AntennasViewer.Visibility = Visibility.Collapsed;
            DocumentViewer.Visibility = Visibility.Collapsed;
            QueryResultViewer.Visibility = Visibility.Visible;
        }
    }
}
