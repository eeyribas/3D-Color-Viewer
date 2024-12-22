using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace SpectrometerColorChart
{
    public static class Functions
    {
        public static void OpenSerialPort(SerialPort serialPort, string selectPortName, int baudRate)
        {
            serialPort.PortName = selectPortName;
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = 5000;
            serialPort.WriteTimeout = 5000;

            serialPort.Open();
        }

        public static void CloseSerialPort(SerialPort serialPort)
        {
            serialPort.Close();
        }

        public static double[] WaveCalculation(SerialPort serialPort, string sendData, int choosingFilter, int section, int loopCount, int dataLenght, int dataCount, int firstPixel, double firstPixelD)
        {
            double[] pixelDataValue = new double[dataCount];
            double[] pixelSectionDataValue = new double[section];
            double[] nmDataValue = new double[dataCount];
            int m = 0, n = 0;
            double searchNumber = 2;

            pixelDataValue = LoopPixelDataProcess(serialPort, sendData, choosingFilter, loopCount, dataLenght, dataCount, firstPixel);
            nmDataValue = NmCalculation(dataCount, firstPixel, firstPixelD);

            while (m < nmDataValue.Length)
            {
                searchNumber = Math.Abs(nmDataValue[m] - (350 + (n * 5)));
                if (searchNumber < 1)
                {
                    pixelSectionDataValue[n] = pixelDataValue[m];
                    m = 0;
                    n++;
                }
                else
                {
                    m++;
                }
            }

            return pixelSectionDataValue;
        }

        public static double[] LoopPixelDataProcess(SerialPort serialPort, string sendData, int choosingFilter, int loopCount, int dataLenght, int dataCount, int firstPixel)
        {
            double[] pixelDataValue = new double[dataCount];
            int[] dataValue = new int[dataCount];

            for (int i = 0; i < loopCount; i++)
            {
                if (choosingFilter == 0)
                    dataValue = NormalDataProcess(serialPort, sendData, dataLenght, dataCount, firstPixel);
                else if (choosingFilter == 1)
                    dataValue = MOADataProcess(serialPort, sendData, dataLenght, dataCount, firstPixel);

                for (int j = 0; j < dataCount; j++)
                    pixelDataValue[j] = pixelDataValue[j] + dataValue[j];
            }

            for (int k = 0; k < dataCount; k++)
                pixelDataValue[k] = pixelDataValue[k] / loopCount;

            return pixelDataValue;
        }

        public static int[] NormalDataProcess(SerialPort serialPort, string sendData, int dataLenght, int dataCount, int firstPixel)
        {
            int dataHexLenght = dataCount * 2;
            int dataCountTwo = dataLenght * 2;
            string[] dataHex = new string[dataHexLenght];
            int[] dataPixel = new int[dataCount];
            byte[] buffer = new byte[dataCountTwo];
            string dataA = "";
            string dataB = "";
            int receiverCount = 0;
            int pixelHex = 0;
            int m = 0, n = 0;

            serialPort.Write(sendData);
            while (dataCountTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataCountTwo);
                receiverCount += n;
                dataCountTwo -= n;
            }

            for (int i = 0; i < dataHex.Length; i++)
            {
                pixelHex = (firstPixel * 2) + i;
                dataA = buffer[pixelHex].ToString("X");
                if (dataA.Length != 2)
                {
                    dataA = "0" + dataA;
                    dataHex[i] = dataA;
                }
                else
                {
                    dataHex[i] = dataA;
                }
            }

            for (int j = 0; j < dataHex.Length; j += 2)
            {
                dataB = dataHex[j] + dataHex[j + 1];
                dataPixel[m] = Int32.Parse(dataB, NumberStyles.HexNumber);
                m++;
            }

            return dataPixel;
        }

        public static int[] MOADataProcess(SerialPort serialPort, string sendData, int dataLenght, int dataCount, int firstPixel)
        {
            int dataLenghtTwo = dataLenght * 2;
            string[] dataHex = new string[dataLenghtTwo];
            byte[] buffer = new byte[dataLenghtTwo];
            int[] dataValue = new int[dataLenght];
            int[] dataPixel = new int[dataCount];
            string dataA = "";
            int receiverCount = 0;
            string pixelHex = "";
            int m = 0, n = 0, o = 0;

            serialPort.Write(sendData);
            while (dataLenghtTwo > 0)
            {
                n = serialPort.Read(buffer, receiverCount, dataLenghtTwo);
                receiverCount += n;
                dataLenghtTwo -= n;
            }

            for (int i = 0; i < dataHex.Length; i++)
            {
                dataA = buffer[i].ToString("X");
                if (dataA.Length != 2)
                {
                    dataA = "0" + dataA;
                    dataHex[i] = dataA;
                }
                else
                {
                    dataHex[i] = dataA;
                }
            }

            for (int j = 0; j < dataHex.Length; j += 2)
            {
                pixelHex = dataHex[j] + dataHex[j + 1];
                dataValue[m] = Int32.Parse(pixelHex, NumberStyles.HexNumber);
                m++;
            }

            for (int k = firstPixel; k < dataCount + firstPixel; k++)
            {
                dataPixel[o] = (dataValue[k - 4] + dataValue[k - 3] + dataValue[k - 2] + dataValue[k - 1] + dataValue[k]) / 5;
                o++;
            }

            return dataPixel;
        }

        public static double[] NmCalculation(int dataCount, int firstPixel, double firstPixelD)
        {
            double slope = 1.9868;
            firstPixel = (int)firstPixelD - 1;
            double[] sumNmDataValue = new double[dataCount];
            for (int i = 0; i < dataCount; i++)
                sumNmDataValue[i] = Math.Round((((firstPixel + i) - firstPixelD) * slope) + 350, 2);

            return sumNmDataValue;
        }

        public static double XYZCalculation(double[] RStandardSample, double[] d65, double[] a10, double[] y10, double deltaLamda)
        {
            double xyzMulti = 0;
            double xyzSample = 0;
            for (int i = 0; i < a10.Length; i++)
                xyzMulti = xyzMulti + (RStandardSample[i] * d65[i] * a10[i] * deltaLamda);
            xyzSample = xyzMulti * KFunction(d65, y10, deltaLamda);

            return xyzSample;
        }

        public static double KFunction(double[] d65, double[] y10, double deltaLamda)
        {
            double sumK = 0;
            double sumResult = 0;
            for (int i = 0; i < d65.Length; i++)
                sumK = sumK + (d65[i] * y10[i] * deltaLamda);
            sumResult = 100 / sumK;

            return sumResult;
        }

        public static int DigitalGainSetting(SerialPort serialPort, string choosingDigitalGain)
        {
            string digitalGain = "";
            if (choosingDigitalGain == "0")
                digitalGain = "*PARAmeter:PDAGain " + "0" + "<CR>\r";
            else if (choosingDigitalGain == "1")
                digitalGain = "*PARAmeter:PDAGain " + "1" + "<CR>\r";
            else
                digitalGain = "*PARAmeter:PDAGain " + "0" + "<CR>\r";

            serialPort.Write(digitalGain);
            int validation = serialPort.ReadByte();

            return validation;
        }

        public static int AnalogGainSetting(SerialPort serialPort, string analogGainValue)
        {
            string analogGain = "*PARAmeter:GAIN " + analogGainValue + "<CR>\r";
            serialPort.Write(analogGain);
            int validation = serialPort.ReadByte();

            return validation;
        }

        public static void ReadFile(string fileName, int textCount, string[] texts, int firstText, int lastText)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            int k = 0, m = 0;
            string[] textData = new string[textCount];

            string text = streamReader.ReadLine();
            while (text != null)
            {
                textData[k] = text;
                text = streamReader.ReadLine();
                k++;
            }

            streamReader.Close();
            fileStream.Close();

            for (int i = firstText; i <= lastText; i++)
            {
                texts[m] = textData[i];
                m++;
            }
        }

        public static void WriteFile(string fileName, string[] texts)
        {
            File.WriteAllText(fileName, String.Empty);
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            for (int i = 0; i < texts.Length; i++)
                streamWriter.WriteLine(texts[i]);
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        public static void DrawGraphics(Chart chart, double[] pixelSectionDataValue, string graphicName, int seriesIndex, int seriesIndexType, 
                                        int graphicMinimum, int graphicMaximum, int aColor, int bColor, int cColor)
        {
            Random random = new Random();

            chart.Series.Add(graphicName + (seriesIndexType).ToString());
            chart.ChartAreas["ChartArea1"].AxisX.Interval = 10;
            chart.ChartAreas["ChartArea1"].AxisX.Minimum = 400;
            chart.ChartAreas["ChartArea1"].AxisX.Maximum = 700;
            chart.ChartAreas["ChartArea1"].AxisY.Minimum = graphicMinimum;
            chart.ChartAreas["ChartArea1"].AxisY.Maximum = graphicMaximum;
            for (int j = 10; j < 71; j++)
            {
                chart.Series[seriesIndex].ChartType = SeriesChartType.Line;
                chart.Series[seriesIndex].Color = Color.FromArgb(random.Next(0, aColor), random.Next(0, bColor), random.Next(0, cColor));
                chart.Series[seriesIndex].Points.AddXY(350 + (j * 5), pixelSectionDataValue[j]);
            }
        }

        public static double GraphicMinimumMethod(double[] pixelSectionDataValue)
        {
            double graphicsMinumumValue = pixelSectionDataValue[0];
            for (int i = 0; i < pixelSectionDataValue.Length; i++)
            {
                if (graphicsMinumumValue > pixelSectionDataValue[i])
                    graphicsMinumumValue = pixelSectionDataValue[i];
            }

            return graphicsMinumumValue;
        }

        public static double GraphicMaximumMethod(double[] pixelSectionDataValue)
        {
            double graphicsMaximumValue = pixelSectionDataValue[0];
            for (int i = 0; i < pixelSectionDataValue.Length; i++)
            {
                if (graphicsMaximumValue < pixelSectionDataValue[i])
                    graphicsMaximumValue = pixelSectionDataValue[i];
            }

            return graphicsMaximumValue;
        }
    }
}
