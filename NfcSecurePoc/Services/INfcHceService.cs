namespace NfcSecurePoc.Services;

/// <summary>
/// Interface for NFC Host Card Emulation (HCE) service.
/// </summary>
public interface INfcHceService
{
    /// <summary>
    /// Whether HCE is supported on this platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Whether the HCE service is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// The payload to serve when GET_DATA is received.
    /// </summary>
    string Payload { get; set; }

    /// <summary>
    /// Start emulating a card.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop emulating a card.
    /// </summary>
    void Stop();

    /// <summary>
    /// Raised when an APDU command is received.
    /// </summary>
    event EventHandler<ApduReceivedEventArgs>? ApduReceived;

    /// <summary>
    /// Raised when an APDU response is sent.
    /// </summary>
    event EventHandler<ApduResponseSentEventArgs>? ApduResponseSent;
}
