using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Graphics.xaml
    /// </summary>
    public partial class Graphics : UserControl
    {
        public Graphics()
        {
            InitializeComponent();
            LoadXAMLImages("length", "Length");
            LoadXAMLImages("length-vertical", "Length_Vertical");
            LoadXAMLImages("3d-cube", "ThreeD_Cube");
            LoadXAMLImages("pie-chart", "Pie_Chart");
            LoadXAMLImages("diagram", "Chart");
        }

        private void LoadXAMLImages(string fileName, string dictionaryKey)
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Pictures/SVG/" + fileName + ".xaml";
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Load the ResourceDictionary from the XAML file
                var resourceDictionary = (ResourceDictionary)XamlReader.Load(stream);

                // Retrieve the DrawingGroup by its key
                // If you placed a name in an app you are using, for ex: Adobe Ilustrator; You have to use the name you put as the key.
                var drawingGroup = (DrawingGroup)resourceDictionary[dictionaryKey];

                // Wrap the DrawingGroup in a DrawingImage
                var drawingImage = new DrawingImage(drawingGroup);

                // Apply the DrawingImage to an Image control
                // You can adjust different properties of the image;
                var imageControl = new Image
                {
                    Source = drawingImage
                };

                if (fileName == "length")
                    horizontal_Length.Children.Add(imageControl);

                else if (fileName.Contains("vertical"))
                    vertical_Length.Children.Add(imageControl);

                foreach(Button button in chart_Type_Button_Panel.Children)
                {
                    if(fileName.Contains(button.Name))
                    {
                        ControlTemplate template = new ControlTemplate(typeof(Button));
                        FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
                        FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));

                        // Set Image properties
                        imageFactory.SetValue(Image.SourceProperty, drawingImage);
                        // imageFactory.SetValue(Image.WidthProperty, 50.0);
                        // imageFactory.SetValue(Image.HeightProperty, 50.0);

                        // Add Image to Grid
                        gridFactory.AppendChild(imageFactory);
                        template.VisualTree = gridFactory;

                        // Apply Template to Button
                        button.Template = template;
                    }
                }
            }
        }
    }
}
