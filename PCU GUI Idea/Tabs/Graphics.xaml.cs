using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using static DbcParser;
using static LdfParser;
using DataPoint = PCU_GUI_Idea.Modules.DataPoint;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Graphics.xaml
    /// </summary>
    /// 
    public partial class Graphics : UserControl
    {
        public ObservableCollection<Signal> Sig { get; set; } = new ObservableCollection<Signal>();

        public Graphics()
        {
            InitializeComponent();
            LoadXAMLImages("length", "Length");
            LoadXAMLImages("length-vertical", "Length_Vertical");
            LoadXAMLImages("3d-cube", "ThreeD_Cube");
            LoadXAMLImages("pie-chart", "Pie_Chart");
            LoadXAMLImages("diagram", "Chart");

            DataContext = this;

        }

        private void LoadXAMLImages(string fileName, string dictionaryKey)
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Pictures/SVG/" + fileName + ".xaml";
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Load the ResourceDictionary from the XAML file
                var resourceDictionary = (ResourceDictionary)XamlReader.Load(stream);

                // Retrieve the DrawingGroup by its key
                // If you placed a name in an app you are using, for ex: Adobe Ilustrator; You have to use the name you put as the key.
                var drawingGroup = (DrawingGroup)resourceDictionary[dictionaryKey];

                // Wrap the DrawingGroup in a DrawingImage
                var drawingImage = new DrawingImage(drawingGroup);

                // Apply the DrawingImage to an Image control
                // You can adjust different properties of the image;
                var imageControl = new Image
                {
                    Source = drawingImage
                };

                if (fileName == "length")
                    horizontal_Length.Children.Add(imageControl);

                else if (fileName.Contains("vertical"))
                    vertical_Length.Children.Add(imageControl);

                foreach (Button button in chart_Type_Button_Panel.Children)
                {
                    if (fileName.Contains(button.Name))
                    {
                        ControlTemplate template = new ControlTemplate(typeof(Button));
                        FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
                        FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));

                        // Set Image properties
                        imageFactory.SetValue(Image.SourceProperty, drawingImage);
                        // imageFactory.SetValue(Image.WidthProperty, 50.0);
                        // imageFactory.SetValue(Image.HeightProperty, 50.0);

                        // Add Image to Grid
                        gridFactory.AppendChild(imageFactory);
                        template.VisualTree = gridFactory;

                        // Apply Template to Button
                        button.Template = template;
                    }
                }
            }
        }

        private void OpenSignalTab(object sender, RoutedEventArgs e)
        {
            AddSignalWindow addItemWindow = new AddSignalWindow
            {
                Owner = Application.Current.MainWindow
            };

            addItemWindow.ShowDialog();
        }

        private void AutoscaleEnable(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (button.IsChecked == true)
            {
                signalMinVertical.Opacity = 0.2;
                signalMaxVertical.Opacity = 0.2;
                signalMinVertical.IsEnabled = false;
                signalMaxVertical.IsEnabled = false;
                foreach (LineSeries series in chart.Series)
                {
                    if (series.Name == signalChartEdit.Text)
                    {
                        (series.VerticalAxis as LinearAxis).Minimum = double.NaN;
                        (series.VerticalAxis as LinearAxis).Maximum = double.NaN;
                        (series.VerticalAxis as LinearAxis).MajorStep = double.NaN;
                    }
                }
                button.SetResourceReference(Button.BackgroundProperty, "ButtonContrast");
            }
            else
            {
                signalMinVertical.Opacity = 1;
                signalMaxVertical.Opacity = 1;
                signalMinVertical.IsEnabled = true;
                signalMaxVertical.IsEnabled = true;
                foreach (LineSeries series in chart.Series)
                {
                    if (series.Name == signalChartEdit.Text)
                    {
                        (series.VerticalAxis as LinearAxis).Minimum = double.Parse(signalMinVertical.Text);
                        (series.VerticalAxis as LinearAxis).Maximum = double.Parse(signalMaxVertical.Text);
                        (series.VerticalAxis as LinearAxis).MajorStep = (double.Parse(signalMaxVertical.Text) - double.Parse(signalMinVertical.Text)) / 5;
                    }
                }
                button.ClearValue(Button.BackgroundProperty);
            }
        }

        private void RemoveSignal_Click(object sender, RoutedEventArgs e)
        {
            foreach (Signal signal in MyListBox.Items)
            {
                foreach (LineSeries lineSeries in chart.Series)
                {
                    if (signal.Name == lineSeries.Name && signal == (Signal)MyListBox.SelectedItem)
                    {
                        lineSeries.Visibility = Visibility.Collapsed;
                        lineSeries.VerticalAxis.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (MyListBox.SelectedItem != null)
                Sig.Remove((Signal)MyListBox.SelectedItem);
            else
                MessageBox.Show("Please select a signal from the list to remove.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HideShow_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (MyListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a signal from the list to hide/show.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (button.Name == "show")
            {
                var item = MyListBox.SelectedItem as Signal;
                item.TextDec = null;
                foreach (LineSeries lineSeries in chart.Series)
                {
                    if (lineSeries.Name == item.Name)
                    {
                        lineSeries.Visibility = Visibility.Visible;
                        lineSeries.VerticalAxis.Visibility = Visibility.Visible;
                    }
                }
            }
            if (button.Name == "hide")
            {

                var item = MyListBox.SelectedItem as Signal;
                item.TextDec = TextDecorations.Strikethrough;
                foreach (LineSeries lineSeries in chart.Series)
                {
                    if (lineSeries.Name == item.Name)
                    {
                        lineSeries.Visibility = Visibility.Collapsed;
                        lineSeries.VerticalAxis.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SignalItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = sender as MenuItem;
            var item = MyListBox.SelectedItem as Signal;
            foreach (LineSeries series in chart.Series)
            {
                if (series.Name == item.Name)
                {
                    if (menuitem.Header.ToString().Contains("left"))
                    {
                        series.VerticalAxis.HorizontalLocation = AxisHorizontalLocation.Left;
                        break;
                    }
                    if (menuitem.Header.ToString().Contains("right"))
                    {
                        series.VerticalAxis.HorizontalLocation = AxisHorizontalLocation.Right;
                        break;
                    }
                }
            }
        }

        public void Edit_Signal_Click(object sender, RoutedEventArgs e)
        {
            var item = MyListBox.SelectedItem as Signal;
            if (item != null)
            {
                signalChartEdit.Text = item.Name;
                signalChartEdit.Foreground = item.SigColor;
                foreach (LineSeries series in chart.Series)
                {
                    if (series.Name == item.Name)
                    {
                        signalMinVertical.Text = (series.VerticalAxis as LinearAxis).Minimum.ToString();
                        signalMaxVertical.Text = (series.VerticalAxis as LinearAxis).Maximum.ToString();
                    }
                }
            }
        }

        private void SignalList_ViewEnable(object sender, RoutedEventArgs e)
        {
            ToggleButton toggle = (ToggleButton)sender;
            if ((bool)toggle.IsChecked)
            {
                MyPopup.IsOpen = true;
                toggle.SetResourceReference(Button.BackgroundProperty, "ButtonContrast");
            }
            else
            {
                MyPopup.IsOpen = false;
                toggle.ClearValue(Button.BackgroundProperty);
            }
        }

        private void ShowChart(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "diagram")
            {
                chart.Visibility = Visibility.Visible;
                cube_chart.Visibility = Visibility.Collapsed;
            }
            else if (button.Name == "cube")
            {
                chart.Visibility = Visibility.Collapsed;
                cube_chart.Visibility = Visibility.Visible;
            }
        }

        public DataPoint GenerateDataPoint(double someValue)
        {
            return new DataPoint(DateTime.Now.ToString("HH:mm:ss:F", CultureInfo.InvariantCulture), someValue);
        }


        public void UpdateChart(object signal, double value)
        {
            if(chart.Visibility == Visibility.Visible) 
            { 
                foreach (LineSeries lineSeries in chart.Series)
                {
                    if( signal is Signal canSig)
                    { 
                        if (canSig.Name == lineSeries.Name && canSig.ChartData != null)
                        {
                            var newDataPoint = GenerateDataPoint(value);

                            canSig.ChartData.Add(newDataPoint);

                            if (canSig.ChartData.Count > 10)
                            {
                                canSig.ChartData.RemoveAt(0);
                            }

                            lineSeries.ItemsSource = canSig.ChartData;
                            chart.DataContext = this;
                        }
                    }

                    if (signal is LinSignal linSig)
                    {
                        if (linSig.Name == lineSeries.Name && linSig.ChartData != null)
                        {
                            var newDataPoint = GenerateDataPoint(value);

                            linSig.ChartData.Add(newDataPoint);

                            if (linSig.ChartData.Count > 10)
                            {
                                linSig.ChartData.RemoveAt(0);
                            }

                            lineSeries.ItemsSource = linSig.ChartData;
                            chart.DataContext = this;
                        }
                    }
                }
            }
        }
    }
}
