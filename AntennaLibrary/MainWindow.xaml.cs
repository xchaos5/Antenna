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
        public enum Panel
        {
            Document,
            Antennas,
            Dimensions,
            QueryResult,
        }

        private readonly Dictionary<string, Panel> _panels = new Dictionary<string, Panel>()
        {
            {"DocumentPanel", Panel.Document},
            {"AntennasPanel", Panel.Antennas},
            {"DimensionsPanel" ,Panel.Dimensions},
            {"QueryResultPanel" ,Panel.QueryResult},
        };

        private Panel _lastPanel = Panel.Antennas;
        private Panel _currentPanel = Panel.Antennas;

        private AntennaManager _antennaManager = new AntennaManager();

        public QueryViewModel QueryViewModel { get; set; } = new QueryViewModel();
        public ObservableCollection<AntennaViewModel> AntennaViewModels { get; set; }
        public ObservableCollection<AntennaTag> Tags { get; set; }

        public AntennaDocumentsRoot AntennaDocuments { get; set; }

        public MainWindow()
        {
            DataContext = this;
            QueryViewModel.NumOfBands = 1;

            InitializeComponent();
        }

        public void Initialize()
        {
            //Directory_Load();

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

            Tags = new ObservableCollection<AntennaTag>();
            foreach (var tag in _antennaManager.Tags)
            {
                var antennaTag = new AntennaTag() { Name = tag, IsChecked = false };
                antennaTag.PropertyChanged += AntennaTagOnPropertyChanged;
                Tags.Add(antennaTag);
            }

            AntennaDocuments = new AntennaDocumentsRoot() { Name = "Antennas" };
            AntennaDocuments.LoadFromFile("AntennaDocuments.xml");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                AntennasControl.ItemsSource = AntennaViewModels;
                TagsPanel.ItemsSource = Tags;
                TreeViewRoot.ItemsSource = AntennaDocuments.Categories;

                QueryResultPanel.DataContext = new QueryResult()
                {
                    HasResult = false
                };
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

        private void ShowPanel(Panel panel)
        {
            foreach (var p in _panels)
            {
                var panelElement = FindName(p.Key) as FrameworkElement;
                if (p.Value == panel)
                {
                    if (panelElement != null)
                    {
                        panelElement.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (panelElement != null)
                    {
                        panelElement.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (_currentPanel != panel)
            {
                _lastPanel = _currentPanel;
                _currentPanel = panel;
            }
        }

        private void DirectoryTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var file = e.NewValue as FileInfo;
            if (null != file)
            {
                var xpsDoc = new XpsDocument(file.FullName, FileAccess.Read);
                DocumentViewer.Document = xpsDoc.GetFixedDocumentSequence();

                ShowPanel(Panel.Document);
            }
        }

        private void TbNumOfBands_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbNumOfBands.Text))
            {
                QueryViewModel.NumOfBands = 0;
                return;
            }

            int numOfBands;
            if (!int.TryParse(TbNumOfBands.Text, out numOfBands))
            {
                MessageBox.Show("请输入正整数");
            }
            else
            {
                QueryViewModel.NumOfBands = numOfBands;
            }
        }

        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var antennaTag in Tags)
            {
                antennaTag.IsChecked = false;
            }
        }

        private void AntennasViewItem_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var antennaViewModel = element.DataContext as AntennaViewModel;
            var file = new FileInfo(antennaViewModel.Antenna.DocumentPath);
            if (null != file)
            {
                var xpsDoc = new XpsDocument(file.FullName, System.IO.FileAccess.Read);
                DocumentViewer.Document = xpsDoc.GetFixedDocumentSequence();

                ShowPanel(Panel.Document);
            }
        }

        private void BtnFind_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Validation.GetHasError(TbNumOfBands) && !Validation.GetHasError(TbGain) && !Validation.GetHasError(Tb3dBWidth) && !Validation.GetHasError(TbVSWR) && !Validation.GetHasError(TbCrossPolarization))
            {
                ShowPanel(Panel.QueryResult);

                var query = new AntennaQuery();
                query.BandRanges = QueryViewModel.BandRanges;
                query.Gain = QueryViewModel.Gain;
                query._3dBWidth = QueryViewModel._3dBWidth;
                query.VSWR = QueryViewModel.VSWR;
                query.CrossPolarization = QueryViewModel.CrossPolarization;
                if (RbSingle.IsChecked != null && RbSingle.IsChecked.Value)
                {
                    query.Polarizations.Add("single band");
                }
                if (RbDual.IsChecked != null && RbDual.IsChecked.Value)
                {
                    query.Polarizations.Add("dual band");
                }
                if (RbLinear.IsChecked != null && RbLinear.IsChecked.Value)
                {
                    query.Polarizations.Add("linear polarization");
                }
                if (RbCircular.IsChecked != null && RbCircular.IsChecked.Value)
                {
                    query.Polarizations.Add("circular polarization");
                }
                QueryResultPanel.DataContext = _antennaManager.ExecuteAntennaQuery(query);
            }
        }

        private void QueryResult_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            if (element == null) return;

            ShowPanel(Panel.Dimensions);

            if (element.DataContext is QueryResult)
            {
                var antenna = element.DataContext as QueryResult;
                DimensionsPanel.DataContext = antenna.BestMatch;
            }
            if (element.DataContext is Antenna)
            {
                DimensionsPanel.DataContext = element.DataContext;
            }
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            ShowPanel(_lastPanel);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabContent.IsSelected)
            {
                ShowPanel(Panel.Antennas);
            }
        }

        private void AntennaDocument_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBlcok = e.Source as TextBlock;
            if (null != textBlcok)
            {
                var antennaDocument = textBlcok.DataContext as AntennaDocument;
                if (null != antennaDocument)
                {
                    var file = new FileInfo(antennaDocument.Document);
                    var xpsDoc = new XpsDocument(file.FullName, FileAccess.Read);
                    DocumentViewer.Document = xpsDoc.GetFixedDocumentSequence();
                }

                ShowPanel(Panel.Document);
            }
        }
    }
}
