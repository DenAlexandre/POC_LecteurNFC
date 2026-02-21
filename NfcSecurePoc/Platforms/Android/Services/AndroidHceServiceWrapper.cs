using NfcSecurePoc.Services;

namespace NfcSecurePoc.Platforms.Android.Services;

/// <summary>
/// Bridges the OS-managed AndroidHceService (static events) to the DI system (INfcHceService).
/// </summary>
public class AndroidHceServiceWrapper : INfcHceService
{
    private string _payload = "Hello from NFC-POC!";

    public bool IsSupported => true;
    public bool IsActive => AndroidHceService.IsActive;

    public string Payload
    {
        get => _payload;
        set
        {
            _payload = value;
            AndroidHceService.SetPayload(value);
        }
    }

    public event EventHandler<ApduReceivedEventArgs>? ApduReceived;
    public event EventHandler<ApduResponseSentEventArgs>? ApduResponseSent;

    public AndroidHceServiceWrapper()
    {
        AndroidHceService.StaticApduReceived += OnStaticApduReceived;
        AndroidHceService.StaticApduResponseSent += OnStaticApduResponseSent;
    }

    public void Start()
    {
        AndroidHceService.SetPayload(_payload);
        AndroidHceService.SetActive(true);
    }

    public void Stop()
    {
        AndroidHceService.SetActive(false);
    }

    private void OnStaticApduReceived(object? sender, (byte[] Apdu, string Description) e)
    {
        ApduReceived?.Invoke(this, new ApduReceivedEventArgs(e.Apdu, e.Description));
    }

    private void OnStaticApduResponseSent(object? sender, (byte[] Apdu, string Description) e)
    {
        ApduResponseSent?.Invoke(this, new ApduResponseSentEventArgs(e.Apdu, e.Description));
    }
}
