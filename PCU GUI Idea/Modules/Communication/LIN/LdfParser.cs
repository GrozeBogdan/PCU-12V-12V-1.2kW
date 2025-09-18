using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Telerik.Windows.Controls.FieldList;
using Telerik.Windows.Documents.FormatProviders.Html.Parsing.Dom;
using vxlapi_NET;
using DataPoint = PCU_GUI_Idea.Modules.DataPoint;

public class LdfParser
{
    public static XLClass.xl_event_collection sentEvents = new XLClass.xl_event_collection(0);
    public static XLClass.xl_event_collection receivedEvents = new XLClass.xl_event_collection(0);
    public static Int32 Baudrate {  get; private set; }
    public static string Master {  get; private set; }
    public static string Slave { get; private set; }

    private readonly static List<SolidColorBrush> AvailableColors = new List<SolidColorBrush>();
    public class LinFrame : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public byte ID { get; set; }
        public string Sender { get; set; }
        public byte DLC { get; set; }
        public List<LinSignal> Signals { get; set; }
        public UInt16 CycleTime { get; set; }

        private byte[] _DataBytes { get; set; }

        public int[] Layout { get; set; }

        public LinFrame()
        {
            _DataBytes = new byte[8];
            Signals = new List<LinSignal>();
            Layout = new int[64];
        }
        public byte[] DataBytes
        {
            get
            { return _DataBytes; }
            set
            {
                _DataBytes = value;
                NotifyPropertyChanged("DataBytes");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
    public class LinSignal : INotifyPropertyChanged
    {
        private TextDecorationCollection dectoration = null;
        public string Name { get; set; }
        public int Length { get; set; }
        public int StartBit { get; set; }
        public string Subscriber { get; set; }
        public string Publisher { get; set; }
        public int Position { get; set; }
        public int InitialValue { get; set; }
        public string EncodingType { get; set; }
        public float EncodingValue { get; set; }
        public int[] Layout { get; set; }
        public float Value { get; set; }
        public Brush SigColor { get; set; }
        public FrameworkElement AssociatedElement { get; set; }
        public ObservableCollection<DataPoint> ChartData { get; set; }
        public TextDecorationCollection TextDec
        {
            get
            {
                return this.dectoration;
            }

            set
            {
                if (value != this.dectoration)
                {
                    this.dectoration = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static List<LinSignal> Signals = new List<LinSignal>();
    public static List<LinFrame> Frames { get; set; }

    public static List<Tuple<string, float>> EncodingList = new List<Tuple<string, float>>();
      
    static Random random = new Random();

    public static void ParseDatabase(string database)
    {
        string ldfContents = File.ReadAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database/" + database);
        ParseLdf(ldfContents);

        foreach(var frame in LdfParser.Frames)
        {
            int signalsCount = 0;

            if(frame.DLC > 0)
                Console.WriteLine($"Message Name: {frame.Name}, ID: {frame.ID}, DLC: {frame.DLC}, Sender: {frame.Sender}");
            else
                Console.WriteLine($"Message Name: {frame.Name}, ID: {frame.ID}");
            Console.WriteLine($"");
            foreach(var signal in frame.Signals)
            {
                signalsCount++;
                Console.WriteLine($"    Signal Name: {signal.Name}, Start Bit: {signal.StartBit},  Length: {signal.Length}");
            }
            Console.WriteLine($"-----------------------------------------------------------------------------");
        }

        var messagesCount = LdfParser.Frames.GroupBy(frame => frame.Sender).ToDictionary(group => group.Key, group => group.Count());
        receivedEvents.messageCount = (uint)messagesCount[LdfParser.Master];
        sentEvents.messageCount = (uint)messagesCount[LdfParser.Slave];

        Console.WriteLine($"-----------------------------------------------------------------------------");
        Console.WriteLine($"Master: {LdfParser.Master}, Slave: {LdfParser.Slave}, Baudrate: {LdfParser.Baudrate}"); 
        Console.WriteLine($"-----------------------------------------------------------------------------"); 
    }

    public static void ParseLdf(string ldfContents)
    {
        Frames = new List<LinFrame>();

        const string framePattern = @"^\s*(\w+)\s*:\s*(?:(0x[\da-fA-F]+|\d+)(?:,\s*([\w\-]+),\s*(\d+))?)?\s*\{";
        const string signalPattern = @"^\s*(\w+),\s*(\d+)\s*;";

        const string masterPattern = @"Master:\s*(\w+),\s*(\d+(?:\.\d+)?)\s*ms,\s*(\d+(?:\.\d+)?)\s*ms\s*;";
        const string slavePattern = @"Slaves:\s*([\w\s,]+)\s*;";
        const string speedPattern = @"LIN_speed\s*=\s*([\d.]+)\s*(k?bps)";

        Match slaveMatch = Regex.Matches(ldfContents, slavePattern)[0];
        Match masterMatch = Regex.Matches(ldfContents, masterPattern)[0];
        Match speedMatch = Regex.Matches(ldfContents, speedPattern)[0];

        LdfParser.Slave = slaveMatch.Groups[1].Value.Replace(" ","");
        LdfParser.Master = masterMatch.Groups[1].Value;
        var unit = speedMatch.Groups[2].Value;
        LdfParser.Baudrate = unit == "kbps" ? (Int32)(float.Parse(speedMatch.Groups[1].Value) * 1000) : Int32.Parse(speedMatch.Groups[1].Value);

        var frameMatches = Regex.Matches(ldfContents, framePattern, RegexOptions.Multiline);
        //const string frameSignalPattern = @"(\w+),\s*(\d+);";

        // Parse frames
        foreach (Match frameMatch in frameMatches)
        {
            string frameName = frameMatch.Groups[1].Value;
            byte id = Convert.ToByte(frameMatch.Groups[2].Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? frameMatch.Groups[2].Value.Substring(2) : frameMatch.Groups[2].Value,frameMatch.Groups[2].Value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? 16 : 10);
            string sender = frameMatch.Groups[3].Value;
            byte.TryParse(frameMatch.Groups[4].Value, out byte dlc);

            LinFrame frame = new LinFrame
            {
                Name = frameName,
                ID = id,
                Sender = sender,
                DLC = dlc,
                Signals = new List<LinSignal>()
            };

            int startIndex = frameMatch.Index + frameMatch.Length;
            int endIndex = frameMatch.NextMatch().Index;

            if (endIndex == 0)
            {
                endIndex = ldfContents.Length;
            }

            string frameText = ldfContents.Substring(startIndex, endIndex - startIndex);

            // Test variabale
            var signalMatches = Regex.Matches(frameText, signalPattern, RegexOptions.Multiline);

            // Parse signals inside frame
            foreach (Match sigMatch in Regex.Matches(frameText, signalPattern, RegexOptions.Multiline))
            {
                string sigName = sigMatch.Groups[1].Value;
                int startBit = int.Parse(sigMatch.Groups[2].Value);


                LinSignal signal = new LinSignal
                {
                    Name = sigName,
                    StartBit = startBit,

                    SigColor = GetRandomBrush(frame),
                    ChartData = CreateSignalDataChart(frame),
                    TextDec = null
                };

                frame.Signals.Add(signal);
            }

            // Edit the length after all signals are initialized
            for(int i = 0; i < frame.Signals.Count; i++)
            {
                if (i < frame.Signals.Count - 1)
                    frame.Signals[i].Length = frame.Signals[i + 1].StartBit - frame.Signals[i].StartBit;
                else
                    try
                    {
                        frame.Signals[i].Length = frame.Signals[i - 1].Length;
                    }
                    catch
                    {
                        continue;
                    }
            }

            Frames.Add(frame);
        }
    }
    public static Brush GetRandomBrush(LinFrame frame)
    {
        if (frame.Sender == "LIN_Commander")
        {
            if (AvailableColors.Count == 0)
            {
                // If all colors have been used, reset the available colors list
                AvailableColors.AddRange(new SolidColorBrush[]
                {
                Brushes.Red,
                Brushes.Orange,
                Brushes.Gold,
                Brushes.GreenYellow,
                Brushes.Blue,
                Brushes.White,
                Brushes.Chocolate,
                Brushes.Black,
                Brushes.Cyan,
                Brushes.LightPink,
                Brushes.AliceBlue,
                Brushes.Honeydew,
                Brushes.Khaki,
                Brushes.Magenta
                });
            }

            // Select a random index from the available colors
            int randomIndex = random.Next(AvailableColors.Count);
            SolidColorBrush randomBrush = AvailableColors[randomIndex];

            // Remove the selected color from the available colors to avoid repetition
            AvailableColors.RemoveAt(randomIndex);

            if (randomBrush.Opacity != 1)
                randomBrush.Opacity = 1;

            return randomBrush;
        }
        else
            return null;
    }
    public static ObservableCollection<DataPoint> CreateSignalDataChart(LinFrame frame)
    {
        if (frame.Sender == "LIN_Commander")
            return new ObservableCollection<DataPoint>();
        else
            return null;
    }
  
    public static LinFrame GetIndividualMsgByID(byte id)
    {
        foreach(LinFrame linFrame in Frames)
        {
            return linFrame.ID == id ? linFrame : null;
        }

        // This should not happen
        return null;
        // This should not happen
    }
}

