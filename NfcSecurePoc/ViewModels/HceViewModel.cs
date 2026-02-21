using System.Windows.Input;
using NfcSecurePoc.Services;

namespace NfcSecurePoc.ViewModels;

public class HceViewModel : BaseViewModel
{
    private readonly INfcHceService _hceService;
    private string _payload = "Hello from NFC-POC!";
    private bool _isActive;
    private string _statusText = "Stopped";

    public string Payload
    {
        get => _payload;
        set
        {
            if (SetProperty(ref _payload, value))
                _hceService.Payload = value;
        }
    }

    public bool IsActive
    {
        get => _isActive;
        private set
        {
            SetProperty(ref _isActive, value);
            StatusText = value ? "Emulating card..." : "Stopped";
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsSupported => _hceService.IsSupported;

    public ICommand ToggleCommand { get; }

    public HceViewModel(INfcHceService hceService)
    {
        _hceService = hceService;

        _hceService.ApduReceived += OnApduReceived;
        _hceService.ApduResponseSent += OnApduResponseSent;

        ToggleCommand = new Command(Toggle);
    }

    private void Toggle()
    {
        if (!_hceService.IsSupported)
        {
            AppendLog("HCE not supported on this platform");
            return;
        }

        if (IsActive)
        {
            _hceService.Stop();
            IsActive = false;
            AppendLog("HCE stopped");
        }
        else
        {
            _hceService.Payload = _payload;
            _hceService.Start();
            IsActive = true;
            AppendLog($"HCE started with payload: \"{_payload}\"");
        }
    }

    private void OnApduReceived(object? sender, ApduReceivedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            AppendLog($"← RX: {e.Description} [{BytesToHex(e.CommandApdu)}]"));
    }

    private void OnApduResponseSent(object? sender, ApduResponseSentEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
            AppendLog($"→ TX: {e.Description} [{BytesToHex(e.ResponseApdu)}]"));
    }
}
