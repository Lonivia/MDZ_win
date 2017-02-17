using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using nRFUart;
using System.Collections.ObjectModel;
using System.Globalization;
using Nordicsemi;

namespace nRFUart
{
    public partial class Form1 : Form
    {
        nRFUartController controller;
        bool isControllerInitialized = false;
        bool isControllerConnected = false;

        const string strConnect = "Connect";
        const string strScanning = "Stop scanning";
        const string strDisconnect = "Disconnect";
        const string strStopSendData = "Stop sending data";
        const string strStartSendData = "Send 100kB data";

        const UInt32 logHighWatermark = 10000;  // If we reach high watermark, we delete until we're
        // down to low watermark
        const UInt32 logLowWatermark = 5000;

        //private ObservableCollection<String> _outputText = null;
        //public ObservableCollection<string> OutputText
        //{
        //    get { return _outputText ?? (_outputText = new ObservableCollection<string>()); }
        //    set { _outputText = value; }
        //}
        //FileStream aFile;
        //StreamWriter sw;
        public Form1()
        {
            //aFile = new FileStream("D:\\temp\\temp.txt", FileMode.OpenOrCreate);
            Control.CheckForIllegalCrossThreadCalls = false; 
            InitializeComponent();
            InitializeNrfUartController();


            
            

        }



        void InitializeNrfUartController()
        {
            controller = new nRFUartController();

            /* Registering event handler methods for all nRFUartController events. */
            controller.LogMessage += OnLogMessage;
            controller.Initialized += OnControllerInitialized;
            controller.Scanning += OnScanning;
            controller.ScanningCanceled += OnScanningCanceled;
            controller.Connecting += OnConnecting;
            controller.ConnectionCanceled += OnConnectionCanceled;
            controller.Connected += OnConnected;
            controller.PipeDiscoveryCompleted += OnControllerPipeDiscoveryCompleted;
            controller.Disconnected += OnDisconnected;
            //controller.SendDataStarted += OnSendDataStarted;
            //controller.SendDataCompleted += OnSendDataCompleted;
            controller.ProgressUpdated += OnProgressUpdated;

            controller.Initialize();
        }




        public void AddToOutput(string text)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.ffff");
            text = String.Format("[{0}] {1}", timestamp, text);

            //if (OutputText.Count >= logHighWatermark)
            //{
            //    UInt32 numToDelete = (UInt32)OutputText.Count - logLowWatermark;
            //    for (UInt32 i = 0; i < numToDelete; i++)
            //    {
            //        OutputText.RemoveAt(0);
            //    }
            //}

            OutputText.Items.Add(text);

            //sw = new StreamWriter(aFile);
            //sw.Write(text.ToString());
            //sw.Close();

            //File.AppendAllText("D:\\temp\\temp.txt", text+"\r\n", Encoding.Default);

            OutputText.SetSelected(OutputText.Items.Count - 1, true);  // 光标移到最后一条
        }

        #region nRFUart event handlers
        void OnControllerInitialized(object sender, EventArgs e)
        {
            isControllerInitialized = true;
            btnConnect.Enabled = true; //.IsEnabled = true;
            AddToOutput("Ready to connect");
        }

        int[,] RxData = new int[2400, 6];
        int Rev_i=0;
        int Rev_i2 = 0;


