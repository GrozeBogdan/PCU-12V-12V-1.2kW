using PCU_GUI_Idea.Menu;
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
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Spreadsheet.Model;
using static ThemePresets;

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
            //themeBox.Text = ThemeManager.CurrentTheme.Name;
        }

        //private void IntializeOthersFromMemory(object sender, RoutedEventArgs e)
        //{
        //    if (_selectedDatabase != null) 
        //    {
        //        databaseBox.EmptyText = _selectedDatabase;
        //    }

        //    if (_cachedWorkbook != null) // First time, create it
        //    {
        //        excelTable.Workbook = _cachedWorkbook;
        //    }
        //}

        private void themeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var radComboBox = sender as RadComboBox;
            RadComboBoxItem item = radComboBox.SelectedItem as RadComboBoxItem; 
            if (radComboBox != null && item != null)
            {
                var theme = Themes[item.Content.ToString()];
                ThemeManager.ApplyTheme(theme);
               
            }
            else
                MessageBox.Show("Nu merge daca nu alegi nimic");
        }

        private void SearchForDatabases(object sender, EventArgs e)
        {
            string directory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database";
            foreach(string file in Directory.GetFiles(directory)) 
            {
                if (databaseBox.Items.Contains(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", "")) == false)
                {
                    if (file.Contains(".dbc"))
                    {
                        databaseBox.Items.Add(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", ""));
                    }
                }
            }
        }

        //private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    if (databaseBox.SelectedItem != null)
        //        _selectedDatabase = (string)databaseBox.SelectedItem;
        //    else
        //        _selectedDatabase = databaseBox.EmptyText;
        //    _cachedWorkbook = excelTable.Workbook;
        //}
    }
}
