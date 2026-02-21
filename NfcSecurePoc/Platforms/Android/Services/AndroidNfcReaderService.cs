using Android.App;
using Android.Nfc;
using Android.Nfc.Tech;
using NfcSecurePoc.Apdu;
using NfcSecurePoc.Crypto;
using NfcSecurePoc.Services;
using System.Text;

namespace NfcSecurePoc.Platforms.Android.Services;

/// <summary>
/// Android NFC reader using NfcAdapter.EnableReaderMode with ISO-DEP.
/// </summary>
public class AndroidNfcReaderService : Java.Lang.Object, INfcReaderService, NfcAdapter.IReaderCallback
{
    private readonly AesCryptoService _crypto = new();
    private NfcAdapter? _nfcAdapter;
    private bool _isScanning;

    public bool IsSupported => NfcAdapter.GetDefaultAdapter(Platform.CurrentActivity) != null;
    public bool IsScanning => _isScanning;

    public event EventHandler<TagDiscoveredEventArgs>? TagDiscovered;
    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<string>? Error;

    public void StartScanning()
    {
        var activity = Platform.CurrentActivity;
        if (activity == null)
        {
            Error?.Invoke(this, "No current activity");
            return;
        }

        _nfcAdapter = NfcAdapter.GetDefaultAdapter(activity);
        if (_nfcAdapter == null)
        {
            Error?.Invoke(this, "NFC not available on this device");
            return;
        }

        var flags = NfcReaderFlags.NfcA | NfcReaderFlags.NfcB | NfcReaderFlags.SkipNdefCheck;
        _nfcAdapter.EnableReaderMode(activity, this, flags, null);
        _isScanning = true;
    }

    public void StopScanning()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null && _nfcAdapter != null)
        {
            _nfcAdapter.DisableReaderMode(activity);
        }
        _isScanning = false;
    }

    public void OnTagDiscovered(Tag? tag)
    {
        if (tag == null) return;

        var tagId = BitConverter.ToString(tag.GetId() ?? Array.Empty<byte>()).Replace("-", ":");
        var techList = string.Join(", ", tag.GetTechList() ?? Array.Empty<string>());

        MainThread.BeginInvokeOnMainThread(() =>
            TagDiscovered?.Invoke(this, new TagDiscoveredEventArgs(tagId, techList)));

        // Try ISO-DEP communication
        var isoDep = IsoDep.Get(tag);
        if (isoDep == null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                Error?.Invoke(this, "Tag does not support ISO-DEP"));
            return;
        }

        Task.Run(() => PerformApduExchange(isoDep));
    }

    private void PerformApduExchange(IsoDep isoDep)
    {
        try
        {
            isoDep.Connect();
            isoDep.Timeout = 5000;

            // 1. SELECT AID
            var selectCmd = ApduCommand.Select(ApduConstants.Aid);
            var selectRaw = isoDep.Transceive(selectCmd)
                ?? throw new InvalidOperationException("No response from SELECT");
            var selectResp = new ApduResponse(selectRaw);

            if (!selectResp.IsSuccess)
            {
                RaiseError($"SELECT failed: SW=0x{selectResp.StatusWord:X4}");
                return;
            }

            var version = Encoding.UTF8.GetString(selectResp.Data);
            RaiseError($"SELECT OK: {version}");

            // 2. GET DATA (receive encrypted payload)
            var getDataCmd = ApduCommand.GetData();
            var getDataRaw = isoDep.Transceive(getDataCmd)
                ?? throw new InvalidOperationException("No response from GET DATA");
            var getDataResp = new ApduResponse(getDataRaw);

            if (!getDataResp.IsSuccess)
            {
                RaiseError($"GET DATA failed: SW=0x{getDataResp.StatusWord:X4}");
                return;
            }

            var decryptedBytes = _crypto.Decrypt(getDataResp.Data);
            var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            MainThread.BeginInvokeOnMainThread(() =>
                DataReceived?.Invoke(this, new DataReceivedEventArgs(getDataResp.Data, decryptedText)));

            // 3. PUT DATA (send encrypted response back)
            var responsePayload = Encoding.UTF8.GetBytes($"ACK: {decryptedText}");
            var encrypted = _crypto.Encrypt(responsePayload);
            var putDataCmd = ApduCommand.PutData(encrypted);
            var putDataRaw = isoDep.Transceive(putDataCmd)
                ?? throw new InvalidOperationException("No response from PUT DATA");
            var putDataResp = new ApduResponse(putDataRaw);

            if (!putDataResp.IsSuccess)
            {
                RaiseError($"PUT DATA failed: SW=0x{putDataResp.StatusWord:X4}");
                return;
            }

            RaiseError("PUT DATA OK — full APDU exchange completed");
        }
        catch (Exception ex)
        {
            RaiseError($"APDU exchange error: {ex.Message}");
        }
        finally
        {
            try { isoDep.Close(); } catch { /* ignore */ }
        }
    }

    private void RaiseError(string message) =>
        MainThread.BeginInvokeOnMainThread(() => Error?.Invoke(this, message));
}
