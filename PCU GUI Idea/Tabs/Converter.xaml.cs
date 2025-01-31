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
using PCU_GUI_Idea.Modules;
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
        private bool _signalesBinded;
        public static List<UIElement> elements;
        public Converter()
        {
            InitializeComponent();
            InitizalizeImages();  
            this.Loaded += BindSignalToFrameworkElement;
        }

        /// <summary>
        /// Finds the FrameworkElement in the UC and binds it to the respective Signal
        /// </summary>
        /// 
        private void BindSignalToFrameworkElement(object sender, RoutedEventArgs e)
        {
            if(!_signalesBinded)
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
                _signalesBinded = true;
            }
        }
        /// <summary>
        /// Method used to find all UIElements
        /// </summary>
        /// 
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

        /// <summary>
        /// Method to filter out unwanted internal elements (like ScrollBars, Rectangles, etc.)
        /// </summary>
        /// <param name="element">The element to be checked</param>
        /// <returns>true if we want the element, false if we don't</returns>
        /// 

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

        /// <summary>
        /// Loads and XAML file that is a drawaing in a UIElement
        /// </summary>
        private void LoadXAMLFile(string fileName, string resourceKey, Thickness margin, Grid grid)
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Pictures/SVG/" + fileName + ".xaml";
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Load the ResourceDictionary from the XAML file
                var resourceDictionary = (ResourceDictionary)XamlReader.Load(stream);

                // Retrieve the DrawingGroup by its key
                // If you placed a name in an app you are using, for ex: Adobe Ilustrator; You have to use the name you put as the key.
                var drawingGroup = (DrawingGroup)resourceDictionary[resourceKey];

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
                grid.Children.Add(imageControl);
            }
        }
        /// <summary>
        /// Initializes the images used in the UC
        /// </summary>
        private void InitizalizeImages()
        {
            // Puting the converter in the UC
            LoadXAMLFile("pcu_buck_boost_converter", "PCU_Layout", new Thickness(100, 50, 100, 150), convTab);

            // Images coresponding for each Duty Cycle
            LoadXAMLFile("pwm_duty", "PWM_Duty", new Thickness(), pwm_duty);
            LoadXAMLFile("pwm_duty", "PWM_Duty", new Thickness(), pwm_duty2);
            LoadXAMLFile("pwm_duty", "PWM_Duty", new Thickness(), pwm_duty3);
            LoadXAMLFile("pwm_duty", "PWM_Duty", new Thickness(), pwm_duty4);

            // Images coresponding for each Frequency
            LoadXAMLFile("pwm_freq", "PWM_Freq", new Thickness(), pwm_freq);
            LoadXAMLFile("pwm_freq", "PWM_Freq", new Thickness(), pwm_freq2);
            LoadXAMLFile("pwm_freq", "PWM_Freq", new Thickness(), pwm_freq3);
            LoadXAMLFile("pwm_freq", "PWM_Freq", new Thickness(), pwm_freq4);
        }

        /// <summary>
        /// temp temp temp
        /// </summary>
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (sender is ToggleButton toggleButton)
            //{
            //    PH1_Curr.Text = toggleButton.IsChecked.Value.ToString();
            //}
        }

        /// <summary>
        /// Asserts the recieved data to the coressponding FrameworkElement / Chart
        /// </summary>
        public static void UpdateUI()
        {

        }
    }
}