        void OnLogMessage(object sender, OutputReceivedEventArgs e)
        {
            if (e.Message.Equals("0144000002"))
            {
                AddToOutput("Start Receive WATCH DATA!");
            }
            else if (e.Message.Equals("0144000003")) {
                AddToOutput("Start Receive BELT DATA!");
            }
            else if (e.Message.Equals("0145000002") || e.Message.Equals("0145000003"))
            {
                AddToOutput("Finish Receive DATA!");
            }
            else if (e.Message.Equals("0137000002"))
            {
                AddToOutput("Started Watch AD...");
                //controller.InitiateDisconnect();
                /*
                controller.conn_BELT();
                AddToOutput("Connected to Belt...");
                byte[] bts_belt = new byte[2];
                bts_belt[0] = 0x01;
                bts_belt[1] = 0x36;
                controller.SendBytes(bts_belt);
                AddToOutput("Starting Belt AD...");
                 * */

            }
            else if (e.Message.Substring(0, 2).Equals("02"))
            {
                if (Rev_i <= 2399)
                {
                    RxData[Rev_i, 0] = int.Parse(e.Message.Substring(6, 4), NumberStyles.HexNumber);
                    RxData[Rev_i, 1] = int.Parse(e.Message.Substring(10, 4), NumberStyles.HexNumber);
                    RxData[Rev_i, 2] = int.Parse(e.Message.Substring(14, 4), NumberStyles.HexNumber);
                    RxData[Rev_i, 3] = int.Parse(e.Message.Substring(18, 4), NumberStyles.HexNumber);
                    RxData[Rev_i, 4] = int.Parse(e.Message.Substring(22, 4), NumberStyles.HexNumber);
                    //RxData[Rev_i, 2] = Convert.ToInt32(e.Message.Substring(10, 4));
                    //RxData[Rev_i, 3] = Convert.ToInt32(e.Message.Substring(14, 4));
                    //RxData[Rev_i, 4] = Convert.ToInt32(e.Message.Substring(18, 4));
                    //RxData[Rev_i, 5] = Convert.ToInt32(e.Message.Substring(22, 4));
                    Rev_i = Rev_i + 1;
                }
                if (Rev_i >= 2400) {
                    Rev_i = 0;
                }

            }
            else if (e.Message.Substring(0, 2).Equals("03"))
            {

                if (Rev_i2 <= 2399)
                {
                    RxData[Rev_i2, 5] = short.Parse(e.Message.Substring(6, 4), NumberStyles.HexNumber);
                    Rev_i2++;
                }
                if(Rev_i2>=2400)
                {
                    Rev_i2 = 0;
                }

                //AddToOutput(e.Message);
            }
            else
            {
                AddToOutput(e.Message);
            }

            //AddToOutput(e.Message);
        }

        void OnScanning(object sender, EventArgs e)
        {
            AddToOutput("Scanning...");
            SetConnectButtonText(strScanning);
        }

        void SetConnectButtonText(string text)
        {
            btnConnect.Text = text;
        }

        void OnScanningCanceled(object sender, EventArgs e)
        {
            AddToOutput("Stopped scanning");
            SetConnectButtonText(strConnect);
        }

        void OnConnectionCanceled(object sender, EventArgs e)
        {
            SetConnectButtonText(strConnect);
        }

        void OnConnecting(object sender, EventArgs e)
        {
            AddToOutput("Connecting...");
        }

        void OnConnected(object sender, EventArgs e)
        {
            isControllerConnected = true;
            SetConnectButtonText(strDisconnect);
        }

        void OnControllerPipeDiscoveryCompleted(object sender, EventArgs e)
        {
            AddToOutput("Ready to send");
        }

        //void OnSendDataStarted(object sender, EventArgs e)
        //{
        //    AddToOutput("Started sending data...");
        //    SetStopDataIsEnabled(true);
        //    SetStartSendIsEnabled(false);
        //    SetStartSendFileIsEnabled(false);
        //}

        //void OnSendDataCompleted(object sender, EventArgs e)
        //{
        //    AddToOutput("Data transfer ended");
        //    SetStopDataIsEnabled(false);
        //    SetStartSendIsEnabled(true);
        //    SetStartSendFileIsEnabled(true);
        //    SetProgressBarValue(0);
        //}

        void OnDisconnected(object sender, EventArgs e)
        {
            isControllerConnected = false;
            AddToOutput("Disconnected");
            SetConnectButtonText(strConnect);
            //SetStopDataIsEnabled(false);
            //SetStartSendIsEnabled(true);
            //SetStartSendFileIsEnabled(true);
        }

        void OnProgressUpdated(object sender, Nordicsemi.ValueEventArgs<int> e)
        {
            //int progress = e.Value;
            //if (0 <= progress && progress <= 100)
            //{
            //    SetProgressBarValue(progress);
            //}
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.Close();
            base.OnClosing(e);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!isControllerInitialized)
            {
                return;
            }

            if (btnConnect.Text == strConnect)
            {
                controller.InitiateConnection();
                //controller.scan2();
                //controller.conn2();
            }
            else if (btnConnect.Text == strScanning)
            {
                controller.StopScanning();
            }
            else if (btnConnect.Text == strDisconnect)
            {
                controller.InitiateDisconnect();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!isControllerConnected)
            {
                return;
            }

