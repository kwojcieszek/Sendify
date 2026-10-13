using System.Net.Sockets;

namespace Sendify.MessagesServiceSmsDigiWr21;

public class TerminalTcp : IStream, IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    public bool IsAvailable => _client is { Connected: true };

    public TerminalTcp(string host, int port = 23)
    {
        _host = host;
        _port = port;
    }

    public int Read()
    {
        if (_client == null || !_client.Connected)
        {
            _client = new TcpClient();
            _client.Connect(_host, _port);
        }

        var stream = _client.GetStream();
        var buffer = new byte[1];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        return bytesRead > 0 ? buffer[0] : -1;
    }

    public void Write(string text)
    {
        if (_client == null || !_client.Connected)
        {
            _client = new TcpClient();
            _client.Connect(_host, _port);
        }

        var stream = _client.GetStream();
        var data = System.Text.Encoding.ASCII.GetBytes(text);

        stream.Write(data, 0, data.Length);
        stream.Flush();
    }

    public void Close()
    {
        if (_client != null)
        {
            if (_client.Connected)
            {
                _client.GetStream().Close();
            }

            _client.Close();
            _client.Dispose();
            _client = null;
        }
    }

    public void Dispose()
    {
        _client?.Close();
        _client?.Dispose();
    }
}