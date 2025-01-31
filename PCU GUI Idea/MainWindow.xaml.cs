using ExCSS;
using PCU_GUI_Idea.Tabs;
using System;
using System.Collections.Generic;
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
        private Converter converter_UC = new Converter();
        public Graphics graphics_UC = new Graphics();
        private Help help_UC = new Help();

        private Dictionary<string, UserControl> controls = new Dictionary<string, UserControl> { };

        public MainWindow()
        {
            InitializeComponent();
            CAN.Start_CAN();
            DbcParser.ParseDatabase();

            customize_UC.InitializeComponent();
            converter_UC.InitializeComponent();
            graphics_UC.InitializeComponent();
            help_UC.InitializeComponent();

            controls.Add("CustomizeButton", customize_UC);
            controls.Add("ConverterButton", converter_UC);
            controls.Add("GraphicsButton", graphics_UC);
            controls.Add("HelpButton", help_UC);
        }

        // Asta trebuie sa fie tot codul pe care il am in MAIN!
        public void Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void Exit_Button(object sender, RoutedEventArgs e)
        {
            CAN.Stop_CAN();
            App.Current.Shutdown();
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
