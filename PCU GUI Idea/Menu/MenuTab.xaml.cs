using ExCSS;
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
            try
            {
                Button clickedButton = sender as Button;
                if (clickedButton.Name is string controlName) 
                { 
                    LoadUserControl(controlName);
                }
                foreach(Button button in buttonTab.Children) 
                {
                    if (clickedButton.Name == button.Name)
                    {
                        // With this line the color remained even after i changed the theme.
                        //clickedButton.Background = (Brush)Application.Current.Resources["ButtonContrast"];

                        // Proposed solution for the problem.
                        // Extra: Try to understand how SetResourceReference works.
                        // On short: It is getting the resource dynamicaly, even if i change the Property from the ResourceDictonary it will change.

                        clickedButton.SetResourceReference(Button.BackgroundProperty, "ButtonContrast");
                    }
                    else
                    {
                        button.ClearValue(Button.BackgroundProperty);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the respective user control in the Window
        /// </summary>
        /// 
        private void LoadUserControl(string controlName)
        {
            // Get the MainWindow instance
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                try
                {
                    // Use reflection to find and instantiate the UserControl

                    // Check if the UserControl exists in the dictionary
                    var usercontrol = mainWindow.GetControlByName(controlName);
                    mainWindow.tab.Content = usercontrol;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading UserControl: {ex.Message}");
                }
            }
        }
    }
}
