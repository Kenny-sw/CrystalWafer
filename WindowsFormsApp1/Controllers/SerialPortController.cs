using System;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace CrystalTable.Controllers
{
    public class SerialPortController : IDisposable
    {
        private readonly SerialPort _port;

        // События для UI
        public event Action<string> DataReceived;                 // текстовые данные RX
        public event Action<bool, string> ConnectionStateChanged; // (isOpen, portName)

        public SerialPortController(SerialPort serialPort)
        {
            _port = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        }

        public void ToggleConnection(string portName, Button btnConnect, ComboBox portsCombo)
        {
            try
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                    if (btnConnect != null) btnConnect.Text = "Подключить";
                    ConnectionStateChanged?.Invoke(false, null);
                    return;
                }

                if (string.IsNullOrWhiteSpace(portName))
                {
                    MessageBox.Show("Выберите COM-порт.", "COM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _port.BaudRate = 115200;
                _port.Parity = Parity.None;
                _port.DataBits = 8;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.PortName = portName;

                _port.Open();
                if (btnConnect != null) btnConnect.Text = "Отключить";
                ConnectionStateChanged?.Invoke(true, _port.PortName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть порт: " + ex.Message, "COM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConnectionStateChanged?.Invoke(false, null);
            }
        }

        public void UpdatePortList(ComboBox combo)
        {
            if (combo == null) return;
            var list = SerialPort.GetPortNames().OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToArray();
            combo.Items.Clear();
            combo.Items.AddRange(list);
            if (list.Length > 0) combo.SelectedIndex = 0;
        }

        public void HandleDataReceived()
        {
            try
            {
                string data = _port.ReadExisting();
                if (!string.IsNullOrEmpty(data))
                    DataReceived?.Invoke(data);
            }
            catch { /* игнорируем спорадические ошибки чтения */ }
        }

        public void Dispose()
        {
            try
            {
                if (_port != null)
                {
                    if (_port.IsOpen) _port.Close();
                    _port.Dispose();
                }
            }
            catch { /* ignore */ }
        }
    }
}
