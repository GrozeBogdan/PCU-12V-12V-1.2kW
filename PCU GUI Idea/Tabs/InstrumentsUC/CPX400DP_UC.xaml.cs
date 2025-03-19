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
using DeviceControlLib;

namespace PCU_GUI_Idea.Tabs.InstrumentsUC
{
    /// <summary>
    /// Interaction logic for CPX400DP_UC.xaml
    /// </summary>
    public partial class CPX400DP_UC : UserControl
    {
        DeviceControlLib.IPowerSupply ps;
        public CPX400DP_UC(IDevice device)
        {
            InitializeComponent();
            this.ps = (IPowerSupply)device;
        }

        private void TurnOnSupply(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int temp = ps.GetSelectedOutput();
            ps.SetSelectedOutput(int.Parse(button.Name.Replace("supplyCH", "")));
            try
            {
                var value = ps.GetOutput();
                ps.SetOutput(!value);
                button.SetResourceReference(Button.BackgroundProperty, !value ? "PanelBackground" : "ButtonAndHighlightBackground");
                ps.SetSelectedOutput(temp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SendSupplyValue(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    TextBox textBox = sender as TextBox;
                    int temp = ps.GetSelectedOutput();

                    if (textBox.Name.Contains("1"))
                    {
                        ps.SetSelectedOutput(1);
                        ps.SetVoltage(double.Parse(supplyVoltageCH1.Text));
                        ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH1.Text));
                    }
                    if (textBox.Name.Contains("2"))
                    {
                        ps.SetSelectedOutput(2);
                        ps.SetVoltage(double.Parse(supplyVoltageCH2.Text));
                        ps.SetCurrentLimitPositive(double.Parse(supplyCurrentCH2.Text));
                    }
                    ps.SetSelectedOutput(temp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
    }
}
