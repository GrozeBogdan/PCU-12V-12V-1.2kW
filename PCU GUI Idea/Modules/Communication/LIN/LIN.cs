using System;
using System.Runtime.InteropServices;
using System.Threading;
using vxlapi_NET;
using System.Linq;
using PCU_GUI_Idea.Modules;
using PCU_GUI_Idea;
using static LdfParser;
using System.Windows;
using PCU_GUI_Idea.Modules.Interfaces;

public static class LIN
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        private static XLDriver LIND = new XLDriver();
        private static String appName = "PCU_DC_DC";

        private static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        private static XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_NONE;
        private static XLClass.xl_linStatPar linStatusParams = new XLClass.xl_linStatPar();
        private static uint hwIndex = 0;
        private static uint hwChannel = 0;
        private static int portHandle = -1;
        private static int eventHandle = -1;

        // RX thread
        private static UInt64 accessMaskMaster = 0;
        private static UInt64 permissionMask = 0;
        private static int channelIndex = 0;

        public static Thread rxThread;

        private static MainWindow mainWindowInstance;
        private static ISignalBindable activeUC;

        public static void Initialize(MainWindow mainWindow)
        {
            mainWindowInstance = mainWindow;
            activeUC = mainWindow.tab.Content as ISignalBindable;
        }

        [STAThread]

        public static void Start_LIN()
        {
            XLDefine.XL_Status status;

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("                       xlLINdemo Single C# V9.7                    ");
            Console.WriteLine("Copyright (c) 2016 by Vector Informatik GmbH.  All rights reserved.");
            Console.WriteLine("-------------------------------------------------------------------\n");

            status = LIND.XL_OpenDriver();
            Console.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            status = LIND.XL_GetDriverConfig(ref driverConfig);
            Console.WriteLine("Get Driver Config : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            Console.WriteLine("DLL Version       : " + LIND.VersionToString(driverConfig.dllVersion));

            Console.WriteLine("Channels found    : " + driverConfig.channelCount);

            for (int i = 0; i < driverConfig.channelCount; i++)
            {
                Console.WriteLine("\n                   [{0}] " + driverConfig.channel[i].name, i);
                Console.WriteLine("                    - Channel Mask    : " + driverConfig.channel[i].channelMask);
                Console.WriteLine("                    - Transceiver Name: " + driverConfig.channel[i].transceiverName);
                Console.WriteLine("                    - Serial Number   : " + driverConfig.channel[i].serialNumber);
            }

            if ((LIND.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                LIND.XL_SetApplConfig(appName, 0, 0, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN);
                PrintAssignError();
            }

            else
            {
                LIND.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN);

                if (hwType == XLDefine.XL_HardwareType.XL_HWTYPE_NONE) PrintAssignError();

                accessMaskMaster = LIND.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
                PrintConfig();
                permissionMask = accessMaskMaster;

                status = LIND.XL_OpenPort(ref portHandle, appName, accessMaskMaster, ref permissionMask, 256, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V3, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN);
                Console.WriteLine("\n\nOpen Port                      : " + status);
                if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

                XLClass.xl_linStatPar linStatPar = new XLClass.xl_linStatPar
                {
                    baudrate = LdfParser.Baudrate,
                    LINMode = XLDefine.XL_LIN_Mode.XL_LIN_MASTER,
                    LINVersion = XLDefine.XL_LIN_Version.XL_LIN_VERSION_2_1
                };

                status = LIND.XL_LinSetChannelParams(portHandle, accessMaskMaster, linStatPar);
                Console.WriteLine("\nSet Channel Parameters (MASTER): " + status);
                if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

                // ????  EDIT: modificata ca am vzt ca trb mai jos
                byte[] DLC = new byte[64];
                int i = 0;
                foreach(LinFrame linFrame in LdfParser.Frames)
                {
                    i++;   
                    if (linFrame != null)
                    {
                        if (linFrame.Sender != LdfParser.Master)
                        {
                            DLC[i] = linFrame.DLC;
                        }
                    }
                }
                i = 0;

                status = LIND.XL_LinSetDLC(portHandle, accessMaskMaster, DLC);
                Console.WriteLine("Set DLC                        : " + status);
                if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

                status = LIND.XL_ActivateChannel(portHandle, accessMaskMaster, XLDefine.XL_BusTypes.XL_BUS_TYPE_LIN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);
                Console.WriteLine("Activate Channel               : " + status);
                if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

                status = LIND.XL_SetNotification(portHandle, ref eventHandle, 1);
                Console.WriteLine("\nSet Notification               : " + status);
                if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

                if (status != XLDefine.XL_Status.XL_SUCCESS)
                {
                    // This should happen when its not ok to turn on the rxThread
                    return;
                }

                rxThread = new Thread(new ThreadStart(RXThread));
                rxThread.Start();
            }
        }

        private static int PrintFunctionError()
        {
            Console.WriteLine("\nERROR: Function call failed!\nPress any key to close this application...");
            //Console.ReadKey();
            return -1;
        }

        private static void PrintConfig()
        {
            channelIndex = LIND.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);

            if (channelIndex > -1)
            {
                Console.WriteLine("\n\nAPPLICATION CONFIGURATION");
                Console.WriteLine("-------------------------------------------------------------------\n");
                Console.WriteLine("Configured Hardware Channel : " + driverConfig.channel[channelIndex].name);
                Console.WriteLine("Hardware Driver Version     : " + LIND.VersionToString(driverConfig.channel[channelIndex].driverVersion));
                Console.WriteLine("Used Transceiver            : " + driverConfig.channel[channelIndex].transceiverName);
                Console.WriteLine("-------------------------------------------------------------------\n");
            }
        }
        private static void PrintAssignError()
        {
            Console.WriteLine("\nPlease check application settings of \"" + appName + " LIN1\" \nand assign it to an available hardware channel and restart application.");
            LIND.XL_PopupHwConfig();
            //Console.ReadLine();
        }

        public static void Stop_LIN()
        {
            rxThread?.Abort();
        }

        public static void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new XLClass.xl_event();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;

            // Result values of WaitForSingleObject 
            XLDefine.WaitResults waitResult = new XLDefine.WaitResults();


            LIND.XL_FlushReceiveQueue(portHandle);

            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                waitResult = (XLDefine.WaitResults)WaitForSingleObject(eventHandle, 1000);

                // If event occured...
                if (waitResult != XLDefine.WaitResults.WAIT_TIMEOUT)
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hardware queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...receive data from hardware...
                        xlStatus = LIND.XL_Receive(portHandle, ref receivedEvent);

                        if(xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if (receivedEvent.tag == XLDefine.XL_EventTags.XL_LIN_MSG)
                            {
                                //string dir = "RX ";
                                //if ((receivedEvent.tagData.linMsgApi.linMsg.flags & XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX)
                                //  == XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX)
                                //{
                                //    dir = "TX ";
                                //}
                                ////Console.WriteLine(receivedEvent.tagData.linMsgApi.linMsg.id);
                                //RxThreadProcessData(receivedEvent);

                                Console.WriteLine(LIND.XL_GetEventString(receivedEvent));
                                foreach (var linFrame in LdfParser.Frames)
                                {
                                    if (linFrame.ID == receivedEvent.tagData.linMsgApi.linMsg.id)
                                    {
                                        // Dont forget to link Graphics UC too
                                        //mainWindowInstance.converter_UC.UpdateUI();

                                        for (int i = 0; i < LdfParser.receivedEvents.messageCount; i++)
                                        {
                                            if (LdfParser.receivedEvents.xlEvent[i].tagData.linMsgApi.linMsg.id == receivedEvent.tagData.linMsgApi.linMsg.id)
                                            {
                                                foreach (var signal in linFrame.Signals)
                                                {
                                                    signal.Value = BinaryToInt(receivedEvent.tagData.linMsgApi.linMsg.data, signal.StartBit, signal.Length);
                                                    //                //var olupitia = MainWindow.ConvertingDigitalValueToAnalogValue(signal.Value, signal.Name);
                                                    Application.Current.Dispatcher.Invoke(() =>
                                                    {
                                                            activeUC?.UpdateUI(signal.Name, Math.Round(signal.Value, 3), signal.AssociatedElement);
                                                            mainWindowInstance.graphics_UC.UpdateChart(signal, Math.Round(signal.Value, 3));
                                                    }); 
                                                    Console.WriteLine("Data extracted:" + signal.Value);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            else
                            {
                                switch (receivedEvent.tag)
                                {
                                    case XLDefine.XL_EventTags.XL_LIN_ERRMSG:
                                        Console.WriteLine("XL_LIN_ERRMSG");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_SYNCERR:
                                        Console.WriteLine("XL_LIN_SYNCERR");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_NOANS:
                                        Console.WriteLine("XL_LIN_NOANS");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_WAKEUP:
                                        Console.WriteLine("XL_LIN_WAKEUP");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_SLEEP:
                                        Console.WriteLine("XL_LIN_SLEEP");
                                        break;

                                    case XLDefine.XL_EventTags.XL_LIN_CRCINFO:
                                        Console.WriteLine("XL_LIN_CRCINFO");
                                        break;
                                }
                            }
                        }
                    }
                }
                // No event occured
            }
        }

    private static int BinaryToInt(byte[] byteArray, int startBit, int bitLength)
    {
        // Extract relevant bits from the byte array
        int startByte = startBit / 8;
        int startBitInByte = startBit % 8;

        int value = 0;
        for (int i = bitLength - 1; i >= 0; i--)
        {
            int bitIndex = startBitInByte + i;
            int byteIndex = startByte + (bitIndex / 8);
            int bitInByte = bitIndex % 8;

            value <<= 1;
            value |= (byteArray[byteIndex] >> bitInByte) & 1;
        }

        // Convert the extracted bits to a float
        //byte[] bytes = BitConverter.GetBytes(value);
        //return BitConverter.ToSingle(bytes, 0);          
        return value;
    }

    // UNUSED FOR NOW

    public static void RxThreadProcessData(XLClass.xl_event receivedEvent)
    {
        string Source = string.Empty;

        LinFrame linFrame = LdfParser.GetIndividualMsgByID(receivedEvent.tagData.linMsgApi.linMsg.id);
        byte[] data = receivedEvent.tagData.linMsgApi.linMsg.data;

        linFrame.DataBytes = data;

        if ((receivedEvent.tagData.linMsgApi.linMsg.flags & XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX) == XLDefine.XL_MessageFlags.XL_LIN_MSGFLAG_TX)
        {
            Source = "TX";
        }
        else
        {
            Source = "RX";
        }

        if (Source == "RX")
        {
            //LdfParser.AddDataToFrameLayout(linFrame);
        }

    }

    public static void LinRequest()
    {
        foreach (LinFrame linFrame in LdfParser.Frames)
        {
            if (linFrame.Sender != "MASTER")
            {
                XLDefine.XL_Status msg = LIND.XL_LinSendRequest(portHandle, accessMaskMaster, linFrame.ID, 0);
                //Console.WriteLine("*** LIN Send Request : " + msg);
            }
        }
    }

    public static void LinTransmit()
    {
        byte[] linData = LdfParser.Frames[0].DataBytes;
        linData[0] = 255;
        linData[1] = 0;
        if (LIND.XL_LinSetSlave(portHandle, accessMaskMaster, 1, linData, 8, XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED) == XLDefine.XL_Status.XL_SUCCESS)
        {
            if (LIND.XL_LinSendRequest(portHandle, accessMaskMaster, 1, 0) == XLDefine.XL_Status.XL_SUCCESS)
            {
                Console.WriteLine("trimis");
            }
        }
    }

    public static void LinTransmitGPIOState()
    {
        LinFrame frm = LdfParser.Frames[0];
        for (int i = 0; i < frm.Signals.Count; i++)
        {
            frm.Layout[i] = frm.Signals[i].Layout[0];
        }

        for (int j = 0; j < 8; j++)
        {
            // Take 8 integers at a time, convert each to a byte
            // Here, the method of conversion will be simple truncation to fit in the byte
            byte value = 0;
            for (int k = 7; k >= 0; k--)
            {
                value <<= 1; // Shift left to make room for the next bit
                value |= (byte)(frm.Layout[j * 8 + k] & 1); // Set the least significant bit
            }
            frm.DataBytes[j] = value; // Simple example: sum of 8 elements
        }

        if (LIND.XL_LinSetSlave(portHandle, accessMaskMaster, 1, frm.DataBytes, 8, XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED) == XLDefine.XL_Status.XL_SUCCESS)
        {
            if (LIND.XL_LinSendRequest(portHandle, accessMaskMaster, 1, 0) == XLDefine.XL_Status.XL_SUCCESS)
            {
                Console.WriteLine("trimis");
            }
        }
    }

    public static void LinTransmitAQVControlDEAStates()
    {
        LinFrame frm = LdfParser.Frames.FirstOrDefault(frame => frame.Name == "AQV_Control_DEA_States_0x0");

        for (int i = 0; i < frm.Signals.Count; i++)
        {
            frm.Layout[i] = frm.Signals[i].Layout[0];
        }


        for (int j = 0; j < 8; j++)
        {
            // Take 8 integers at a time, convert each to a byte
            // Here, the method of conversion will be simple truncation to fit in the byte
            byte value = 0;
            for (int k = 7; k >= 0; k--)
            {
                value <<= 1; // Shift left to make room for the next bit
                value |= (byte)(frm.Layout[j * 8 + k] & 1); // Set the least significant bit
            }
            frm.DataBytes[j] = value;
        }


        if (LIND.XL_LinSetSlave(portHandle, accessMaskMaster, 5, frm.DataBytes, 8, XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED) == XLDefine.XL_Status.XL_SUCCESS)
        {
            if (LIND.XL_LinSendRequest(portHandle, accessMaskMaster, 5, 0) == XLDefine.XL_Status.XL_SUCCESS)
            {
                Console.WriteLine("trimis");
            }
        }
    }

    public static void LinTransmitPWMConfig(int freq, int duty, int duration, int number, int pwmOn)
    {
        LinFrame PWMfrm = LdfParser.Frames.FirstOrDefault(frame => frame.Name == "PWM_Config_0x50");

        PWMfrm.Signals[0].Value = freq;
        PWMfrm.Signals[1].Value = duty;
        PWMfrm.Signals[2].Value = duration;
        PWMfrm.Signals[3].Value = number;
        PWMfrm.Signals[4].Value = pwmOn;

        for (int i = 0; i < PWMfrm.Signals.Count; i++)
        {
            PWMfrm.Layout[i] = (int)PWMfrm.Signals[i].Value;
            Console.WriteLine(PWMfrm.Signals[i].Name);
            Console.WriteLine(PWMfrm.Signals[i].Layout[0]);
            Console.WriteLine(PWMfrm.Signals[i].Value);
        }


        for (int j = 0; j < 8; j++)
        {
            byte value = 0;
            //for (int k = 7; k >= 0; k--)
            //{
            value <<= 1;
            value |= (byte)(PWMfrm.Layout[j]);
            //}
            PWMfrm.DataBytes[j] = value;
            Console.WriteLine("PWMFrm.DataBytes[" + j + "] : " + PWMfrm.DataBytes[j]);
        }

        if (LIND.XL_LinSetSlave(portHandle, accessMaskMaster, 50, PWMfrm.DataBytes, 8, XLDefine.XL_LIN_CalcChecksum.XL_LIN_CALC_CHECKSUM_ENHANCED) == XLDefine.XL_Status.XL_SUCCESS)
        {
            if (LIND.XL_LinSendRequest(portHandle, accessMaskMaster, 50, 0) == XLDefine.XL_Status.XL_SUCCESS)
            {
                Console.WriteLine("trimis");
            }
        }
    }
}
