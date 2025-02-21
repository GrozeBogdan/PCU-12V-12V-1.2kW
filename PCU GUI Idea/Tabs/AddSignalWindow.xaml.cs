using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls.ChartView;
using static DbcParser;

namespace PCU_GUI_Idea
{
    public partial class AddSignalWindow : Window
    {
        public ObservableCollection<Signal> WindowMessages { get; } = new ObservableCollection<Signal>();
        //public ObservableCollection<Signal> Sig { get; set; } = new ObservableCollection<Signal>();

        private static MainWindow mainWindow;

        public AddSignalWindow()
        {
            InitializeComponent();
            DbcParser.ParseDatabase("CANdb12Vto12V.dbc");
            foreach (var message in DbcParser.Messages) 
            {
                if (message.Sender == "Vector__XXX")
                {
                    foreach (var signal in message.Signals)
                    {
                        if (signal.Name.Contains("OV") || signal.Name.Contains("OC"))
                            break;
                        WindowMessages.Add(signal);
                    }
                }
            }

            DataContext = this;
        }
        public static void Initialize(MainWindow window)
        {
            mainWindow = window;
        }
        private void Add_Signals(object sender, RoutedEventArgs e)
        {
            bool found = false;
            foreach (Signal item in radSignalComboBox.SelectedItems) 
            {
                foreach(var item1 in mainWindow.graphics_UC.Sig)
                {
                    if(item1.Name == item.Name)
                    {
                        found = true;
                    }
                }
                if(found == false)
                    mainWindow.graphics_UC.Sig.Add(item);
                found = false;
            }
            mainWindow.graphics_UC.MyListBox.ItemsSource = mainWindow.graphics_UC.Sig;

            foreach (LineSeries lineSeries in mainWindow.graphics_UC.chart.Series)
            {
                lineSeries.VerticalAxis.Visibility = Visibility.Collapsed;
                foreach (Signal signal in mainWindow.graphics_UC.MyListBox.Items)
                {
                    if (signal.Name == lineSeries.Name)
                    {
                        lineSeries.VerticalAxis.ElementBrush = signal.SigColor;
                        lineSeries.Stroke = signal.SigColor;
                        lineSeries.Visibility = Visibility.Visible;
                        lineSeries.VerticalAxis.Visibility = Visibility.Visible;
                    }
                }
            }

            //mainWindow.graphics_UC.MyListBox.ItemsSource = radSignalComboBox.SelectedItems;
            this.Close();
        }

        private void Exit_Button(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Maximize_Window(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void Minimize_Window(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
