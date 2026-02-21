using System.Windows.Input;
using NfcSecurePoc.Services;

namespace NfcSecurePoc.ViewModels;

public class ReaderViewModel : BaseViewModel
{
    private readonly INfcReaderService _readerService;
    private bool _isScanning;
    private string _statusText = "Idle";
    private string _tagId = "-";
    private string _decryptedData = "-";

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            SetProperty(ref _isScanning, value);
            StatusText = value ? "Scanning..." : "Idle";
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string TagId
    {
        get => _tagId;
        private set => SetProperty(ref _tagId, value);
    }

    public string DecryptedData
    {
        get => _decryptedData;
        private set => SetProperty(ref _decryptedData, value);
    }

    public bool IsSupported => _readerService.IsSupported;

    public ICommand ToggleScanCommand { get; }

    public ReaderViewModel(INfcReaderService readerService)
    {
        _readerService = readerService;

        _readerService.TagDiscovered += OnTagDiscovered;
        _readerService.DataReceived += OnDataReceived;
        _readerService.Error += OnError;

        ToggleScanCommand = new Command(ToggleScan);
    }

    private void ToggleScan()
    {
        if (!_readerService.IsSupported)
        {
            AppendLog("NFC reader not supported on this device");
            return;
        }

        if (IsScanning)
        {
            _readerService.StopScanning();
            IsScanning = false;
            AppendLog("Reader stopped");
        }
        else
        {
            TagId = "-";
            DecryptedData = "-";
            _readerService.StartScanning();
            IsScanning = true;
            AppendLog("Reader started — waiting for tag...");
        }
    }

    private void OnTagDiscovered(object? sender, TagDiscoveredEventArgs e)
    {
        TagId = e.TagId;
        AppendLog($"Tag discovered: {e.TagId} ({e.TagType})");
    }

    private void OnDataReceived(object? sender, DataReceivedEventArgs e)
    {
        DecryptedData = e.DecryptedText ?? "(unable to decrypt)";
        AppendLog($"Data received: \"{e.DecryptedText}\" ({e.RawData.Length} bytes encrypted)");
    }

    private void OnError(object? sender, string message)
    {
        AppendLog(message);
    }
}
