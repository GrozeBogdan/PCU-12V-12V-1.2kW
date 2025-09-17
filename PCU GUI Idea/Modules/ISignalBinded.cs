using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PCU_GUI_Idea.Modules
{
    public interface ISignalBindable
    {
        void ChangeSignalBinding(bool value);
        void UpdateUI(string name, double value, FrameworkElement element);

    }
}
