using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using DeviceControlLib;
using DeviceControlLib.TTi;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.FieldList;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Instruments.xaml
    /// </summary>
    public partial class Instruments : UserControl
    {
        private Thread InstrumentsThread;

        private bool instrumentsAreRunning;

        List<string> VendorIds;
        List<string> ModelList;
        List<string> ConnectedDevicesIds;
        List<string> ConnectedDevicesParsed = new List<string>();

        DeviceControlLib.IPowerSupply ps;
        DeviceControlLib.IElectronicLoad ld;

        private Dictionary<string, int> OperationMode = new Dictionary<string, int> 
        {
            {"CV", 0},
            {"CC", 1},
            {"CP", 2},
            {"CR", 3}
        };

        public Instruments()
        {
            InitializeComponent();
            InitializeInstruments();
        }

        private void InitializeInstruments()
        {
            var error = DeviceControlLib.VisaConnections.GetAllConnections(out ConnectedDevicesIds, out VendorIds, out ModelList);

            if (ConnectedDevicesIds.Count != VendorIds.Count || ConnectedDevicesIds.Count != ModelList.Count)
            {
                Console.WriteLine("Nu e ok");
                return;
            }

            for (int i = 0; i < ModelList.Count; i++)
            {
                ModelList[i] = ModelList[i].Replace(" ", "");   
            }

            if (ConnectedDevicesIds.Count <= 0) return;

            for (var i = 0; i < ConnectedDevicesIds.Count; i++)
            {
                var id = ConnectedDevicesIds[i];
                var vendor = VendorIds[i];
                var model = ModelList[i];

                var end = id.IndexOf("::INSTR", StringComparison.Ordinal);
                var shortId = id.Substring(end - 4, 4);

                var finalString = $"{model} | {shortId} ";

                ConnectedDevicesParsed.Add(finalString);
            }
        }
        public void CloseThread()
        {
            instrumentsAreRunning = false;
            if(InstrumentsThread != null) 
                InstrumentsThread.Abort();
            //GC.Collect();
        }
        private void Search_ModelList(object sender, EventArgs e)
        {
            RadComboBox radComboBox = sender as RadComboBox;
            if (ConnectedDevicesParsed == null)
                return;
            foreach(string model in ConnectedDevicesParsed)
            {
                if(radComboBox.Items.Contains(model))
                {
                    break;
                }
                else
                    radComboBox.Items.Add(model);
            }
        }
        private void Load_Instrument(object sender, SelectionChangedEventArgs e)
        {
            RadComboBox radComboBox = sender as RadComboBox;
            DeviceControlLib.IDevice device = DeviceFactory.CreateDevice(ModelList[radComboBox.SelectedIndex]);

            Error erdevice = device.Initialize(ConnectedDevicesIds[radComboBox.SelectedIndex]);
            // Exemplu powersupply in mod CV
            if (erdevice.IsOk)
            {
                if (device is DeviceControlLib.IPowerSupply powersupply)
                {
                    ps = (IPowerSupply)device;
                    MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");

                    powersupply.SetOperationMode(OperationModeEnum.CV_MODE);
                    //double voltage = 5; // 5V
                    //double current = 5;
                    //double currentlimit = 1; // 1A
                    //powersupply.SetCurrentLimitPositive(current);
                    //powersupply.SetVoltage(voltage);
                    //powersupply.SetCurrent(current);
                }

                else if (device is DeviceControlLib.IElectronicLoad load)
                {
                    ld = (IElectronicLoad)device;
                    MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");
                    load.SetOperationMode(OperationModeEnum.CC_MODE);
                    //double current = 0.5; // 0.5 amperi
                    //double voltagelimit = 16; // 16 volti
                    //load.SetOVP(voltagelimit);
                    //load.SetCurrent(current);
                }
            }

            if((ps != null && ps is IPowerSupply) &&  (ld != null && ld is IElectronicLoad))
            {
                if(instrumentsAreRunning)
                {

                }
                else
                {
                    InstrumentsThread = new Thread(new ThreadStart(InstrumentsInformationTransfer_Thread));
                    instrumentsAreRunning = true;
                    InstrumentsThread.Start();
                }
            }
        }
        private void InstrumentsInformationTransfer_Thread()
        {
            while (instrumentsAreRunning)
            {
                Dispatcher.Invoke(() =>
                {
                    //Load via VISA
                    if (ld != null)
                    {
                        ld_voltage.Text = ld.GetVoltage().ToString() + " V";
                        ld_current.Text = ld.GetCurrent().ToString() + " A";
                        ld_power.Text = ld.GetPower().ToString() + " W";
                        ld_resistance.Text = ld.GetResistance().ToString() + " Ω";

                    }
                    if(ps != null) 
                    {
                        ps_voltageCH1.Text = ps.GetVoltage().ToString() + " V";
                        ps_currentCH1.Text = ps.GetCurrent().ToString() + " A";

                        ps.SetSelectedOutput(2);

                        ps_voltageCH2.Text = ps.GetVoltage().ToString() + " V";
                        ps_currentCH2.Text = ps.GetCurrent().ToString() + " A";

                        ps.SetSelectedOutput(1);
                        //realEffic.Text = (Math.Round((double.Parse(loadPower2.Text) / double.Parse(supplyPower.Text) * 100), 3)).ToString();
                    }
                });
                Thread.Sleep(100);
            }
        }
        private void SetWorkMode(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                ld.SetOperationMode((OperationModeEnum)OperationMode[button.Content.ToString()]);
                foreach(Button controls in loadWorkMode.Children)
                {
                    if(controls == button)
                    {
                        button.SetResourceReference(Button.BackgroundProperty, "PanelBackground");
                    }
                    else
                        controls.ClearValue(Button.BackgroundProperty);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void TurnOnSupply(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
           // int temp = ps.GetSelectedOutput();
           // ps.SetSelectedOutput(int.Parse(button.Name.Replace("supplyCH", "")));
            try
            {
                var value = ps.GetOutput();
                ps.SetOutput(!value);
                button.SetResourceReference(Button.BackgroundProperty, !value ? "PanelBackground" : "ButtonAndHighlightBackground");
                // Revert back to old output, so to not interfere with the threads.
        //        ps.SetSelectedOutput(temp);
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
                button.Content = !value ? "On" : "Off" ;
                button.SetResourceReference(Button.BackgroundProperty, !value ? "PanelBackground" : "ButtonAndHighlightBackground");
            }
            catch(Exception ex )
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "Error");
            }
        }

        private void SendLoadValue(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SendLoadValue(sender, (RoutedEventArgs)e);
            }
        }

        private void SendSupplyValue(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    TextBox textBox = sender as TextBox;
                    //int temp = ps.GetSelectedOutput();

                    if (textBox.Name.Contains("1"))
                    {           
  //                      ps.SetSelectedOutput(1);
                        ps.SetVoltage(double.Parse(supplyVoltageCH1.Text));
                        ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH1.Text));
                    }
                    if (textBox.Name.Contains("2"))
                    {
 //                       ps.SetSelectedOutput(2);
                        ps.SetVoltage(double.Parse(supplyVoltageCH2.Text));
                        ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH2.Text));
                    }
                   // ps.SetSelectedOutput(temp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
    }
}
