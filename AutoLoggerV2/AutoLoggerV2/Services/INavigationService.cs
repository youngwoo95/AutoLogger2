using AutoLoggerV2.ViewModels;
using System.ComponentModel;

namespace AutoLoggerV2.Services
{
    public interface INavigationService : INotifyPropertyChanged
    {
        ViewModelBase CurrentViewModel { get; set; }
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    }
}
