using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
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
using Svg;
using Telerik.Windows.Documents.Fixed.Model.Objects;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using Image = System.Windows.Controls.Image;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Converter.xaml
    /// </summary>
    public partial class Converter : UserControl
    {
        public Converter()
        {
            InitializeComponent();
            //LoadXAMLFile("pcu_buck_boost_inductor_up", new Thickness(400, 50, 400, 50));
            //LoadXAMLFile("pcu_buck_boost_inductor_down", new Thickness(400, 0, 400, 550));
            LoadXAMLFile("pcu_buck_boost_converter", new Thickness(100,50,100,150));
            //LoadXAMLFile("SchematicConv", new Thickness(0));
        }

        private void LoadXAMLFile(string fileName, Thickness margin)
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Pictures/SVG/" + fileName + ".xaml";
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Load the ResourceDictionary from the XAML file
                var resourceDictionary = (ResourceDictionary)XamlReader.Load(stream);

                // Retrieve the DrawingGroup by its key
                // If you placed a name in an app you are using, for ex: Adobe Ilustrator; You have to use the name you put as the key.
                var drawingGroup = (DrawingGroup)resourceDictionary["PCU_Layout"];

                // Wrap the DrawingGroup in a DrawingImage
                var drawingImage = new DrawingImage(drawingGroup);

                // Apply the DrawingImage to an Image control
                // You can adjust different properties of the image;
                var imageControl = new Image
                {
                    Margin = margin,
                    Source = drawingImage
                };

                // Add the generated image to the grid.
                convTab.Children.Add(imageControl);
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                PH1_Curr.Text = toggleButton.IsChecked.Value.ToString();
            }
        }


        public static void UpdateUI()
        {

        }
    }
}
