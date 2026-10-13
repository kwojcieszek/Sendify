namespace Sendify.MessagesServiceSmsDigiWr21;

internal class Wr21StreamService
{
    private readonly IStream _stream;

    public Wr21StreamService(IStream stream)
    {
        _stream = stream;
    }

    public string ReadLineOrColonOrGreater()
    {
        try
        {
            var buffer = new List<char>();

            while (true)
            {
                var c = _stream.Read();

                if (c > 0)
                {
                    buffer.Add((char)c);
                }

                if (c == -1 || c == '\n' || c == ':' || c == '>' || buffer.Count > 255)
                {
                    return new string(buffer.ToArray());
                }
            }
        }
        catch (Exception e)
        {
            _stream?.Close();

            throw new InvalidOperationException("Error reading from stream.", e);
        }
    }

    public void WriteLine(string line)
    {
        try
        {
            _stream.Write(line + "\r\n");
        }
        catch (Exception e)
        {
            _stream?.Close();

            throw new InvalidOperationException("Error writing to stream.", e);
        }
    }
}