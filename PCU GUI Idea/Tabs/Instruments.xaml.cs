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
using Telerik.Windows.Controls;

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

        DeviceControlLib.IDevice ps;
        DeviceControlLib.IDevice ld;

        Dictionary<string, string> ModelAndId = new Dictionary<string, string> { };

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
                if (ModelList[i].Contains("CPX400DP"))
                {
                    ModelList[i] = ModelList[i].Replace(" ", "");
                }
                ModelAndId.Add(ModelList[i], ConnectedDevicesIds[i]);
            }

            if (ConnectedDevicesIds.Count <= 0) return;
        }
        private void Search_ModelList(object sender, EventArgs e)
        {
            RadComboBox radComboBox = sender as RadComboBox;
            foreach(string model in ModelList)
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
            DeviceControlLib.IDevice device = DeviceFactory.CreateDevice(radComboBox.Text);

            Error erdevice = device.Initialize(ModelAndId[radComboBox.Text]);
            // Exemplu powersupply in mod CV
            if (erdevice.IsOk)
            {
                if (device is DeviceControlLib.IPowerSupply powersupply)
                {
                    ps = device;
                    MessageBox.Show(radComboBox.Text + " succesfully connected!", "Instrumentation");

                    powersupply.SetOperationMode(OperationModeEnum.CV_MODE);
                    double voltage = 5; // 5V
                    double current = 5;
                    //double currentlimit = 1; // 1A
                    powersupply.SetCurrentLimitPositive(current);
                    powersupply.SetVoltage(voltage);
                    //powersupply.SetCurrent(current);
                }

                else if (device is DeviceControlLib.IElectronicLoad load)
                {
                    ld = device;
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
                        ld_voltage.Text = ld.GetVoltage().ToString();
                        ld_current.Text = ld.GetCurrent().ToString();
                        ld_power.Text = ld.GetPower().ToString();
                        //ld_resistance.Text = ld.GetResistance().ToString();

                    }
                    if(ps != null) 
                    {
                        ps_voltageCH1.Text = ps.GetVoltage().ToString();
                        ps_currentCH1.Text = ps.GetCurrent().ToString();                      


                        //realEffic.Text = (Math.Round((double.Parse(loadPower2.Text) / double.Parse(supplyPower.Text) * 100), 3)).ToString();
                    }
                });
                Thread.Sleep(100);
            }
        }
    }
}
