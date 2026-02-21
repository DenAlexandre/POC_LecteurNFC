using NfcSecurePoc.ViewModels;

namespace NfcSecurePoc.Views;

public partial class ReaderPage : ContentPage
{
    public ReaderPage(ReaderViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
