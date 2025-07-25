namespace Sendify.MessageManagerSmsDigiWr21;

public interface IStream
{
    int Read();
    void Write(string text);
    void Close();
    bool IsAvailable { get; }
}