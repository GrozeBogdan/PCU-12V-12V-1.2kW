using DeviceControlLib;
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

namespace PCU_GUI_Idea.Tabs.InstrumentsUC
{
    /// <summary>
    /// Interaction logic for DL3031_UC.xaml
    /// </summary>
    public partial class DL3031_UC : UserControl
    {
        DeviceControlLib.IElectronicLoad ld;
        private Dictionary<string, int> OperationMode = new Dictionary<string, int>
        {
            {"CV", 0},
            {"CC", 1},
            {"CP", 2},
            {"CR", 3}
        };

        public DL3031_UC(IDevice device)
        {
            InitializeComponent();
            this.ld = (IElectronicLoad)device;
        }
        private void SetWorkMode(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                ld.SetOperationMode((OperationModeEnum)OperationMode[button.Content.ToString()]);
                foreach (Button controls in loadWorkMode.Children)
                {
                    if (controls == button)
                    {
                        button.SetResourceReference(Button.BackgroundProperty, "PanelBackground");
                    }
                    else
                        controls.ClearValue(Button.BackgroundProperty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void TurnOnLoad(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                var value = ld.GetOutput();
                ld.SetOutput(!value);
                button.Content = !value ? "On" : "Off";
                button.SetResourceReference(Button.BackgroundProperty, !value ? "PanelBackground" : "ButtonAndHighlightBackground");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void LoadRampMode(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                foreach (Button button in loadWorkMode.Children)
                {
                    if (button.Content.ToString() == "CC" && button.Background == Application.Current.Resources["PanelBackground"] && ld != null)
                        ld.SetCurrent<int>(double.Parse(loadRampStart.Text), double.Parse(loadRampEnd.Text), int.Parse(loadRampEnd.Text), _ => { }, Array.Empty<int>());

                    //  Works but needs to be changed the Ramp method in the library.
                    //ld.SetCurrent<int>(double.Parse(loadRampStart.Text), double.Parse(loadRampEnd.Text), 10, args => Thread.Sleep(args.Length > 0 ? args[0] : 0), 100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
        private void SendLoadValue(object sender, RoutedEventArgs e)
        {
            //foreach (Button btn in loadWorkMode.Children)
            //{
            //    if(btn.Background == Application.Current.Resources["PanelBackground"])
            //    {
            //        var item = ld.GetWorkingMode();
            //    }
            //}

            //Bullshit incomming
            try
            {
                foreach (Button btn in loadWorkMode.Children)
                {
                    if (btn.Background == Application.Current.Resources["PanelBackground"])
                    {
                        if (btn.Content.ToString() == "CC")
                        {
                            ld.SetCurrent(double.Parse(loadValue.Text));
                            return;
                        }
                        if (btn.Content.ToString() == "CV")
                        {
                            ld.SetVoltage(double.Parse(loadValue.Text));
                            return;
                        }
                        if (btn.Content.ToString() == "CR")
                        {
                            ld.SetResistance(double.Parse(loadValue.Text));
                            return;
                        }
                        if (btn.Content.ToString() == "CP")
                        {
                            ld.SetPower(double.Parse(loadValue.Text));
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Error");
            }
        }
        private void SendLoadValue(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendLoadValue(sender, (RoutedEventArgs)e);
            }
        }
    }
}
