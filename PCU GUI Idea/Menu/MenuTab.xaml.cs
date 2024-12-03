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

namespace PCU_GUI_Idea.Menu
{
    /// <summary>
    /// Interaction logic for MenuTab.xaml
    /// </summary>
    public partial class MenuTab : UserControl
    {
        public MenuTab()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Acts like a ToggleButton/Switch to ensure the connection between the respective tab and UC.
        /// </summary>
        /// 
        private void Menu_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            try
            {
               if(clickedButton != null) 
               {

               }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
    }
}
