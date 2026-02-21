using NfcSecurePoc.Services;

namespace NfcSecurePoc.Platforms.iOS.Services;

/// <summary>
/// iOS HCE stub — Host Card Emulation is not supported on iOS.
/// Apple does not expose the HCE API.
/// </summary>
public class IosHceService : INfcHceService
{
    public bool IsSupported => false;
    public bool IsActive => false;

    public string Payload { get; set; } = string.Empty;

    public event EventHandler<ApduReceivedEventArgs>? ApduReceived;
    public event EventHandler<ApduResponseSentEventArgs>? ApduResponseSent;

    public void Start()
    {
        // HCE not available on iOS
    }

    public void Stop()
    {
        // HCE not available on iOS
    }
}
