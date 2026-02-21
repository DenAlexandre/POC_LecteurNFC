using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NfcSecurePoc.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private string _logs = string.Empty;

    public string Logs
    {
        get => _logs;
        set => SetProperty(ref _logs, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void AppendLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Logs = $"[{timestamp}] {message}\n{Logs}";
    }

    protected static string BytesToHex(byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", " ");
}
