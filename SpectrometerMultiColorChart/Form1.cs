using SpectrometerMultiColorChart.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectrometerMultiColorChart
{
    public partial class Form1 : Form
    {
        public List<int> arrayList = new List<int>();
        public int measurementCount = 0;
        public int measurementLenght = 61;

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(562, 420);
            this.StartPosition = FormStartPosition.CenterScreen;

            ReadSettings();
            GroupBoxFalse();
            LabelReset();
            Connection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                label27.Text = "Default";
                label27.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((Parameters.lastPixelD - Parameters.firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                int firstPixel = (int)Parameters.firstPixelD - 1;
                int lastPixel = (int)Parameters.lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;

                if (serialPort1.IsOpen)
                {
                    if (Functions.DigitalGainSetting(serialPort1, Parameters.digitalGain) == 6 && 
                        Functions.AnalogGainSetting(serialPort1, Parameters.analogGain) == 6)
                    {
                        label27.Text = "True";
                        label27.ForeColor = Color.Green;
                    }
                    else
                    {
                        label27.Text = "False";
                        label27.ForeColor = Color.Red;
                    }

                    string sendData = "*MEASure:DARKspectra " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
                    pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, Parameters.choosingFilter, section, Parameters.loopCountOther, 
                                            dataLenght, dataCount, firstPixel, Parameters.firstPixelD);
                    if (label27.Text != "False")
                    {
                        label27.Text = "True";
                        label27.ForeColor = Color.Green;
                    }
                }
                else
                {
                    label27.Text = "Close";
                    label27.ForeColor = Color.Red;
                }
            }
            catch (Exception)
            {
                label27.Text = "False";
                label27.ForeColor = Color.Red;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                label28.Text = "Default";
                label28.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((Parameters.lastPixelD - Parameters.firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                double[] ERefLed = new double[61];
                string[] ERsLedString = new string[61];
                int firstPixel = (int)Parameters.firstPixelD - 1;
                int lastPixel = (int)Parameters.lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;

                if (serialPort1.IsOpen)
                {
                    string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
                    pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, Parameters.choosingFilter, section, Parameters.loopCountOther, 
                                            dataLenght, dataCount, firstPixel, Parameters.firstPixelD);

                    for (int i = 10; i < ERefLed.Length + 10; i++)
                        ERefLed[i - 10] = pixelSectionDataValue[i];
                    for (int j = 0; j < Parameters.ERsLed.Length; j++)
                        Parameters.ERsLed[j] = ERefLed[j] / Parameters.standartPlate[j];

                    for (int k = 0; k < Parameters.ERsLed.Length; k++)
                        ERsLedString[k] = Parameters.ERsLed[k].ToString();
                    Functions.WriteFile("PlateValues.txt", ERsLedString);

                    label28.Text = "True";
                    label28.ForeColor = Color.Green;
                }
                else
                {
                    label28.Text = "False";
                    label28.ForeColor = Color.Red;
                }
            }
            catch (Exception)
            {
                label28.Text = "False";
                label28.ForeColor = Color.Red;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                label29.Text = "Default";
                label29.ForeColor = Color.Brown;
                this.Refresh();
                Thread.Sleep(200);

                int section = (int)(((Parameters.lastPixelD - Parameters.firstPixelD) * (1.9868) / 5) + 1);
                double[] pixelSectionDataValue = new double[section];
                double[] RStandardSample = new double[61];
                double[] EStandardSample = new double[61];
                string[] ERsLedString = new string[61];
                int firstPixel = (int)Parameters.firstPixelD - 1;
                int lastPixel = (int)Parameters.lastPixelD;
                int dataCount = lastPixel - firstPixel;
                int dataLenght = 2049;

                Functions.ReadFile("PlateValues.txt", 61, ERsLedString, 0, 60);
                for (int k = 0; k < Parameters.ERsLed.Length; k++)
                    Parameters.ERsLed[k] = Convert.ToDouble(ERsLedString[k]);

                string sendData = "*MEASure:REFERence " + Parameters.integrationTime + " " + Parameters.averageScan + " format<CR>\r";
                pixelSectionDataValue = Functions.WaveCalculation(serialPort1, sendData, Parameters.choosingFilter, section, Parameters.loopCountOther, 
                                        dataLenght, dataCount, firstPixel, Parameters.firstPixelD);

                for (int i = 10; i < EStandardSample.Length + 10; i++)
                    EStandardSample[i - 10] = pixelSectionDataValue[i];
                for (int j = 0; j < RStandardSample.Length; j++)
                    RStandardSample[j] = EStandardSample[j] / Parameters.ERsLed[j];

                double XStandardSample = Functions.XYZCalculation(RStandardSample, Parameters.d65, Parameters.x10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10x;
                double YStandardSample = Functions.XYZCalculation(RStandardSample, Parameters.d65, Parameters.y10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10y;
                double ZStandardSample = Functions.XYZCalculation(RStandardSample, Parameters.d65, Parameters.z10, Parameters.y10, Parameters.deltaLamda) / Parameters.w10z;
                Parameters.lStandardSample = (116 * Math.Pow(YStandardSample, 1.0 / 3.0)) - 16;
                Parameters.aStandardSample = 500 * (Math.Pow(XStandardSample, 1.0 / 3.0) - Math.Pow(YStandardSample, 1.0 / 3.0));
                Parameters.bStandardSample = 200 * (Math.Pow(YStandardSample, 1.0 / 3.0) - Math.Pow(ZStandardSample, 1.0 / 3.0));

                label29.Text = "True";
                label29.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                label29.Text = "False";
                label29.ForeColor = Color.Red;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Functions.CloseSerialPort(serialPort1);

            Form2 form2 = new Form2();
            form2.ShowDialog();
            this.Close();
        }

        private void ReadSettings()
        {
            string fileName = "Settings.txt";
            int textCount = 12;
            string[] readData = new string[textCount];
            Functions.ReadFile(fileName, textCount, readData, 0, 11);

            Parameters.selectDevicePort = readData[0];
            Parameters.firstPixelD = Convert.ToDouble(readData[1]);
            Parameters.lastPixelD = Convert.ToDouble(readData[2]);
            Parameters.integrationTime = readData[3];
            Parameters.averageScan = readData[4];
            Parameters.digitalGain = readData[5];
            Parameters.analogGain = readData[6];
            Parameters.loopCountTest = Convert.ToInt32(readData[7]);
            Parameters.loopCountOther = Convert.ToInt32(readData[8]);
            if (readData[9] == "Normal")
                Parameters.choosingFilter = 0;
            else if (readData[9] == "MOA")
                Parameters.choosingFilter = 1;
            Parameters.measurementCount = Convert.ToInt32(readData[10]);
            Parameters.threadTime = Convert.ToInt32(readData[11]);
        }

        private void GroupBoxFalse()
        {
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
        }

        private void LabelReset()
        {
            label14.Text = "X";
            label15.Text = "X";
            label16.Text = "X";
            label17.Text = "X";
            label18.Text = "X";
            label19.Text = "X";
            label20.Text = "X";
            label21.Text = "X";
            label22.Text = "X";
            label23.Text = "X";
            label26.Text = "X";
            label24.Text = "X";
            label25.Text = "X";

            label27.Text = "Default";
            label28.Text = "Default";
            label29.Text = "Default";
            label30.Text = "Default";
        }

        private void Connection()
        {
            ReadSettings();

            if (Parameters.selectDevicePort != "1")
            {
                serialPort1.PortName = Parameters.selectDevicePort;
                Functions.OpenSerialPort(serialPort1, Parameters.selectDevicePort, 3000000); 
            }
            else
            {
                Functions.CloseSerialPort(serialPort1);
            }

            if (serialPort1.IsOpen)
            {
                groupBox1.Enabled = true;
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
                groupBox4.Enabled = true;
                groupBox5.Enabled = true;

                LabelText();
                label26.Text = "Connected";
                label26.ForeColor = Color.Green;
            }
            else
            {
                Parameters.selectDevicePort = "1";

                GroupBoxFalse();

                label26.Text = "Disconnection";
                label26.ForeColor = Color.Red;
            }
        }

        private void LabelText()
        {
            label14.Text = Parameters.selectDevicePort;
            label15.Text = Parameters.firstPixelD.ToString();
            label16.Text = Parameters.lastPixelD.ToString();
            label17.Text = Parameters.integrationTime;
            label18.Text = Parameters.averageScan;
            label19.Text = Parameters.digitalGain;
            label20.Text = Parameters.analogGain;
            label21.Text = Parameters.loopCountTest.ToString();
            label22.Text = Parameters.loopCountOther.ToString();
            label23.Text = Parameters.choosingFilter.ToString();
            label24.Text = Parameters.measurementCount.ToString();
            label25.Text = Parameters.threadTime.ToString();
        }
    }
}
