using NfcSecurePoc.ViewModels;

namespace NfcSecurePoc.Views;

public partial class HcePage : ContentPage
{
    public HcePage(HceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
