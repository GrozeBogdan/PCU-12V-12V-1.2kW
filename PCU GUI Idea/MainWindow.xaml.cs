using ExCSS;
using PCU_GUI_Idea.Tabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace PCU_GUI_Idea
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Customize customize_UC = new Customize();
        public Converter converter_UC = new Converter();
        public Graphics graphics_UC = new Graphics();
        private Help help_UC = new Help();
        public Instruments instruments_UC = new Instruments();

        private Dictionary<string, UserControl> controls = new Dictionary<string, UserControl> { };

        public MainWindow()
        {
            InitializeComponent();
            DbcParser.ParseDatabase("CANdb12Vto12V.dbc");
            CAN.Initialize(this);
            CAN.Start_CAN();

            customize_UC.InitializeComponent();
            converter_UC.InitializeComponent();
            graphics_UC.InitializeComponent();
            help_UC.InitializeComponent();
            instruments_UC.InitializeComponent();
            AddSignalWindow.Initialize(this);

            controls.Add("CustomizeButton", customize_UC);
            controls.Add("ConverterButton", converter_UC);
            controls.Add("GraphicsButton", graphics_UC);
            controls.Add("HelpButton", help_UC);
            controls.Add("InstrumentsButton", instruments_UC);


            int tier = RenderCapability.Tier >> 16;
            Console.WriteLine("Rendering Tier: " + tier);
            // Added the UC for each device.
        }

        // Asta trebuie sa fie tot codul pe care il am in MAIN!
        public void Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void Exit_Button(object sender, RoutedEventArgs e)
        {
            foreach(Thread thread in Instruments.runningThreads)
            {
                thread.Abort();
            }
            CAN.Stop_CAN();
            App.Current.Shutdown();
            GC.Collect();
        }

        public void Minimize_Window(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;   
        }

        public void Maximize_Window(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                sWindow.CornerRadius = new CornerRadius(0);
                return;
            }
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                sWindow.CornerRadius = new CornerRadius(24);
                return;
            }
        }

        public UserControl GetControlByName(string controlName)
        {
            if (controls.ContainsKey(controlName))
            {
                return controls[controlName];
            }
            else MessageBox.Show($"Could not find UserControl: {controlName}");
            return null; // or throw exception if needed
        }
    }
}
