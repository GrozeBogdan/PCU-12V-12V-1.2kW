using DeviceControlLib;
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

namespace PCU_GUI_Idea.Tabs.InstrumentsUC
{
    /// <summary>
    /// Interaction logic for RP7945A_UC.xaml
    /// </summary>
    public partial class RP7945A_UC : UserControl
    {
        DeviceControlLib.IPowerSupply ps;
        Thread innerThread;

        public RP7945A_UC(IDevice device)
        {
            InitializeComponent();
            this.ps = (IPowerSupply)device;
            
            innerThread = new Thread(new ThreadStart(innerThread_InfoTransfer));
            innerThread.Start();
            Instruments.runningThreads.Add(innerThread);
        }

        private void innerThread_InfoTransfer()
        {
            while(innerThread.IsAlive)
            { 
                Dispatcher.Invoke(() =>
                {
                        if (ps != null)
                        {
                            ps_voltageCH1.Text = Math.Round(ps.GetVoltage(),5).ToString() + " V";
                            ps_currentCH1.Text = Math.Round(ps.GetCurrent(),5).ToString() + " A";
                            ps_power.Text = Math.Round(ps.GetPower(), 5).ToString() + "W";
                            //realEffic.Text = (Math.Round((double.Parse(loadPower2.Text) / double.Parse(supplyPower.Text) * 100), 3)).ToString();
                        }
                });
                Thread.Sleep(100);
            }
        }

        public void CloseThread()
        {
            innerThread.Abort();
            GC.Collect();
        }

        private void SendSupplyValue(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    ps.SetVoltage(double.Parse(supplyVoltageCH1.Text));
                    ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH1.Text));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void SetWorkMode(object sender, RoutedEventArgs e) 
        {
            Button button = sender as Button;
            try
            {
                ps.SetOperationMode((OperationModeEnum)int.Parse(button.Uid));
                foreach (Button controls in SupplyWorkMode.Children)
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

        private void TurnOnSupply(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                var value = ps.GetOutput();
                ps.SetOutput(!value);
                button.SetResourceReference(Button.BackgroundProperty, !value ? "PanelBackground" : "ButtonAndHighlightBackground");
                button.Content = !value ? "On" : "Off";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