            // 格式：01 02 03 23，十六进制，转换成字节发送
            string text = tbInput.Text;
            string[] texts = text.Split(' ');
            byte[] bts = new byte[texts.Length];
            int i = 0;
            foreach (string str in texts)
            {
                bts[i++] = (byte)Convert.ToInt32(str, 16);
            }

            controller.SendBytes(bts);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...左手浮");
            
            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md11101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11101.csv", ""+ "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11101.csv", "," +(wi*0.025).ToString()+","+ RxData[(int)wi, 0]+"\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md11102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md11103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md11104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md11105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md11106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md11106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md11106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md11106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }


        class StringValue
        {
            public string Text { get; private set; }
            public object Data { get; private set; }

            public StringValue(string val)
            {
                Text = val;
            }

            public StringValue(string val, object data)
            {
                Text = val;
                Data = data;
            }
        }

        /*
        MasterEmulator masterEmulator = new MasterEmulator();
        BindingList<StringValue> discoveredDevices = new BindingList<StringValue>();
         * */

        private void scan_Click(object sender, EventArgs e)
        {
            AddToOutput("Start Scanning>>>");
            controller.scan2(this);
            AddToOutput("Scan Finish!");
           
        }

        /*
        private void DiscoverPipes()
        {
            
            try
            {
                masterEmulator.DiscoverPipes();
                masterEmulator.OpenAllRemotePipes();
                //pipeDiscoveryComplete = true;
                //if (isConnected)
                {
                    //grpRight.Enabled = true;
                    //pnlRight.BackColor = Color.LightGreen;
                }
            }
            catch (Exception ex)
            {
                //DisplayErrorMessage(ex);
            }
        }
         * */

        private void conn_Click(object sender, EventArgs e)
        {
            controller.conn_WATCH();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            controller.conn_BELT();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            controller.InitiateDisconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...左手中");

            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md12101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12101.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 0] + "\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md12102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md12103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md12104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md12105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md12106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md12106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md12106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md12106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...左手沉");

            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md13101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13101.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 0] + "\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md13102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md13103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md13104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md13105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md13106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md13106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md13106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md13106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...右手浮");

            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md21101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21101.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 0] + "\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md21102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md21103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md21104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md21105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md21106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md21106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md21106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md21106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...右手中");

            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md22101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22101.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 0] + "\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md22102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md22103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md22104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md22105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md22106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md22106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md22106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md22106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AddToOutput("Writing...右手沉");

            FileStream stream1 = File.Create("C:\\MDZ_File_Test\\md23101.csv");
            stream1.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23101.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23101.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23101.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 0] + "\r\n", Encoding.Default);
            }

            FileStream stream2 = File.Create("C:\\MDZ_File_Test\\md23102.csv");
            stream2.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23102.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23102.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23102.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 1] + "\r\n", Encoding.Default);
            }

            FileStream stream3 = File.Create("C:\\MDZ_File_Test\\md23103.csv");
            stream3.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23103.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23103.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23103.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 2] + "\r\n", Encoding.Default);
            }

            FileStream stream4 = File.Create("C:\\MDZ_File_Test\\md23104.csv");
            stream4.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23104.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23104.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23104.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 3] + "\r\n", Encoding.Default);
            }

            FileStream stream5 = File.Create("C:\\MDZ_File_Test\\md23105.csv");
            stream5.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23105.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23105.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23105.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 4] + "\r\n", Encoding.Default);
            }

            FileStream stream6 = File.Create("C:\\MDZ_File_Test\\md23106.csv");
            stream6.Close();

            File.AppendAllText("C:\\MDZ_File_Test\\md23106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23106.csv", "" + "\r\n", Encoding.Default);
            File.AppendAllText("C:\\MDZ_File_Test\\md23106.csv", "" + "\r\n", Encoding.Default);
            for (double wi = 0; wi <= 2399; wi++)
            {
                File.AppendAllText("C:\\MDZ_File_Test\\md23106.csv", "," + (wi * 0.025).ToString() + "," + RxData[(int)wi, 5] + "\r\n", Encoding.Default);
            }


            AddToOutput("Finish Write!");
        }
             

    }
}
