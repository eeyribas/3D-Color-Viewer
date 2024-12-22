using Microsoft.Win32;
using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectrometerColorChart
{
    public partial class Form1 : Form
    {
        private double[] x10 = { 0.0191, 0.0434, 0.0847, 0.1406, 0.2045, 0.2647, 0.3147, 0.3577, 0.3837, 0.3867, 0.3707, 0.3430, 0.3023,
                                 0.2541, 0.1956, 0.1323, 0.0805, 0.0411, 0.0162, 0.0051, 0.0038, 0.0154, 0.0375, 0.0714, 0.1177, 0.1730,
                                 0.2365, 0.3042, 0.3768, 0.4516, 0.5298, 0.6161, 0.7052, 0.7938, 0.8787, 0.9512, 1.0142, 1.0743, 1.1185,
                                 1.1343, 1.1240, 1.0891, 1.0305, 0.9507, 0.8563, 0.7549, 0.6475, 0.5351, 0.4316, 0.3437, 0.2683, 0.2043,
                                 0.1526, 0.1122, 0.0813, 0.0579, 0.0409, 0.0286, 0.0199, 0.0138, 0.0096 };

        private double[] y10 = { 0.0020, 0.0045, 0.0088, 0.0145, 0.0214, 0.0295, 0.0387, 0.0496, 0.0621, 0.0747, 0.0895, 0.1063, 0.1282,
                                 0.1528, 0.1852, 0.2199, 0.2536, 0.2977, 0.3391, 0.3954, 0.4608, 0.5314, 0.6067, 0.6857, 0.7618, 0.8233,
                                 0.8752, 0.9238, 0.9620, 0.9822, 0.9918, 0.9991, 0.9973, 0.9824, 0.9556, 0.9152, 0.8689, 0.8256, 0.7774,
                                 0.7204, 0.6583, 0.5939, 0.5280, 0.4618, 0.3981, 0.3396, 0.2835, 0.2283, 0.1798, 0.1402, 0.1076, 0.0812,
                                 0.0603, 0.0441, 0.0318, 0.0226, 0.0159, 0.0111, 0.0077, 0.0054, 0.0037 };

        private double[] z10 = { 0.0860, 0.1971, 0.3894, 0.6568, 0.9725, 1.2825, 1.5535, 1.7985, 1.9673, 2.0273, 1.9948, 1.9007, 1.7454,
                                 1.5549, 1.3176, 1.0302, 0.7721, 0.5701, 0.4153, 0.3024, 0.2185, 0.1592, 0.1120, 0.0822, 0.0607, 0.0431,
                                 0.0305, 0.0206, 0.0137, 0.0079, 0.0040, 0.0011, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000,
                                 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000,
                                 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000, 0.0000 };

        private double[] d65 = { 82.75, 87.12, 91.49, 92.46, 93.43, 90.06, 86.68, 95.77, 104.86, 110.94, 117.01, 117.41, 117.81, 116.34,
                                 114.86, 115.39, 115.92, 112.37, 108.81, 109.08, 109.35, 108.58, 107.80, 106.30, 104.79, 106.24, 107.69,
                                 106.05, 104.41, 104.23, 104.05, 104.02, 100.00, 98.17, 96.33, 96.06, 95.79, 92.24, 88.69, 89.35, 90.01,
                                 89.80, 89.60, 88.65, 87.70, 85.49, 83.29, 83.49, 83.70, 81.86, 80.03, 80.12, 80.21, 81.25, 82.28, 80.28,
                                 78.28, 74.00, 69.72, 70.67, 71.61 };

        private double[] standardPlate = { 0.85800, 0.86810, 0.87820, 0.88565, 0.89310, 0.89670, 0.90030, 0.90605, 0.91180, 0.91350, 0.91520,
                                           0.91745, 0.91970, 0.91955, 0.91940, 0.92260, 0.92580, 0.92235, 0.91890, 0.92265, 0.92640, 0.92855, 
                                           0.93070, 0.93495, 0.93920, 0.93810, 0.93700, 0.93515, 0.93330, 0.93470, 0.93610, 0.93900, 0.94190, 
                                           0.94155, 0.94120, 0.94090, 0.94060, 0.94165, 0.94270, 0.94590, 0.94910, 0.94875, 0.94840, 0.95125, 
                                           0.95410, 0.95655, 0.95900, 0.95940, 0.95980, 0.95595, 0.95210, 0.95440, 0.95670, 0.96190, 0.96710, 
                                           0.96710, 0.96710, 0.96650, 0.96590, 0.96810, 0.97030 };

        private double w10x = 94.76;
        private double w10y = 99.98;
        private double w10z = 107.304;
        private double deltaLamda = 5.0;

        private double[] ERsLed = new double[61];
        private double lStandardSample = 0;
        private double aStandardSample = 0;
        private double bStandardSample = 0;

        private string selectDevicePortName = "";
        private double firstPixelD = 0;
        private double lastPixelD = 0;
        private string integrationTime = "";
        private string averageScan = "";
        private string digitalGain = "";
        private string analogGain = "";
        private int choosingFilter = 10;
        private int loopCountTest = 0;
        private int loopCountOther = 0;
        private int measurementCount = 0;
        private int measurementLenght = 61;

        private bool taskState = true;
        private Random random = new Random();
        private Dictionary<int, List<int>> dictionaryList = new Dictionary<int, List<int>>();
        private List<int> arrayList = new List<int>();
        private int count = 0;

        private string fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
        private static WebSocketServer webSocketServer;
        private Dictionary<string, WebSocketSession> webSocketSessions;

        private int graphicMinimum = 0;
        private int graphicMaximum = 0;
        private int seriesIndex1 = 0;
        private int seriesIndex2 = 0;

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(1460, 820);
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;

            webSocketSessions = new Dictionary<string, WebSocketSession>();
            webSocketServer = new WebSocketServer();
            webSocketServer.Setup(3435);
            webSocketServer.NewSessionConnected += WebSocketServer_NewSessionConnected;
            webSocketServer.Start();

            FormConfig();
            Connection();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string chartPath = Directory.GetCurrentDirectory() + @"\WebPage\PassiveChart.html";
            webBrowser1.Url = new Uri(chartPath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                label4.Text = "Default";
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                int firstPixel = (int)firstPixelD - 1;
                int lastPixel = (int)lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;

                if (Functions.DigitalGainSetting(serialPort1, digitalGain) == 6 &&
                    Functions.AnalogGainSetting(serialPort1, analogGain) == 6)
                    label4.Text = "True";
                else
                    label4.Text = "False";

                string sendData = "*MEASure:DARKspectra " + integrationTime + " " + averageScan + " format<CR>\r";
                pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, choosingFilter, section,
                                        loopCountOther, dataLenght, dataCount, firstPixel, firstPixelD);
                if (label1.Text != "False")
                    label1.Text = "True";
            }
            catch (Exception)
            {
                label4.Text = "False";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                label5.Text = "Default";
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                int firstPixel = (int)firstPixelD - 1;
                int lastPixel = (int)lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;
                double[] ERefLed = new double[61];

                string sendData = "*MEASure:REFERence " + integrationTime + " " + averageScan + " format<CR>\r";
                pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, choosingFilter, section,
                                        loopCountOther, dataLenght, dataCount, firstPixel, firstPixelD);

                for (int i = 10; i < ERefLed.Length + 10; i++)
                    ERefLed[i - 10] = pixelSectionDataValue[i];
                for (int j = 0; j < ERsLed.Length; j++)
                    ERsLed[j] = ERefLed[j] / standardPlate[j];

                label5.Text = "True";
            }
            catch (Exception)
            {
                label5.Text = "False";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                label6.Text = "Default";
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                double[] RStandardSample = new double[61];
                double[] EStandardSample = new double[61];
                int firstPixel = (int)firstPixelD - 1;
                int lastPixel = (int)lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;

                string sendData = "*MEASure:REFERence " + integrationTime + " " + averageScan + " format<CR>\r";
                pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, choosingFilter, section, 
                                        loopCountTest, dataLenght, dataCount, firstPixel, firstPixelD);

                for (int i = 10; i < EStandardSample.Length + 10; i++)
                    EStandardSample[i - 10] = pixelSectionDataValue[i];
                for (int j = 0; j < RStandardSample.Length; j++)
                    RStandardSample[j] = EStandardSample[j] / ERsLed[j];

                double XStandardSample = Functions.XYZCalculation(RStandardSample, d65, x10, y10, deltaLamda) / w10x;
                double YStandardSample = Functions.XYZCalculation(RStandardSample, d65, y10, y10, deltaLamda) / w10y;
                double ZStandardSample = Functions.XYZCalculation(RStandardSample, d65, z10, y10, deltaLamda) / w10z;
                lStandardSample = (116 * Math.Pow(YStandardSample, 1.0 / 3.0)) - 16;
                aStandardSample = 500 * (Math.Pow(XStandardSample, 1.0 / 3.0) - Math.Pow(YStandardSample, 1.0 / 3.0));
                bStandardSample = 200 * (Math.Pow(YStandardSample, 1.0 / 3.0) - Math.Pow(ZStandardSample, 1.0 / 3.0));

                label6.Text = "True";
            }
            catch (Exception)
            {
                label6.Text = "False";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            measurementCount = Convert.ToInt32(textBox1.Text);
            label1.Text = "Count = " + measurementCount.ToString();
            for (int i = 0; i < measurementCount; i++)
            {
                List<int> list = new List<int>();
                dictionaryList.Add(i, list);
            }

            groupBox2.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (count != measurementCount)
                {
                    int section = (int)(((lastPixelD - firstPixelD) * (1.9868) / 5) + 1);
                    double[] pixelSectionDataValue = new double[section];
                    double[] RTestSample = new double[61];
                    double[] ETestSample = new double[61];
                    int firstPixel = (int)firstPixelD - 1;
                    int lastPixel = (int)lastPixelD;
                    int dataCount = lastPixel - firstPixel;
                    int dataLenght = 2049;

                    string sendData = "*MEASure:REFERence " + integrationTime + " " + averageScan + " format<CR>\r";
                    pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, choosingFilter, section, loopCountOther,
                                            dataLenght, dataCount, firstPixel, firstPixelD);
                    for (int i = 10; i < ETestSample.Length + 10; i++)
                        ETestSample[i - 10] = pixelSectionDataValue[i];

                    seriesIndex1 = chart6.Series.Count;
                    seriesIndex2 = seriesIndex2 + 1;
                    double graphicMinumumValue = Functions.GraphicMinimumMethod(pixelSectionDataValue);
                    double graphicMaximumValue = Functions.GraphicMaximumMethod(pixelSectionDataValue);
                    if (graphicMinumumValue < graphicMinimum)
                        graphicMinimum = (int)graphicMinumumValue + 1;
                    if (graphicMaximumValue > graphicMaximum)
                        graphicMaximum = (int)graphicMaximumValue + 1;
                    Functions.DrawGraphics(chart6, pixelSectionDataValue, "Test ", seriesIndex1, seriesIndex2, graphicMinimum, graphicMaximum, 0, 0, 255);

                    for (int i = 0; i < measurementLenght; i++)
                    {
                        int changeValue = Convert.ToInt32(ETestSample[i]);
                        dictionaryList[count].Add(changeValue);
                    }
                    count++;

                    label2.Text = count.ToString();
                }
                else
                {
                    label2.Text = "Full";
                    button2.Enabled = false;
                    groupBox3.Enabled = true;
                    groupBox4.Enabled = true;
                    count = 0;
                }
            }
            catch
            {
                label2.Text = "False";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label3.Text = "Start";
            label3.ForeColor = Color.Green;
            taskState = true;
            int lastLoop = 0;

            arrayList.Add(measurementCount + 2);
            for (int i = 0; i < measurementLenght; i++)
                arrayList.Add(0);

            for (int j = 0; j < measurementCount; j++)
            {
                for (int k = 0; k < measurementLenght; k++)
                    arrayList.Add(dictionaryList[j][k]);
            }

            for (int m = 0; m < measurementLenght; m++)
                arrayList.Add(0);

            foreach (var webSocketSession in webSocketSessions)
                webSocketSession.Value.Send(JsonConvert.SerializeObject(arrayList));
            Thread.Sleep(200);

            Task task = new Task(() =>
            {
                while (true)
                {
                    if (taskState == true)
                    {
                        for (int n = 0; n < measurementLenght; n++)
                            arrayList.RemoveAt(arrayList.Count - 1);

                        for (int p = 1 + measurementLenght; p < measurementLenght + measurementLenght + 1; p++)
                            arrayList.RemoveAt(p);

                        for (int r = 0; r < measurementLenght; r++)
                            arrayList.Add(dictionaryList[lastLoop - 1][r]);

                        for (int v = 0; v < measurementLenght; v++)
                            arrayList.Add(0);

                        foreach (var webSocketSession in webSocketSessions)
                        {
                            webSocketSession.Value.Send(JsonConvert.SerializeObject(arrayList));
                            Thread.Sleep(200);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });
            task.Start();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            taskState = false;
            label3.Text = "False";
            label3.ForeColor = Color.Red;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            graphicMaximum = 0;
            graphicMinimum = 0;

            seriesIndex1 = 0;
            seriesIndex2 = 0;
            chart6.Series.Clear();
            chart7.Series["Series1"].Points.Clear();

            label10.Text = "X";
        }

        private void WebSocketServer_NewSessionConnected(WebSocketSession webSocketSession)
        {
            if (webSocketSessions.ContainsKey(webSocketSession.SessionID))
                webSocketSessions[webSocketSession.SessionID] = webSocketSession;
            else
                webSocketSessions.Add(webSocketSession.SessionID, webSocketSession);
        }

        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", 
                   feature), RegistryKeyPermissionCheck.ReadWriteSubTree))
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
        }

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 11000;
            switch (browserVersion)
            {
                case 7:
                    mode = 7000;
                    break;
                case 8:
                    mode = 8000;
                    break;
                case 9:
                    mode = 9000;
                    break;
                case 10:
                    mode = 10000;
                    break;
                default:
                    break;
            }

            return mode;
        }

        public void FormConfig()
        {
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private void Connection()
        {
            ReadDeviceSettings();
            ReadParameterSettings();
            ReadOtherSettings();

            if (selectDevicePortName == "1")
            {
                Functions.CloseSerialPort(serialPort1);
                label7.Text = "Disconnection";
            }
            else
            {
                serialPort1.PortName = selectDevicePortName;
                Functions.OpenSerialPort(serialPort1, selectDevicePortName, 3000000);
                label7.Text = "Connecting...";
            }

            if (!serialPort1.IsOpen)
            {
                selectDevicePortName = "1";
                label7.Text = "Disconnection";
            }
            else
            {
                label7.Text = "Connected";
            }
        }

        private void ReadDeviceSettings()
        {
            string fileName = "DeviceSettings.txt";
            int textCount = 9;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 2);

            selectDevicePortName = readData[0];
            firstPixelD = Convert.ToDouble(readData[1]);
            lastPixelD = Convert.ToDouble(readData[2]);
        }

        private void ReadParameterSettings()
        {
            string fileName = "ParameterSettings.txt";
            int textCount = 4;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 3);

            integrationTime = readData[0];
            averageScan = readData[1];
            digitalGain = readData[2];
            analogGain = readData[3];
        }

        private void ReadOtherSettings()
        {
            string fileName = "OtherSettings.txt";
            int textCount = 3;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 2);

            loopCountTest = Convert.ToInt32(readData[0]);
            loopCountOther = Convert.ToInt32(readData[1]);
            if (readData[2] == "Normal")
                choosingFilter = 0;
            else if (readData[2] == "MOA")
                choosingFilter = 1;
        }

        delegate void SetListBoxCallback(ListBox listBox, string text);
        private void SetListBox(ListBox listBox, string text)
        {
            if (listBox.InvokeRequired)
            {
                SetListBoxCallback d = new SetListBoxCallback(_SetListBox);
                listBox.Invoke(d, new object[] { listBox, text });
            }
            else
            {
                _SetListBox(listBox, text);
            }
        }

        private void _SetListBox(ListBox listBox, string text)
        {
            listBox.Items.Add(text);
        }  
    }
}
