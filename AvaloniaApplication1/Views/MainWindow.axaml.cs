using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ScottPlot.Avalonia;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private AvaPlot _plot;
        private double[] _dataBuffer;  // 예: 고정 길이 버퍼
        private int _bufferIndex = 0;
        private const int BufferSize = 1000;

        readonly ScottPlot.Plottables.DataLogger Logger1;

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
            _plot = this.Find<AvaPlot>("AvaPlot1");
            _dataBuffer = new double[BufferSize];

            Logger1 = _plot.Plot.Add.DataLogger();

            SetupSerial("COM3", 115200);
        }

        private void SetupSerial(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                //_serialPort.Open();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine();
                if (double.TryParse(line, out double val))
                {
                    // 버퍼에 저장 (순환 방식)
                    _dataBuffer[_bufferIndex] = val;
                    _bufferIndex = (_bufferIndex + 1) % BufferSize;

                    // UI 스레드에서 그래프 갱신
                    Dispatcher.UIThread.Post(() => UpdatePlot());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial read error: {ex.Message}");
            }
        }

        private void UpdatePlot()
        {
            // X축 데이터 생성 (예: 인덱스 기반)
            double[] xs = new double[BufferSize];
            for (int i = 0; i < BufferSize; i++)
                xs[i] = i;

            _plot.Plot.Clear();
            Logger1.Add(_dataBuffer);
            _plot.Refresh();
        }
    }
}