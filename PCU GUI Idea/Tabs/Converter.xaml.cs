using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static List<UIElement> elements;
        public Converter()
        {
            InitializeComponent();
            //LoadXAMLFile("pcu_buck_boost_inductor_up", new Thickness(400, 50, 400, 50));
            //LoadXAMLFile("pcu_buck_boost_inductor_down", new Thickness(400, 0, 400, 550));
            LoadXAMLFile("pcu_buck_boost_converter", new Thickness(100, 50, 100, 150));
            //LoadXAMLFile("SchematicConv", new Thickness(0));
            this.Loaded += BindSignalToFrameworkElement;
        }
        private void BindSignalToFrameworkElement(object sender, RoutedEventArgs e)
        {
            // Now that the control is loaded, we can safely get all elements inside it
            var allElements = GetAllUIElements(gridContainingElements); // 'this' refers to the UserControl
            elements = allElements;
            // Do something with allElements
            foreach (var element in elements)
            {
                foreach(var message in DbcParser.Messages)
                {
                    foreach(var signal in message.Signals)
                    {
                        if (element is FrameworkElement frameworkElement && frameworkElement.Name.Contains(signal.Name))
                        {
                            signal.AssociatedElement = frameworkElement;
                            Debug.WriteLine("Message:" + message.Name + "  " + signal.Name);
                            Debug.WriteLine("UI Element: " + frameworkElement.Name);
                        }
                    }    
                }    
            }
        }
        public List<UIElement> GetAllUIElements(DependencyObject parent)
        {
            var elements = new List<UIElement>();
            var visitedElements = new HashSet<DependencyObject>(); // Keep track of visited elements to avoid duplicates

            if (parent == null) return elements;

            // Traverse logical tree and filter for UI elements (ignores non-UI elements)
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is UIElement uiElement && !visitedElements.Contains(child) && IsElementWanted(uiElement))
                {
                    elements.Add(uiElement);
                    visitedElements.Add((DependencyObject)child); // Mark this element as visited
                    elements.AddRange(GetAllUIElements(uiElement)); // Recursively add children
                }
            }

            // Traverse visual tree and filter for UI elements (exclude layout panels, etc.)
            if (parent is Visual visualParent)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visualParent); i++)
                {
                    var visualChild = VisualTreeHelper.GetChild(visualParent, i);
                    if (visualChild is UIElement uiElement && !visitedElements.Contains(visualChild) && IsElementWanted(uiElement)) // Filter out internal elements
                    {
                        elements.Add(uiElement);
                        visitedElements.Add(visualChild); // Mark this element as visited
                        elements.AddRange(GetAllUIElements(visualChild)); // Recursively add children
                    }
                }
            }

            return elements;
        }
        // Method to filter out unwanted internal elements (like ScrollBars, Rectangles, etc.)
        private bool IsElementWanted(UIElement element)
        {
            // Exclude unwanted types of elements that are part of the visual tree but not part of the UI you're interested in
            return !(element is ScrollBar ||
                     element is System.Windows.Shapes.Rectangle ||
                     element is ScrollContentPresenter ||
                     element is AdornerLayer ||
                     element is Ellipse ||
                     element is Grid ||
                     element is Border); // Add any other unwanted types here
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
            //if (sender is ToggleButton toggleButton)
            //{
            //    PH1_Curr.Text = toggleButton.IsChecked.Value.ToString();
            //}
        }

        public static void UpdateUI()
        {

        }
    }
}
