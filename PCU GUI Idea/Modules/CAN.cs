using Microsoft.Win32.SafeHandles;
using PCU_GUI_Idea;
using PCU_GUI_Idea.Modules;
using PCU_GUI_Idea.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using vxlapi_NET;


    public static class CAN
    {
        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)
        public static XLDriver CAND = new XLDriver();
        public static String appName = "PCU_DC_DC";

        // Driver configuration
        public static XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        // Variables required by XLDriver
        public static XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_NONE;
        public static uint hwIndex = 0;
        public static uint hwChannel = 0;
        public static int portHandle = -1;
        public static UInt64 accessMask = 0;
        public static UInt64 permissionMask = 0;
        public static UInt64 txMask = 0;
        public static UInt64 rxMask = 0;
        public static int txCi = -1;
        public static int rxCi = -1;
        public static EventWaitHandle xlEvWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);

        // RX thread
        public static Thread rxThread;
        public static bool blockRxThread = false;
    // -----------------------------------------------------------------------------------------------
        private static MainWindow mainWindowInstance;
        private static ISignalBindable activeUC;
        public static void Initialize(MainWindow mainWindow)
        {
            mainWindowInstance = mainWindow;
            activeUC = mainWindow.tab.Content as ISignalBindable;
        }



    // -----------------------------------------------------------------------------------------------
    /// <summary>
    /// MAIN
    /// 
    /// Sends and receives CAN messages using main methods of the "XLDriver" class.
    /// This demo requires two connected CAN channels (Vector network interface). 
    /// The configuration is read from Vector Hardware Config (vcanconf.exe).
    /// </summary>
    // -----------------------------------------------------------------------------------------------
    [STAThread]
        public static int Start_CAN()
        {
            XLDefine.XL_Status status;

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("                     xlCANdemo.NET C# V20.30                       ");
            Console.WriteLine("Copyright (c) 2020 by Vector Informatik GmbH.  All rights reserved.");
            Console.WriteLine("-------------------------------------------------------------------\n");

            // print .NET wrapper version
            Console.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);

            // Open XL Driver
            status = CAND.XL_OpenDriver();
            Console.WriteLine("Open Driver       : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();


            // Get XL Driver configuration
            status = CAND.XL_GetDriverConfig(ref driverConfig);
            Console.WriteLine("Get Driver Config : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();


            // Convert the dll version number into a readable string
            Console.WriteLine("DLL Version       : " + CAND.VersionToString(driverConfig.dllVersion));


            // Display channel count
            Console.WriteLine("Channels found    : " + driverConfig.channelCount);


            // Display all found channels
            for (int i = 0; i < driverConfig.channelCount; i++)
            {
                Console.WriteLine("\n                   [{0}] " + driverConfig.channel[i].name, i);
                Console.WriteLine("                    - Channel Mask    : " + driverConfig.channel[i].channelMask);
                Console.WriteLine("                    - Transceiver Name: " + driverConfig.channel[i].transceiverName);
                Console.WriteLine("                    - Serial Number   : " + driverConfig.channel[i].serialNumber);
            }

            // If the application name cannot be found in VCANCONF...
            if ((CAND.XL_GetApplConfig(appName, 0, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS) ||
                (CAND.XL_GetApplConfig(appName, 1, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN) != XLDefine.XL_Status.XL_SUCCESS))
            {
                //...create the item with two CAN channels
                CAND.XL_SetApplConfig(appName, 0, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                CAND.XL_SetApplConfig(appName, 1, XLDefine.XL_HardwareType.XL_HWTYPE_NONE, 0, 0, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
                PrintAssignErrorAndPopupHwConf();
            }

            // Request the user to assign channels until both CAN1 (Tx) and CAN2 (Rx) are assigned to usable channels
            while (!GetAppChannelAndTestIsOk(0, ref txMask, ref txCi) || !GetAppChannelAndTestIsOk(1, ref rxMask, ref rxCi))
            {
                PrintAssignErrorAndPopupHwConf();
                break;
            }

            //PrintConfig();

            accessMask = txMask | rxMask;
            permissionMask = accessMask;

            // Open port
            status = CAND.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 1024, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            Console.WriteLine("\n\nOpen Port             : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Check port
            status = CAND.XL_CanRequestChipState(portHandle, accessMask);
            Console.WriteLine("Can Request Chip State: " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Activate channel
            status = CAND.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_NONE);
            Console.WriteLine("Activate Channel      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Initialize EventWaitHandle object with RX event handle provided by DLL
            int tempInt = -1;
            status = CAND.XL_SetNotification(portHandle, ref tempInt, 1);
            xlEvWaitHandle.SafeWaitHandle = new SafeWaitHandle(new IntPtr(tempInt), true);

            Console.WriteLine("Set Notification      : " + status);
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Reset time stamp clock
            status = CAND.XL_ResetClock(portHandle);
            Console.WriteLine("Reset Clock           : " + status + "\n\n");
            if (status != XLDefine.XL_Status.XL_SUCCESS) PrintFunctionError();

            // Run Rx Thread
            //Console.WriteLine("Start Rx thread...");
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();

            // Kill Rx thread
            //rxThread.Abort();
            //Console.WriteLine("Close Port                     : " + CAND.XL_ClosePort(portHandle));
            //Console.WriteLine("Close Driver                   : " + CAND.XL_CloseDriver());

            return 0;
        }
    // -----------------------------------------------------------------------------------------------

        public static void Stop_CAN()
        { 
            rxThread?.Abort();
        }

        private static float BinaryToFloat(byte[] byteArray, int startBit, int bitLength)
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
            float result = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
            return result;
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
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Error message/exit in case of a functional call does not return XL_SUCCESS
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static int PrintFunctionError()
        {
            Console.WriteLine("\nERROR: Function call failed!\nPress any key to continue...");
            return -1;
        }
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Displays the Vector Hardware Configuration.
        /// </summary>
        // -----------------------------------------------------------------------------------------------

        // -----------------------------------------------------------------------------------------------


        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Error message if channel assignment is not valid and popup VHwConfig, so the user can correct the assignment
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private static void PrintAssignErrorAndPopupHwConf()
        {
            Console.WriteLine("\nPlease check application settings of \"" + appName + " CAN1/CAN2\",\nassign them to available hardware channels and press enter.");
            CAND.XL_PopupHwConfig();
        }
        // -----------------------------------------------------------------------------------------------

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        // -----------------------------------------------------------------------------------------------
        private static bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = CAND.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                Console.WriteLine("XL_GetApplConfig      : " + status);
                PrintFunctionError();
            }

            chMask = CAND.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = CAND.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            // test if CAN is available on this channel
            return (driverConfig.channel[chIdx].channelBusCapabilities & XLDefine.XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) != 0;
        }



        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Sends some CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
        // -----------------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// EVENT THREAD (RX)
        /// 
        /// RX thread waits for Vector interface events and displays filtered CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
        public static void RXThread()
        {
            // Create new object containing received data 
            XLClass.xl_event receivedEvent = new XLClass.xl_event();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                //Test
                //receivedEvent.tagData.can_Msg.id = 0xA2;
                //receivedEvent.tagData.can_Msg.dlc = 8;
                //receivedEvent.tagData.can_Msg.data[0] = 0x40;
                //receivedEvent.tagData.can_Msg.data[1] = 0x48;
                //receivedEvent.tagData.can_Msg.data[2] = 0xF5;
                //receivedEvent.tagData.can_Msg.data[3] = 0xC3;
                //receivedEvent.tagData.can_Msg.data[4] = 0;
                //receivedEvent.tagData.can_Msg.data[5] = 0;
                //receivedEvent.tagData.can_Msg.data[6] = 0;
                //receivedEvent.tagData.can_Msg.data[7] = 0;
                //Console.WriteLine(CAND.XL_GetEventString(receivedEvent));
                //foreach (var message in DbcParser.Messages)
                //{
                //    if (message.Id == receivedEvent.tagData.can_Msg.id)
                //    {
                //        for (int i = 0; i < 4; i++)
                //        {
                //            if (MainWindow.receivedEvents.xlEvent[i].tagData.can_Msg.id == receivedEvent.tagData.can_Msg.id)
                //            {
                //                foreach (var signal in message.Signals)
                //                {
                //                    if (signal.DataType == "Float")
                //                    {
                //                        signal.Value = BinaryToFloat(receivedEvent.tagData.can_Msg.data, signal.StartBit, signal.Length);
                //                        Application.Current.Dispatcher.Invoke(() =>
                //                        {
                //                            mainWindowInstance.UpdateInterface(signal.Name, signal.Value);
                //                        });
                //                    }
                //                    //if (signal.DataType == "Unsigned")
                //                    //{
                //                    //    signal.Value = BinaryToUnsignedInteger(receivedEvent.tagData.can_Msg.data, signal.StartBit, signal.Length);
                //                    //    Application.Current.Dispatcher.Invoke(() =>
                //                    //    {
                //                    //        mainWindowInstance.UpdateInterface(signal.Name, signal.Value);
                //                    //    });
                //                    //}
                //                    //Console.WriteLine("Data extracted:" + signal.Value);
                //                    //Thread.Sleep(5000);
                //                    //// Wait for hardware events
                //                }
                //            }
                //        }
                //    }
                //}

                if (xlEvWaitHandle.WaitOne(1000))
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) { Thread.Sleep(1000); }

                        // ...receive data from hardware.
                        xlStatus = CAND.XL_Receive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if ((receivedEvent.flags & XLDefine.XL_MessageFlags.XL_EVENT_FLAG_OVERRUN) != 0)
                            {
                                Console.WriteLine("-- XL_EVENT_FLAG_OVERRUN --");
                            }

                            // ...and data is a Rx msg...
                            if (receivedEvent.tag == XLDefine.XL_EventTags.XL_RECEIVE_MSG)
                            {
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_OVERRUN) != 0)
                                {
                                    Console.WriteLine("-- XL_CAN_MSG_FLAG_OVERRUN --");
                                }

                                // ...check various flags
                                if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_ERROR_FRAME)
                                {
                                    Console.WriteLine("ERROR FRAME");
                                }

                                else if ((receivedEvent.tagData.can_Msg.flags & XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                    == XLDefine.XL_MessageFlags.XL_CAN_MSG_FLAG_REMOTE_FRAME)
                                {
                                    Console.WriteLine("REMOTE FRAME");
                                }

                                else
                                {

                                    Console.WriteLine(CAND.XL_GetEventString(receivedEvent));
                                    foreach (var message in DbcParser.Messages)
                                    {
                                        if (message.Id == receivedEvent.tagData.can_Msg.id)
                                        {
                                        // Dont forget to link Graphics UC too
                                         //mainWindowInstance.converter_UC.UpdateUI();

                                            for (int i = 0; i < DbcParser.receivedEvents.messageCount ; i++)
                                            {
                                                if (DbcParser.receivedEvents.xlEvent[i].tagData.can_Msg.id == receivedEvent.tagData.can_Msg.id)
                                                {
                                                    foreach (var signal in message.Signals)
                                                    {
                                                        if (signal.DataType == "Float")
                                                        {
                                                           signal.Value = BinaryToFloat(receivedEvent.tagData.can_Msg.data, signal.StartBit, signal.Length);
                                                        //                //var olupitia = MainWindow.ConvertingDigitalValueToAnalogValue(signal.Value, signal.Name);
                                                        Application.Current.Dispatcher.Invoke(() =>
                                                        {
                                                            activeUC?.UpdateUI(signal.Name, Math.Round(signal.Value, 3), signal.AssociatedElement);
                                                            mainWindowInstance.graphics_UC.UpdateChart(signal, Math.Round(signal.Value, 3));
                                                        });
                                                    }
                                                        if (signal.DataType == "Unsigned")
                                                        {
                                            //                signal.Value = BinaryToInt(receivedEvent.tagData.can_Msg.data, signal.StartBit, signal.Length);
                                            //                Application.Current.Dispatcher.Invoke(() =>
                                            //                {
                                            //                    mainWindowInstance.UpdateInterface(signal.Name, Math.Round(signal.Value, 3));
                                            //                    if (mainWindowInstance.Enables_and_Readings_0x0D2_V12Supply.IsChecked == true && mainWindowInstance.Enables_and_Readings_0x0D2_referenceSupply.IsChecked == true)
                                            //                    {
                                            //                        // mainWindowInstance.LogData(signal.Name, signal.Value);
                                            //                    }
                                            //                    mainWindowInstance.UpdateChart(signal, Math.Round(signal.Value, 3));
                                            //                });
                                                        }
                                                        Console.WriteLine("Data extracted:" + signal.Value);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // No event occurred
            }
        }
        // -----------------------------------------------------------------------------------------------
    }
