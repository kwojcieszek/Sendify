namespace Sendify.MessagesServiceSmsDigiWr21;

public class SimplyLogger
{
    private readonly string _logDirectory;
    private string _currentLogFile;
    private DateTime _currentDate;

    public SimplyLogger(string logDirectory)
    {
        _logDirectory = logDirectory;

        Directory.CreateDirectory(_logDirectory);

        UpdateLogFile();
    }

    private void UpdateLogFile()
    {
        _currentDate = DateTime.Today;

        _currentLogFile = Path.Combine(_logDirectory,
            $"log_{_currentDate:yyyy-MM-dd}.txt");
    }

    public void Log(string message)
    {
        if (DateTime.Today != _currentDate)
        {
            UpdateLogFile();
        }

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

        File.AppendAllText(_currentLogFile, line + Environment.NewLine);
    }
}