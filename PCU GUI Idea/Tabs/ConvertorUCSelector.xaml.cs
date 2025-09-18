using DeviceControlLib;
using Microsoft.Office.Interop.Excel;
using PCU_GUI_Idea.Modules;
using PCU_GUI_Idea.Modules.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml;
using Telerik.Windows.Controls;
using Application = System.Windows.Application;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    /// 

    public partial class ConvertorUCSelector : UserControl
    {
        private static readonly Dictionary<string, UserControl> _converterCache = new Dictionary<string, UserControl>();

        public ConvertorUCSelector()
        {
            InitializeComponent();
            this.Loaded += CheckCache;
        }

        public static void UpdateSignalBind(bool value)
        {
            // Assuming there's only one cached UC
            var uc = _converterCache.Values.FirstOrDefault();
            if (uc is ISignalBindable bindableUC)
            {
                bindableUC.ChangeSignalBinding(value);
            }
        }

        private void CheckCache(object sender, RoutedEventArgs e)
        {
            if (_converterCache.Count > 0 && System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                var control = _converterCache.Values.FirstOrDefault();
                if (control != null)
                {
                    mainWindow.tab.Content = control;
                }
            }
        }

        private string[] GetConverterUserControls()
        {
            string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            string folderPath = System.IO.Path.Combine(basePath, "Tabs", "Converter UserControls");
            string[] files = Directory.GetFiles(folderPath, "*.xaml");
            return files;
        }

        private void SearchForConverters(object sender, EventArgs e)
        {
            try
            {
                RadComboBox radComboBox = sender as RadComboBox;
                string[] files = GetConverterUserControls();
                foreach (string fileName in files)
                {
                    if (radComboBox.Items.Contains(System.IO.Path.GetFileNameWithoutExtension(fileName)))
                    {
                        break;
                    }
                    else
                        radComboBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadConverter(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if((DbcParser.Messages == null || !DbcParser.Messages.Any()) && (LdfParser.Frames == null || !LdfParser.Frames.Any()))
                {
                    MessageBox.Show("No COM database loaded. \n Go on Customize Tab to choose.");
                    databaseBox.Visibility = Visibility.Visible;
                    return;
                }
                
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                {
                    RadComboBox radComboBox = sender as RadComboBox;
                    string selectedItem = radComboBox.SelectedItem as string;

                    if (string.IsNullOrEmpty(selectedItem))
                        return;

                    // Check cache first
                    if (_converterCache.TryGetValue(selectedItem, out UserControl cachedControl))
                    {
                        mainWindow.tab.Content = cachedControl;
                        radComboBox.SelectedItem = null;
                        return;
                    }

                    // Dynamically resolve and instantiate the type by class name
                    string className = "PCU_GUI_Idea.Tabs.Converter_UserControls." + selectedItem;
                    Type type = Type.GetType(className);

                    if (type != null)
                    {
                        var control = (UserControl)Activator.CreateInstance(type);

                        // Save in cache for next time
                        _converterCache[selectedItem] = control;

                        mainWindow.tab.Content = control;

                        radComboBox.SelectedItem = null;
                    }
                    else
                    {
                        MessageBox.Show("Could not find converter: " + className);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchForDatabases(object sender, EventArgs e)
        {
            string directory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database";
            foreach (string file in Directory.GetFiles(directory))
            {
                if (databaseBox.Items.Contains(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", "")) == false)
                {
                    if (file.Contains(".dbc") || file.Contains(".ldf"))
                    {
                        databaseBox.Items.Add(file.Replace(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database\\", ""));
                    }
                }
            }
        }
        private async void LoadDatabase(object sender, SelectionChangedEventArgs e)
        {
            //Clear lists.
            if (DbcParser.Messages != null || LdfParser.Frames != null)
            {
                DbcParser.Messages.Clear();
                LdfParser.Frames.Clear();
            }

            //Load database
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            RadComboBox radComboBox = sender as RadComboBox;
            if (radComboBox.SelectedItem.ToString().Contains(".dbc"))
                DbcParser.ParseDatabase(radComboBox.SelectedItem.ToString());
            else
                LdfParser.ParseDatabase(radComboBox.SelectedItem.ToString());
            ConvertorUCSelector.UpdateSignalBind(true);

            feedBack.Visibility = Visibility.Visible;
            await Task.Delay(2000);

            LoadConverter(usercontrolSelectorComboBox, e);
        }

        //private void Load_Converter(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
        //        {
        //            RadComboBox radComboBox = sender as RadComboBox;
        //            string[] files = GetConverterUserControls();
        //            foreach (string item in radComboBox.Items)
        //            {
        //                if (item == radComboBox.SelectedItem)
        //                {
        //                    var filePath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Tabs\\Converter UserControls\\" + item + ".xaml";
        //                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //                    using (XmlReader reader = XmlReader.Create(fs))
        //                    {
        //                        var control = (UserControl)XamlReader.Load(reader);

        //                    // Put it into a placeholder ContentControl
        //                    mainWindow.tab.Content = control;
        //                    return;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
    }
}
