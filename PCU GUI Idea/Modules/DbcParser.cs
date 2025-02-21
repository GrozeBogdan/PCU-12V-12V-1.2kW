using PCU_GUI_Idea;
using PCU_GUI_Idea.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Telerik.Charting;
using vxlapi_NET;
using static DbcParser;
using DataPoint = PCU_GUI_Idea.Modules.DataPoint;

public  class DbcParser
{
    private readonly static List<SolidColorBrush> AvailableColors = new List<SolidColorBrush>();
   

    public class Signal : INotifyPropertyChanged
    {
        private TextDecorationCollection dectoration = null;    
        public string Message { get; set; }
        public string Name { get; set; }
        public int StartBit { get; set; }
        public int EndBit { get; set; }
        public int Length { get; set; }
        public string DataType { get; set; }
        public double Offset { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
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
        // control interfata 

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class Message
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int DataLengthCode { get; set; }
        public string Sender { get; set; }
        public List<Signal> Signals { get; set; }
        public BitArray Data { get; set; }
        //dataa 64 biti
    }

    private static readonly Random _random = new Random();
    public static List<Message> Messages { get; private set; }
    public static XLClass.xl_event_collection sentEvents = new XLClass.xl_event_collection(0);
    public static XLClass.xl_event_collection receivedEvents = new XLClass.xl_event_collection(0);
    public DbcParser()
    {
        AvailableColors.AddRange(new SolidColorBrush[]
        {
                Brushes.Red,
                Brushes.Orange,
                Brushes.Gold,
                Brushes.GreenYellow,
                Brushes.Blue,
                Brushes.Indigo,
                Brushes.Chocolate,
                Brushes.Black,
                Brushes.Cyan,
                Brushes.LightPink
        });
    }

    public static void ParseDatabase(string database)
    {
        string dbcContents = File.ReadAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Database/" + database);
        ParseMessages(dbcContents);
        int i = 0;

        // First loop to find the 
        foreach (var message in DbcParser.Messages)
        {
            int signalsCount = 0;
            
            Console.WriteLine($"Message Name: {message.Name}, ID: {message.Id}, DLC: {message.DataLengthCode}, Sender: {message.Sender}");
            Console.WriteLine($"");
            foreach (var signal in message.Signals)
            {
                signalsCount++;
                Console.WriteLine($"    Signal Name: {signal.Name}, Start Bit: {signal.StartBit}, End Bit {signal.EndBit}, Length: {signal.Length}, Data Type: {signal.DataType}");
            }
            Console.WriteLine($"-----------------------------------------------------------------------------");
        }

        var messagesCount = DbcParser.Messages.GroupBy(meassage => meassage.Sender).ToDictionary(group => group.Key, group => group.Count()) ;
        sentEvents.messageCount = (uint)messagesCount["Canoe_Debug"];
        receivedEvents.messageCount = (uint)messagesCount["Vector__XXX"];

        foreach (var message in DbcParser.Messages)
        {
            if (message.Sender == "Canoe_Debug")
            {
                sentEvents.xlEvent.Add(new XLClass.xl_event());
                sentEvents.xlEvent[i].tagData.can_Msg.id = (uint)message.Id;
                sentEvents.xlEvent[i].tagData.can_Msg.dlc = (ushort)message.DataLengthCode;
                i++;
                if (i == sentEvents.messageCount)
                    i = 0;
            }
            if (message.Sender == "Vector__XXX")
            {
                receivedEvents.xlEvent.Add(new XLClass.xl_event());
                receivedEvents.xlEvent[i].tagData.can_Msg.id = (uint)message.Id;
                receivedEvents.xlEvent[i].tagData.can_Msg.dlc = (ushort)message.DataLengthCode;
                i++;
                if (i == receivedEvents.messageCount)
                    i = 0;
            }
        }
    }

    public static Brush GetRandomBrush(Message message)
    {
        if (message.Sender == "Vector__XXX")
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
            int randomIndex = _random.Next(AvailableColors.Count);
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
    public static Brush GetRandomBrush()
    { 
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            int randomIndex = _random.Next(properties.Length);
            Brush randomBrush = (Brush)properties[randomIndex].GetValue(null);
            if (randomBrush.Opacity != 1)
                randomBrush.Opacity = 1;
            return randomBrush;
    }
    public static ObservableCollection<DataPoint> CreateSignalDataChart(Message message)
    {
        if (message.Sender == "Vector__XXX")
            return new ObservableCollection<DataPoint>();
        else
            return null;
    }
    private static void ParseMessages(string dbcContents)
    {
        Messages = new List<Message>();
        const string messagePattern = @"BO_ (\d+) (\w+): (\d+) (\w+)";
        const string signalPattern = @"SG_ (\w+) : (\d+)\|(\d+)@([\d\.+|-]+)";

        MatchCollection messageMatches = Regex.Matches(dbcContents, messagePattern);

        foreach (Match messageMatch in messageMatches)
        {
            int id = int.Parse(messageMatch.Groups[1].Value);
            string name = messageMatch.Groups[2].Value;
            int dlc = int.Parse(messageMatch.Groups[3].Value);
            string sender = messageMatch.Groups[4].Value;

            Message message = new Message
            {
                Id = id,
                Name = name,
                DataLengthCode = dlc,
                Sender = sender,
                Data = new BitArray(64),
                Signals = new List<Signal>()
            };

            // Find the start and end index of the current message definition
            int startIndex = messageMatch.Index;
            int endIndex = messageMatch.NextMatch().Index;

            if (endIndex == 0)
            {
                endIndex = dbcContents.Length;
            }

            string messageText = dbcContents.Substring(startIndex, endIndex - startIndex);

            foreach (Match signalMatch in Regex.Matches(messageText, signalPattern))
            {
                string signalName = signalMatch.Groups[1].Value;
                int startBit = int.Parse(signalMatch.Groups[2].Value);
                int length = int.Parse(signalMatch.Groups[3].Value);
                //string dataType = signalMatch.Groups[4].Value;
                string dataType;
                if (signalMatch.Groups[4].Value == "1-")
                {
                    dataType = "Float";
                }
                else
                {
                    dataType = "Unsigned";
                }
                Signal signal = new Signal
                {
                    //from database
                    Message = message.Name,
                    Name = signalName,
                    StartBit = startBit,
                    EndBit = startBit + length - 1,
                    Length = length,
                    DataType = dataType,
                    
                    //from interface
                    SigColor = GetRandomBrush(message),
                    ChartData = CreateSignalDataChart(message),
                    TextDec = null
                };
                
                message.Signals.Add(signal);
            }

            Messages.Add(message);
        }
    }
}
