using System.IO.Ports;

namespace Sendify.MessageManagerSmsDigiWr21;

public class TerminalSerialPort : IStream
{
    private readonly SerialPort _serialPort;

    public bool IsAvailable => _serialPort.IsOpen;

    public TerminalSerialPort(string portName)
    {
        _serialPort = new SerialPort()
        {
            PortName = portName,
            BaudRate = 115200,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            RtsEnable = false
        };
    }

    public int Read()
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.Open();

            _serialPort.Write("Login\r\n");
        }

        return _serialPort.BytesToRead > 0 ? _serialPort.ReadByte() : -1;
    }

    public void Write(string text)
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.Open();
        }

        _serialPort.Write(text);
    }

    public void Close()
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }
}