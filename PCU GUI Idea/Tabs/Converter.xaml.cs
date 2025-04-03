using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
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
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixToolkit.Wpf;
using PCU_GUI_Idea.Modules;
using Telerik.Windows.Controls.FieldList;
using Telerik.Windows.Controls.Gauge;
using Telerik.Windows.Documents.Fixed.Model.Objects;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using vxlapi_NET;
using static DbcParser;
using Image = System.Windows.Controls.Image;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Converter.xaml
    /// </summary>
    public partial class Converter : UserControl
    {
        public static List<UIElement> elements;

        private enum WorkMode 
        { 
            BUCK_MODE,
            BUCK_BOOST_MODE,
            BOOST_MODE,
        };
        private Dictionary<WorkMode, (string, bool)> Madalina = new Dictionary<WorkMode, (string, bool)>
        {
            {WorkMode.BUCK_MODE, ("pcu_work_mode_buck" , true)},
            {WorkMode.BOOST_MODE, ("pcu_work_mode_boost", true)},
            {WorkMode.BUCK_BOOST_MODE, ("pcu_work_mode_buck_boost", false)}
        };
        private Dictionary<bool, string> ValueToDirection = new Dictionary<bool, string>
        {
            {false, "left"},
            {true, "right"}
        };
        //private Dictionary<WorkMode, (bool, string)> CorrespondingWorkMode = new Dictionary<WorkMode, (bool, string)>
        //{
        //    {WorkMode.BUCK_MODE,  (false, "in")},
        //    {WorkMode.BUCK_MODE,  (true,  "out")},
        //    {WorkMode.BOOST_MODE, (false, "out")},
        //    {WorkMode.BOOST_MODE, (true,  "in")}
        //};

        private bool _signalesBinded;
        public Converter()
        {
            InitializeComponent();
            InitizalizeImages();  
            this.Loaded += BindSignalToFrameworkElement;
            Load3DModel();
        }

        private void AppendSignalValue(byte[] binaryArray, int value, int startBit, int length)
        {
            for (int i = 0; i < length; i++)
            {
                int bit = (value >> i) & 1;
                int byteIndex = (startBit + i) / 8;
                int bitIndex = (startBit + i) % 8;
                binaryArray[byteIndex] |= (byte)(bit << bitIndex);
            }
        }

        private void AppendAndTransmitData(DbcParser.Message msg, byte[] bArr)
        {
            for (int i = 0; i < DbcParser.sentEvents.messageCount; i++)
            {
                if( msg.Id == DbcParser.sentEvents.xlEvent[i].tagData.can_Msg.id )
                {
                    DbcParser.sentEvents.xlEvent[i].tagData.can_Msg.data = bArr;
                    DbcParser.sentEvents.xlEvent[i].tag = XLDefine.XL_EventTags.XL_TRANSMIT_MSG;
                    CAN.CAND.XL_CanTransmit(CAN.portHandle, CAN.txMask, DbcParser.sentEvents.xlEvent[i]);
                }
            }
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
                                Debug.WriteLine("");
                                Debug.WriteLine("Message:" + message.Name + "  " + signal.Name);
                                Debug.WriteLine("UI Element: " + frameworkElement.Name);
                                Debug.WriteLine("Type: " + frameworkElement.GetType());
                                Debug.WriteLine("");
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

            if (parent is HelixViewport3D viewport3D)
            {
                foreach (var child in viewport3D.Children)
                {
                    if (child is Viewport2DVisual3D viewport)
                    {
                        elements.Add(viewport.Visual as TextBox);
                        //visitedElements.Add(uiElement);
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
        /// 

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
        /// 

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

            // Images coresponding for each Temp of the Halfbridge
            LoadXAMLFile("thermometer", "Thermo" , new Thickness(), thermometer_hb1);
            LoadXAMLFile("thermometer", "Thermo" , new Thickness(), thermometer_hb2);
            LoadXAMLFile("thermometer", "Thermo" , new Thickness(), thermometer_hb3);
            LoadXAMLFile("thermometer", "Thermo" , new Thickness(), thermometer_hb4);

            LoadXAMLFile("arrow", "Arrow", new Thickness(), directionLeft);
            LoadXAMLFile("arrow", "Arrow", new Thickness(), directionRight);
            
        }


        /// <summary>
        /// Method used to see which ToggleButton was triggered and 
        /// from which group to send the respective message with the signals
        /// corresponding to the ToggleButton.IsChecked.Value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton actioner = (ToggleButton)sender;
            CheckEnables(actioner);
            byte[] binaryArray = new byte[8];
            DbcParser.Message msg = new DbcParser.Message();

            foreach (UIElement element in gridContainingElements.Children)
            {
                if (element is ToggleButton toggle)
                {
                    foreach (var message in DbcParser.Messages)
                    {
                        if (toggle.Name.Contains(message.Name) && actioner.Name.Contains(message.Name))
                        {
                            foreach (var signal in message.Signals)
                            {
                                if (toggle.Name.Contains(signal.Name))
                                {
                                    AppendSignalValue(binaryArray, System.Convert.ToInt16(toggle.IsChecked.Value), signal.StartBit, signal.Length);
                                    break;
                                }
                            }
                            msg = message;
                            break;
                        }
                    }
                }
            }
            AppendAndTransmitData(msg, binaryArray);

            //A delay is needed to not overload the Slope action of changing the duty       
        }

        private void CheckEnables(ToggleButton toggle)
        {
           if(toggle.Name.Contains("trip_all"))
            {
                Enables_and_Readings_0x0D2_trip_ph1_in.Unchecked -= ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph1_out.Unchecked -= ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph2_in.Unchecked -= ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph2_out.Unchecked -= ToggleButton_Checked;

                Enables_and_Readings_0x0D2_trip_ph1_in.IsChecked = false;
                Enables_and_Readings_0x0D2_trip_ph1_out.IsChecked = false;
                Enables_and_Readings_0x0D2_trip_ph2_in.IsChecked = false;
                Enables_and_Readings_0x0D2_trip_ph2_out.IsChecked = false;

                Enables_and_Readings_0x0D2_trip_ph1_in.Unchecked += ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph1_out.Unchecked += ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph2_in.Unchecked += ToggleButton_Checked;
                Enables_and_Readings_0x0D2_trip_ph2_out.Unchecked += ToggleButton_Checked;
            }
        }

        private void Send_PWM_Values(object sender, KeyEventArgs e)
        {
            byte[] binaryArray = new byte[8];
            TextBox senderBox = sender as TextBox;
            if(e.Key == Key.Enter)
            {
                try
                {
                    List<TextBox> textBoxes = new List<TextBox>();
                    foreach (var message in DbcParser.Messages)
                    {
                        if (senderBox.Name.Contains(message.Name))
                        {
                            foreach (UIElement element in gridContainingElements.Children)
                            {
                                if (element is TextBox textbox && textbox.Name.Contains(senderBox.Name.Remove(0, senderBox.Name.Length - 10)))
                                {
                                    textBoxes.Add(textbox);
                                }
                            }

                            foreach (var signal in message.Signals)
                            {
                                foreach (var text in textBoxes)
                                {
                                    if (text.Name.Contains(signal.Name))
                                    {
                                        AppendSignalValue(binaryArray, System.Convert.ToInt16(text.Text), signal.StartBit, signal.Length);
                                    }
                                    if (signal.Name.Contains(text.Uid))
                                    {
                                        AppendSignalValue(binaryArray, 1, signal.StartBit, signal.Length);
                                    }
                                }
                            }

                            AppendAndTransmitData(message, binaryArray);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.FormatException)
                        MessageBox.Show("Please enter a value in the empty text box", "Invalid data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    else
                        MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Asserts the recieved data to the coressponding FrameworkElement / Chart
        /// </summary>
        /// 

        public void UpdateUI(string name, double value, FrameworkElement element)
        {
            if (element != null)
            {
                if(element is TextBox textbox)
                {
                    textbox.Text = value.ToString();
                }
                else if(element is BarIndicator barIndicator) 
                {
                    barIndicator.Value = Math.Min(value,120.0);
                }
            }
        }
        private void Load3DModel()
        {
            var stLReader = new StLReader();
            Model3D model = stLReader.Read("C:\\Users\\Bogdan\\Documents\\GitHub\\PCU-12V-12V-1.2kW\\PCU GUI Idea\\Pictures\\3D Models\\car-battery.stl");

            model = CenterAndScaleModel(model, 10.0); // Adjust size (5.0 is a good default scale)

            var material = new DiffuseMaterial((SolidColorBrush)FindResource("SVGBackground")); // Change to desired color
            ApplyMaterial(model, material);

            // Create a ModelVisual3D to hold the model
            ModelVisual3D modelVisual = new ModelVisual3D { Content = model };

            // Add the model to the Helix viewport
            battery_in.Children.Add(modelVisual);

            modelVisual = new ModelVisual3D { Content = model };
            battery_out.Children.Add(modelVisual);

            FitCameraToModel(model);
        }
        private Model3D CenterAndScaleModel(Model3D model, double desiredSize)
        {
            Rect3D bounds = model.Bounds;
            double maxDimension = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));

            // Calculate scale factor
            double scale = desiredSize / maxDimension;

            // Center model at (0,0,0)
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new ScaleTransform3D(scale, scale, scale));
            transformGroup.Children.Add(new TranslateTransform3D(
                -bounds.X - bounds.SizeX / 2,
                -bounds.Y - bounds.SizeY / 2,
                -bounds.Z - bounds.SizeZ / 2));

            model.Transform = transformGroup;
            return model;
        }
        private void FitCameraToModel(Model3D model)
        {
            Rect3D bounds = model.Bounds;
            double maxDimension = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));

            // Set camera distance based on model size
            double distance = maxDimension * 1.5; // Adjust multiplier if needed

            battery_in.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, distance/5, distance),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 50
            };

            battery_out.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, distance/5, distance),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 50
            };

            // Panned view Top-Left
            //battery_in.Camera = new PerspectiveCamera
            //{
            //    Position = new Point3D(-distance, distance, distance),
            //    LookDirection = new Vector3D(1, -1, -1),
            //    UpDirection = new Vector3D(0, 1, 0),
            //    FieldOfView = 50
            //};

            // Panned view Top-Right
            //battery_out.Camera = new PerspectiveCamera
            //{
            //    Position = new Point3D(distance, distance, distance),
            //    LookDirection = new Vector3D(-1, -1, -1),
            //    UpDirection = new Vector3D(0, 1, 0),
            //    FieldOfView = 50
            //};
        }
        public void ApplyMaterial(Model3D model, Material material)
        {
            if (model is GeometryModel3D geometryModel)
            {
                geometryModel.Material = material;
                geometryModel.BackMaterial = material; // Apply color to both sides
            }
            else if (model is Model3DGroup modelGroup)
            {
                foreach (Model3D child in modelGroup.Children)
                {
                    ApplyMaterial(child, material); // Recursively apply material
                }
            }
        }
        private void Clear_Box(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Clear();
        }
        private void Sync_Text_In_TextBox(object sender, TextChangedEventArgs e)
        {
            TextBox _senderTextBox = (TextBox)sender;
            foreach (UIElement element in gridContainingElements.Children)
            {
                if (element is TextBox tb)
                { 
                    if (tb.Name.Contains("phase") && _senderTextBox.Name.Contains("phase"))
                    {
                        tb.Text = _senderTextBox.Text;
                    }

                    if (tb.Name.Contains("frequency") && _senderTextBox.Name.Contains("frequency"))
                    {
                        tb.Text = _senderTextBox.Text;
                    }
                }
            }
        }

        private void ChangeWorkMode(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            var workModes = Enum.GetValues(typeof(WorkMode));
            foreach(var item in workModes)
            {
                if (slider.Value == (int)item)
                {
                    byte[] array = new byte[8];
                    if (DbcParser.Messages == null)
                        return;
                    foreach(var message in DbcParser.Messages)
                    {
                        if(slider.Name.Contains(message.Name))
                        {
                            foreach(var signal in message.Signals)
                            {

                                if (Madalina[(WorkMode)item].Item1 == signal.Name)
                                {
                                    AppendSignalValue(array, 1, signal.StartBit, signal.Length);
                                    WorkingMode_StateMachine_0x0D3_pcu_direction_.IsEnabled = Madalina[(WorkMode)item].Item2;
                                    CheckTextBoxes((WorkMode)item, WorkingMode_StateMachine_0x0D3_pcu_direction_.IsChecked.Value);
                                }
                                else if (signal.Name.Contains(ValueToDirection[WorkingMode_StateMachine_0x0D3_pcu_direction_.IsChecked.Value]) && (WorkMode)item != WorkMode.BUCK_BOOST_MODE)
                                {
                                    AppendSignalValue(array, WorkingMode_StateMachine_0x0D3_pcu_direction_.IsChecked.Value ? 1 : 1, signal.StartBit, signal.Length);
                                    CheckTextBoxes((WorkMode)item, WorkingMode_StateMachine_0x0D3_pcu_direction_.IsChecked.Value);
                                }
                                else
                                    AppendSignalValue(array, 0, signal.StartBit, signal.Length);
                            }
                            AppendAndTransmitData(message, array);
                            return;
                        }
                    }
                }
            }
        }

        // A different approach to write a function that sends a CAN MSG with the Signals i want..
        private void ChangeDirection(object sender, RoutedEventArgs e)
        {
            ToggleButton toggle = (ToggleButton)sender;
            byte[] array = new byte[8]; 

            Message message = DbcParser.FindMessage(toggle.Name);
            Signal directionSignal = DbcParser.FindSignal(message, ValueToDirection[toggle.IsChecked.Value]);
            Signal workModeSignal = DbcParser.FindSignal(message, Madalina[(WorkMode)WorkingMode_StateMachine_0x0D3_pcu_work_mode_.Value].Item1);

            CheckTextBoxes((WorkMode)WorkingMode_StateMachine_0x0D3_pcu_work_mode_.Value, toggle.IsChecked.Value);

            AppendSignalValue(array, 1, workModeSignal.StartBit, workModeSignal.Length);
            AppendSignalValue(array, 1, directionSignal.StartBit, directionSignal.Length);

            AppendAndTransmitData(message, array);
        }

        private void CheckTextBoxes(WorkMode workMode, bool direction)
        {
            string boxID = null;
            if ((workMode == WorkMode.BUCK_MODE && direction ==  false) || (workMode == WorkMode.BOOST_MODE && direction == true))
            {
                boxID = "in";
            }

            if ((workMode == WorkMode.BUCK_MODE && direction == true) || (workMode == WorkMode.BOOST_MODE && direction == false))
            {
                boxID = "out";
            }
            if (workMode == WorkMode.BUCK_BOOST_MODE)
                boxID = " ";

            foreach (UIElement element in gridContainingElements.Children)
            {
                if(element is TextBox textBox)
                {
                    if (textBox.Uid.Contains(boxID))
                    {
                        textBox.Opacity = 0.5;
                        textBox.IsEnabled = false;
                    }
                    else
                    {
                        textBox.Opacity = 1;
                        textBox.IsEnabled = true;
                    }
                }
            }
        }
    }
}
