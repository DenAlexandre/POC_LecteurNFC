namespace NfcSecurePoc.Services;

/// <summary>
/// Interface for NFC reader mode service.
/// </summary>
public interface INfcReaderService
{
    /// <summary>
    /// Whether NFC reader is supported on this platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Whether the reader is currently scanning.
    /// </summary>
    bool IsScanning { get; }

    /// <summary>
    /// Start scanning for NFC tags/cards.
    /// </summary>
    void StartScanning();

    /// <summary>
    /// Stop scanning for NFC tags/cards.
    /// </summary>
    void StopScanning();

    /// <summary>
    /// Raised when a tag is discovered.
    /// </summary>
    event EventHandler<TagDiscoveredEventArgs>? TagDiscovered;

    /// <summary>
    /// Raised when data is received from a tag.
    /// </summary>
    event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Raised when an error occurs.
    /// </summary>
    event EventHandler<string>? Error;
}
