using Microsoft.Extensions.Logging;
using NfcSecurePoc.Crypto;
using NfcSecurePoc.Services;
using NfcSecurePoc.ViewModels;
using NfcSecurePoc.Views;

namespace NfcSecurePoc;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Crypto
        builder.Services.AddSingleton<IAesCryptoService, AesCryptoService>();

        // Platform-specific NFC services
#if ANDROID
        builder.Services.AddSingleton<INfcHceService, Platforms.Android.Services.AndroidHceServiceWrapper>();
        builder.Services.AddSingleton<INfcReaderService, Platforms.Android.Services.AndroidNfcReaderService>();
#elif IOS
        builder.Services.AddSingleton<INfcHceService, Platforms.iOS.Services.IosHceService>();
        builder.Services.AddSingleton<INfcReaderService, Platforms.iOS.Services.IosNfcReaderService>();
#endif

        // ViewModels
        builder.Services.AddTransient<HceViewModel>();
        builder.Services.AddTransient<ReaderViewModel>();

        // Pages
        builder.Services.AddTransient<HcePage>();
        builder.Services.AddTransient<ReaderPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
