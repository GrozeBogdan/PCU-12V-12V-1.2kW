using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCU_GUI_Idea.Modules
{
    public class DataPoint
    {
        public string Timestamp { get; set; }
        public double Value { get; set; }

        public DataPoint(string timestamp, double value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
