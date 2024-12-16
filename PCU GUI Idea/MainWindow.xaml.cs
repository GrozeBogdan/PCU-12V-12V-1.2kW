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
        public MainWindow()
        {
            InitializeComponent();
            CAN.Start_CAN();
            DbcParser.ParseDatabase();
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

    }
}
