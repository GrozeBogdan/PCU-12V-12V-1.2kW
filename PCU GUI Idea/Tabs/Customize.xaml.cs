using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static ThemePresets;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Customize.xaml
    /// </summary>
    public partial class Customize : UserControl
    {
        List<Theme> Themes = new List<Theme>();
        public Customize()
        {
            
            Themes.Add(LightTheme);
            Themes.Add(DarkTheme);
            Themes.Add(Sunset);
            Themes.Add(Chill);

            InitializeComponent();            
        }

        private void databaseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ChangeTheme(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.Text = ThemePresets.Sunset.Name;  
                ThemeManager.ApplyTheme(theme);
            }
        }
    }
}
