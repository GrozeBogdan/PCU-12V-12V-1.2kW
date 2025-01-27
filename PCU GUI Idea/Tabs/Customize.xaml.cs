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

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Customize.xaml
    /// </summary>
    public partial class Customize : UserControl
    {
        public Customize()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.ApplyTheme(ThemePresets.Chill);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            ThemeManager.ApplyTheme(ThemePresets.OlaMamiCheSamset);
        }
    }
}
