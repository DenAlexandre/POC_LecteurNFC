using CoreNFC;
using Foundation;
using NfcSecurePoc.Apdu;
using NfcSecurePoc.Crypto;
using NfcSecurePoc.Services;
using ObjCRuntime;
using System.Text;

namespace NfcSecurePoc.Platforms.iOS.Services;

/// <summary>
/// iOS NFC reader using NFCTagReaderSession with ISO 14443.
/// </summary>
public class IosNfcReaderService : NSObject, INfcReaderService, INFCTagReaderSessionDelegate
{
    private readonly AesCryptoService _crypto = new();
    private NFCTagReaderSession? _session;
    private bool _isScanning;

    public bool IsSupported => NFCTagReaderSession.ReadingAvailable;
    public bool IsScanning => _isScanning;

    public event EventHandler<TagDiscoveredEventArgs>? TagDiscovered;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<string>? Error;

    public void StartScanning()
    {
        _session = new NFCTagReaderSession(NFCPollingOption.Iso14443, this, null);
        _session.AlertMessage = "Hold your device near the NFC tag";
        _session.BeginSession();
        _isScanning = true;
    }

    public void StopScanning()
    {
        _session?.InvalidateSession();
        _session = null;
        _isScanning = false;
    }

    public void DidDetectTags(NFCTagReaderSession session, INFCTag[] tags)
    {
        if (tags.Length == 0) return;

        var tag = tags[0];

        if (tag.Type != NFCTagType.Iso7816Compatible)
        {
            session.InvalidateSession("Tag does not support ISO 7816");
            return;
        }

        var iso7816Tag = Runtime.GetINativeObject<INFCIso7816Tag>(tag.Handle, false);
        if (iso7816Tag == null)
        {
            session.InvalidateSession("Failed to get ISO 7816 interface");
            return;
        }

        session.ConnectTo(tag, (error) =>
        {
            if (error != null)
            {
                session.InvalidateSession($"Connection failed: {error.LocalizedDescription}");
                return;
            }

            var identifier = iso7816Tag.Identifier?.ToArray() ?? Array.Empty<byte>();
            var tagId = BitConverter.ToString(identifier).Replace("-", ":");

            MainThread.BeginInvokeOnMainThread(() =>
                TagDiscovered?.Invoke(this, new TagDiscoveredEventArgs(tagId, "ISO 7816")));

            PerformApduExchange(session, iso7816Tag);
        });
    }

    private void PerformApduExchange(NFCTagReaderSession session, INFCIso7816Tag tag)
    {
        // 1. SELECT AID
        var selectApdu = new NFCIso7816Apdu(
            ApduConstants.ClaIso,
            ApduConstants.InsSelect,
            ApduConstants.P1SelectByName,
            ApduConstants.P2SelectFirst,
            NSData.FromArray(ApduConstants.Aid),
            -1);

        tag.SendCommand(selectApdu, (data, sw1, sw2, error) =>
        {
            if (error != null)
            {
                InvalidateWithError(session, $"SELECT error: {error.LocalizedDescription}");
                return;
            }

            var sw = (ushort)((sw1 << 8) | sw2);
            if (sw != ApduConstants.SwSuccess)
            {
                InvalidateWithError(session, $"SELECT failed: SW=0x{sw:X4}");
                return;
            }

            var version = Encoding.UTF8.GetString(data?.ToArray() ?? Array.Empty<byte>());
            RaiseLog($"SELECT OK: {version}");

            // 2. GET DATA
            var getDataApdu = new NFCIso7816Apdu(
                ApduConstants.ClaIso,
                ApduConstants.InsGetData,
                0x00, 0x00,
                null,
                -1);

            tag.SendCommand(getDataApdu, (getData, gdSw1, gdSw2, gdError) =>
            {
                if (gdError != null)
                {
                    InvalidateWithError(session, $"GET DATA error: {gdError.LocalizedDescription}");
                    return;
                }

                var gdSw = (ushort)((gdSw1 << 8) | gdSw2);
                if (gdSw != ApduConstants.SwSuccess)
                {
                    InvalidateWithError(session, $"GET DATA failed: SW=0x{gdSw:X4}");
                    return;
                }

                var encryptedData = getData?.ToArray() ?? Array.Empty<byte>();

                try
                {
                    var decryptedBytes = _crypto.Decrypt(encryptedData);
                    var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                    MainThread.BeginInvokeOnMainThread(() =>
                        DataReceived?.Invoke(this, new DataReceivedEventArgs(encryptedData, decryptedText)));

                    // 3. PUT DATA
                    var responsePayload = Encoding.UTF8.GetBytes($"ACK: {decryptedText}");
                    var encrypted = _crypto.Encrypt(responsePayload);

                    var putDataApdu = new NFCIso7816Apdu(
                        ApduConstants.ClaIso,
                        ApduConstants.InsPutData,
                        0x00, 0x00,
                        NSData.FromArray(encrypted),
                        -1);

                    tag.SendCommand(putDataApdu, (pdData, pdSw1, pdSw2, pdError) =>
                    {
                        if (pdError != null)
                        {
                            InvalidateWithError(session, $"PUT DATA error: {pdError.LocalizedDescription}");
                            return;
                        }

                        var pdSw = (ushort)((pdSw1 << 8) | pdSw2);
                        if (pdSw != ApduConstants.SwSuccess)
                        {
                            InvalidateWithError(session, $"PUT DATA failed: SW=0x{pdSw:X4}");
                            return;
                        }

                        session.InvalidateSession("APDU exchange completed successfully");
                        RaiseLog("Full APDU exchange completed");
                    });
                }
                catch (Exception ex)
                {
                    InvalidateWithError(session, $"Decryption error: {ex.Message}");
                }
            });
        });
    }

    private void InvalidateWithError(NFCTagReaderSession session, string message)
    {
        session.InvalidateSession(message);
        RaiseLog(message);
    }

    private void RaiseLog(string message) =>
        MainThread.BeginInvokeOnMainThread(() => Error?.Invoke(this, message));

    public void DidInvalidate(NFCTagReaderSession session, NSError error)
    {
        _isScanning = false;
        if (error != null && error.Code != (long)NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead
            && error.Code != (long)NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
        {
            RaiseLog($"Session invalidated: {error.LocalizedDescription}");
        }
    }
}
