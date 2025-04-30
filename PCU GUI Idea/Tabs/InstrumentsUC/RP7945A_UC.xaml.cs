using DeviceControlLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        bool isLoadEnabled;
        DeviceControlLib.IPowerSupply ps;
        Thread innerThread;

        public string Voltage { get; set; }
        public string Current { get; set; }
        public string Power { get; set; }

        public RP7945A_UC(IDevice device)
        {
            InitializeComponent();
            this.ps = (IPowerSupply)device;
         
            
            innerThread = new Thread(new ThreadStart(InnerThread_InfoTransfer));
            innerThread.Priority = ThreadPriority.Lowest;
            innerThread.IsBackground = true;
            innerThread.Start();
            Instruments.runningThreads.Add(innerThread);
        }

        private void InnerThread_InfoTransfer()
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //stopwatch.Start();
            while (innerThread.IsAlive)
            {
                if (ps != null)
                {
                    Voltage = Math.Round(ps.GetVoltage(), 5).ToString() + " V";
                    Current = Math.Round(ps.GetCurrent(), 5).ToString() + " A";
                    Power = Math.Round(ps.GetPower(), 2).ToString() + "W";
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                    //    this.DataContext = null;
                    //    this.DataContext = this;
                          ps_voltageCH1.Text = Voltage;
                          ps_currentCH1.Text = Current;
                          ps_power.Text = Power;
                    });
                }
                Thread.Sleep(100);
                //stopwatch.Stop();
                //Console.WriteLine("Time elpased: " + stopwatch.ElapsedMilliseconds);
                //stopwatch.Restart();
            }

            //List<string> result = await Task.Run(() =>
            //{
            //    List<string> results = new List<string>();
            //    var dummy = Math.Round(ps.GetVoltage(), 5).ToString() + " V";
            //    var dummy1 = Math.Round(ps.GetCurrent(),5).ToString() + " A";
            //    var dummy2 = Math.Round(ps.GetPower(), 5).ToString() + "W";

            //    results.Add(dummy);
            //    results.Add(dummy1);
            //    results.Add(dummy2);

            //    return results;
            //});

            //ps_voltageCH1.Text = result[0];
            //ps_currentCH1.Text = result[1];
            //ps_power.Text = result[2];

        }

        public void CloseThread()
        {
            innerThread.Abort();
            GC.Collect();
        }

        public void SendSupplyValue2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isLoadEnabled == false)
                {
                    ps.SetVoltage(double.Parse(supplyVoltageCH1.Text));
                    ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH1.Text));
                }
                else if (isLoadEnabled == true)
                {
                    ps.SetVoltage(1);
                    ps.SetCurrentLimitPositive(0.25);
                    ps.SetCurrentLimitNegative(-double.Parse(supplyCurrentCH1.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public void SendSupplyValue(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if(isLoadEnabled == false)
                    {
                        ps.SetVoltage(double.Parse(supplyVoltageCH1.Text));
                        ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH1.Text));
                    }
                    else if(isLoadEnabled == true)
                    {
                        ps.SetVoltage(1);
                        ps.SetCurrentLimitPositive(0.25);
                        ps.SetCurrentLimitNegative(-double.Parse(supplyCurrentCH1.Text));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void SetSupplyOrLoad(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            try
            {
                foreach (UIElement controls in SupplyDirection.Children)
                {
                    if (controls == button)
                    {
                        button.SetResourceReference(Button.BackgroundProperty, "PanelBackground");
                        if (button.Content.ToString() == "Load")
                            isLoadEnabled = true;
                        else
                            isLoadEnabled = false;  
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
