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
using PCU_GUI_Idea.Tabs.InstrumentsUC;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.FieldList;

namespace PCU_GUI_Idea.Tabs
{
    /// <summary>
    /// Interaction logic for Instruments.xaml
    /// </summary>
    public partial class Instruments : UserControl
    {
        private Dictionary<string, Func<DeviceControlLib.IDevice, UserControl>> deviceControls = 
        new Dictionary<string, Func<DeviceControlLib.IDevice, UserControl>>()
        {
            { "CPX400DP", device => new CPX400DP_UC(device) },
            { "DL3031", device => new DL3031_UC(device) },
            { "RP7945A | 0128 ", device => new RP7945A_UC(device) },
            { "RP7945A | 0135 ", device => new RP7945A_UC(device) }
        };

        List<string> VendorIds;
        List<string> ModelList;
        List<string> ConnectedDevicesIds;
        List<string> ConnectedDevicesParsed = new List<string>();

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

            // So to clear any spaces inside
            for (int i = 0; i < ModelList.Count; i++)
            {
                ModelList[i] = ModelList[i].Replace(" ", "");   
            }

            if (ConnectedDevicesIds.Count <= 0) return;

            // In case u have multiple devices with same name ( case Keysight Supply....)
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
            if (erdevice.IsOk)
            {
                MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");
                if (radComboBox.Name == "deviceLeft_comboBox")
                {
                    deviceLeft.Content = deviceControls[ConnectedDevicesParsed[radComboBox.SelectedIndex]](device);
                }
                else if (radComboBox.Name == "deviceRight_comboBox")
                {
                    deviceRght.Content = deviceControls[ConnectedDevicesParsed[radComboBox.SelectedIndex]](device); 
                }
            }
            
        }


        // Astea le folosesti pentru TTI si Rigol, fa o versiune noua care sa mearga pentru toate tipurile de scule.
        //private void Load_Instrument(object sender, SelectionChangedEventArgs e)
        //{
        //    RadComboBox radComboBox = sender as RadComboBox;
        //    DeviceControlLib.IDevice device = DeviceFactory.CreateDevice(ModelList[radComboBox.SelectedIndex]);

        //    Error erdevice = device.Initialize(ConnectedDevicesIds[radComboBox.SelectedIndex]);
        //    Exemplu powersupply in mod CV
        //    if (erdevice.IsOk)
        //    {
        //        if (device is DeviceControlLib.IPowerSupply powersupply)
        //        {
        //            ps = (IPowerSupply)device;
        //            MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");

        //            powersupply.SetOperationMode(OperationModeEnum.CV_MODE);
        //            double voltage = 5; // 5V
        //            double current = 5;
        //            double currentlimit = 1; // 1A
        //            powersupply.SetCurrentLimitPositive(current);
        //            powersupply.SetVoltage(voltage);
        //            powersupply.SetCurrent(current);
        //        }

        //        else if (device is DeviceControlLib.IElectronicLoad load)
        //        {
        //            ld = (IElectronicLoad)device;
        //            MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");
        //            load.SetOperationMode(OperationModeEnum.CC_MODE);
        //            double current = 0.5; // 0.5 amperi
        //            double voltagelimit = 16; // 16 volti
        //            load.SetOVP(voltagelimit);
        //            load.SetCurrent(current);
        //        }
        //    }

        //    if ((ps != null && ps is IPowerSupply) && (ld != null && ld is IElectronicLoad))
        //    {
        //        if (instrumentsAreRunning)
        //        {

        //        }
        //        else
        //        {
        //            InstrumentsThread = new Thread(new ThreadStart(InstrumentsInformationTransfer_Thread));
        //            instrumentsAreRunning = true;
        //            InstrumentsThread.Start();
        //        }
        //    }
        //}
        //private void InstrumentsInformationTransfer_Thread()
        //{
        //    while (instrumentsAreRunning)
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            //Load via VISA
        //            if (ld != null)
        //            {
        //                ld_voltage.Text = ld.GetVoltage().ToString() + " V";
        //                ld_current.Text = ld.GetCurrent().ToString() + " A";
        //                ld_power.Text = ld.GetPower().ToString() + " W";
        //                ld_resistance.Text = ld.GetResistance().ToString() + " Ω";

        //            }
        //            if(ps != null) 
        //            {
        //                ps_voltageCH1.Text = ps.GetVoltage().ToString() + " V";
        //                ps_currentCH1.Text = ps.GetCurrent().ToString() + " A";

        //                ps.SetSelectedOutput(2);

        //                ps_voltageCH2.Text = ps.GetVoltage().ToString() + " V";
        //                ps_currentCH2.Text = ps.GetCurrent().ToString() + " A";

        //                ps.SetSelectedOutput(1);
        //                //realEffic.Text = (Math.Round((double.Parse(loadPower2.Text) / double.Parse(supplyPower.Text) * 100), 3)).ToString();
        //            }
        //        });
        //        Thread.Sleep(100);
        //    }
        //}

    }
}
