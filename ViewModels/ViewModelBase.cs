using AvaloniaAppCrypto.Localization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>Localizer bound by every view.</summary>
        public Localizer Loc => Localizer.Instance;
    }
}
