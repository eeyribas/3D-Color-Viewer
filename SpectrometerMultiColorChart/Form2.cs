using Microsoft.Win32;
using Newtonsoft.Json;
using SpectrometerMultiColorChart.Classes;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpectrometerMultiColorChart
{
    public partial class Form2 : Form
    {
        private string fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
        private static WebSocketServer webSocketServer;
        private Dictionary<string, WebSocketSession> webSocketSessions;

        public List<int> arrayList = new List<int>();
        public double[] ETestSample = new double[61];
        public int[] specMeasurementArray = new int[61];
        private int[] tmpArray = new int[794];
        public int deltaECount = 0;
        public const int measurementLenght = 61;
        public int specMeasurementCount = 0;

        public Thread graphicsThread;
        public bool graphicsThreadState = true;
        public Thread measurementThread;
        public bool measurementThreadState = true;
        public Thread deltaEThread;
        public bool deltaEThreadState = true;

        public Form2()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1920, 1080);

            webSocketSessions = new Dictionary<string, WebSocketSession>();
            webSocketServer = new WebSocketServer();
            webSocketServer.Setup(3435);
            webSocketServer.NewSessionConnected += WebSocketServer_NewSessionConnected;
            webSocketServer.Start();

            FormConfig();
            Connection();
            Data();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string url = Directory.GetCurrentDirectory() + @"\WebPage\chart.html";
            webBrowser1.Url = new Uri(url);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.BackColor = Color.Gray;
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
            if (Parameters.selectDevicePort != "1")
            {
                serialPort1.PortName = Parameters.selectDevicePort;
                Functions.OpenSerialPort(serialPort1, Parameters.selectDevicePort, 3000000);
            }
            else
            {
                Functions.CloseSerialPort(serialPort1);
            }

            if (!serialPort1.IsOpen)
            {
                Parameters.selectDevicePort = "1";
                MessageBox.Show("COM Port Connection Error", "Information Window");
            }
        }

        private void Data()
        {
            int lastLoop = 0;
            arrayList.Add(Parameters.measurementCount + 2);

            for (int i = 0; i < measurementLenght; i++)
                arrayList.Add(0);

            for (int j = 0; j < (Parameters.measurementCount * measurementLenght); j++)
                arrayList.Add(0);

            for (int k = 0; k < measurementLenght; k++)
                arrayList.Add(0);

            foreach (var webSocketSession in webSocketSessions)
                webSocketSession.Value.Send(JsonConvert.SerializeObject(arrayList));

            Thread.Sleep(50);

            measurementThreadState = true;
            if (measurementThread != null && measurementThread.IsAlive == true)
                return;
            measurementThread = new Thread(() => MeasurementThreadFunction(serialPort1));
            measurementThread.Start();

            deltaEThreadState = true;
            if (deltaEThread != null && deltaEThread.IsAlive == true)
                return;
            deltaEThread = new Thread(() => DeltaEThreadFunction(chart1, chart7));
            deltaEThread.Start();

            graphicsThreadState = true;
            if (graphicsThread != null && graphicsThread.IsAlive == true)
                return;
            graphicsThread = new Thread(() => GraphicsThreadFunction(lastLoop));
            graphicsThread.Start();
        }

        private void MeasurementThreadFunction(SerialPort serialPort)
        {
            while (true)
            {
                if (measurementThreadState == true)
                {
                    Thread.Sleep(Parameters.threadTime);

                    int section = (int)(((Parameters.lastPixelD - Parameters.firstPixelD) * (1.9868) / 5) + 1);
                    double[] pixelSectionDataValue = new double[section];
                    int firstPixel = (int)Parameters.firstPixelD - 1;
                    int lastPixel = (int)Parameters.lastPixelD;
                    int dataCount = lastPixel - firstPixel;
                    int dataLenght = 2049;

                    string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
                    pixelSectionDataValue = Functions.WaveCalculation(serialPort, sendData, Parameters.choosingFilter, section, Parameters.loopCountOther, 
                                            dataLenght, dataCount, firstPixel, Parameters.firstPixelD);

                    for (int i = 10; i < ETestSample.Length + 10; i++)
                        ETestSample[i - 10] = pixelSectionDataValue[i];
                }
                else
                {
                    break;
                }
            }
        }

        private void DeltaEThreadFunction(Chart ch1, Chart ch2)
        {
            while (true)
            {
                if (measurementThreadState == true)
                {
                    Thread.Sleep(Parameters.threadTime);

                    double[] RTestSample = new double[61];
                    for (int j = 0; j < RTestSample.Length; j++)
                        RTestSample[j] = ETestSample[j] / Parameters.ERsLed[j];

                    double XTestSample = Functions.XYZCalculation(RTestSample, Parameters.d65, Parameters.x10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10x;
                    double YTestSample = Functions.XYZCalculation(RTestSample, Parameters.d65, Parameters.y10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10y;
                    double ZTestSample = Functions.XYZCalculation(RTestSample, Parameters.d65, Parameters.z10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10z;
                    double lTestSample = (116 * Math.Pow(YTestSample, 1.0 / 3.0)) - 16;
                    double aTestSample = 500 * (Math.Pow(XTestSample, 1.0 / 3.0) - Math.Pow(YTestSample, 1.0 / 3.0));
                    double bTestSample = 200 * (Math.Pow(YTestSample, 1.0 / 3.0) - Math.Pow(ZTestSample, 1.0 / 3.0));

                    double lDifferenceAbs = lTestSample - Parameters.lStandardSample;
                    lDifferenceAbs = Math.Abs(lDifferenceAbs);
                    double lDifference = Math.Pow(lDifferenceAbs, 2);
                    SetLabel(label1, Math.Round(lDifferenceAbs, 2).ToString());
                    double aDifferenceAbs = aTestSample - Parameters.aStandardSample;
                    aDifferenceAbs = Math.Abs(aDifferenceAbs);
                    double aDifference = Math.Pow(aDifferenceAbs, 2);
                    SetLabel(label2, Math.Round(aDifferenceAbs, 2).ToString());
                    double bDifferenceAbs = bTestSample - Parameters.bStandardSample;
                    bDifferenceAbs = Math.Abs(bDifferenceAbs);
                    double bDifference = Math.Pow(bDifferenceAbs, 2);
                    SetLabel(label3, Math.Round(bDifferenceAbs, 2).ToString());
                    double deltaEResult = lDifference + aDifference + bDifference;
                    deltaEResult = Math.Sqrt(deltaEResult);
                    SetLabel(label4, Math.Round(deltaEResult, 2).ToString());

                    AddPointToChart(ch2, deltaECount, deltaEResult);
                    deltaECount++;

                    ClearChart(ch1);
                    SetChart(ch1, ETestSample);
                }
            }
        }

        private void GraphicsThreadFunction(int lastLoop)
        {
            while (true)
            {
                if (graphicsThreadState == true)
                {
                    List<int> tmpArrayList = new List<int>();

                    tmpArray[0] = 13;
                    for (int k = (tmpArray[0] - 3); k >= 1; k--)
                    {
                        for (int i = 0; i < 61; i++)
                            tmpArray[((k + 1) * 61) + i + 1] = tmpArray[(k * 61) + i + 1];
                    }

                    for (int i = 0; i < 61; i++)
                    {
                        tmpArray[(0 * 61) + i + 1] = 0;
                        tmpArray[(1 * 61) + i + 1] = Convert.ToInt32(ETestSample[i]);
                        tmpArray[((tmpArray[0] - 1) * 61) + i + 1] = 0;
                    }

                    for (int i = 0; i < 794; i++)
                        tmpArrayList.Add(tmpArray[i]);

                    foreach (var webSocketSession in webSocketSessions)
                    {
                        webSocketSession.Value.Send(JsonConvert.SerializeObject(tmpArrayList));
                        Thread.Sleep(Parameters.threadTime);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        delegate void AddPointToChartCallback(Chart chart, int time, double value);
        public void AddPointToChart(Chart chart, int time, double value)
        {
            if (chart.InvokeRequired)
            {
                AddPointToChartCallback d = new AddPointToChartCallback(_AddPointToChart);
                chart.Invoke(d, new object[] { chart, time, value });
            }
            else
            {
                _AddPointToChart(chart, time, value);
            }
        }

        private void _AddPointToChart(Chart chart, int time, double value)
        {
            if (chart.Series[0].Points.Count > 100)
                chart.Series[0].Points.RemoveAt(0);
            chart.Series[0].Points.Add(value);
        }

        delegate void SetChartCallback(Chart chart, double[] values);
        private void SetChart(Chart chart, double[] values)
        {
            if (chart.InvokeRequired)
            {
                SetChartCallback d = new SetChartCallback(_SetChart);
                chart.Invoke(d, new object[] { chart, values });
            }
            else
            {
                _SetChart(chart, values);
            }
        }

        private void _SetChart(Chart chart, double[] values)
        {
            for (int j = 0; j < values.Length; j++)
                chart.Series[0].Points.AddXY(400 + (j * 5), Convert.ToInt32(values[j]));
        }

        delegate void ClearChartCallback(Chart chart);
        private void ClearChart(Chart chart)
        {
            if (chart.InvokeRequired)
            {
                ClearChartCallback d = new ClearChartCallback(_ClearChart);
                chart.Invoke(d, new object[] { chart });
            }
            else
            {
                _ClearChart(chart);
            }
        }

        private void _ClearChart(Chart chart)
        {
            chart.Series[0].Points.Clear();
        }

        delegate void SetLabelCallback(Label label, string text);
        private void SetLabel(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                SetLabelCallback d = new SetLabelCallback(_SetLabel);
                label.Invoke(d, new object[] { label, text });
            }
            else
            {
                _SetLabel(label, text);
            }
        }

        private void _SetLabel(Label label, string text)
        {
            label.Text = text;
        }
    }
}
