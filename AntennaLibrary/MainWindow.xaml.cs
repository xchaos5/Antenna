using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

namespace AntennaLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AntennaQuery Query { get; set; } = new AntennaQuery();

        public MainWindow()
        {
            DataContext = this;
            Query.NumOfBands = 1;
            InitializeComponent();
            Directory_Load();
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

            directoryTreeView.ItemsSource = directory;
        }

        private void DirectoryTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var file = e.NewValue as FileInfo;
            if (null != file)
            {
                var xpsDoc = new XpsDocument(file.FullName, System.IO.FileAccess.Read);
                documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabContent.IsSelected)
            {
                wrapPanel.Visibility = Visibility.Hidden;
                documentViewer.Visibility = Visibility.Visible;
            }
            else if (tabDesign.IsSelected)
            {
                wrapPanel.Visibility = Visibility.Visible;
                documentViewer.Visibility = Visibility.Hidden;
            }
        }

        private void TbNumOfBands_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbNumOfBands.Text))
            {
                Query.NumOfBands = 0;
                return;
            }

            uint numOfBands;
            if (!uint.TryParse(tbNumOfBands.Text, out numOfBands))
            {
                MessageBox.Show("请输入正整数");
            }
            else
            {
                Query.NumOfBands = numOfBands;
            }
        }

        private void rbCircular_Checked(object sender, RoutedEventArgs e)
        {
            lblCross.Visibility = Visibility.Visible;
            tbCross.Visibility = Visibility.Visible;
        }

        private void rbCircular_Unchecked(object sender, RoutedEventArgs e)
        {
            lblCross.Visibility = Visibility.Collapsed;
            tbCross.Visibility = Visibility.Collapsed;
        }
    }
}
