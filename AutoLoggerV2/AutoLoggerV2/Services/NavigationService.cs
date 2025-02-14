using AutoLoggerV2.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoLoggerV2.Services
{
    public class NavigationService : INavigationService, INotifyPropertyChanged
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
                    //OnPropertyChanged(nameof(CurrentViewModel));
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler CurrentViewModelChanged;


        private readonly IServiceProvider _serviceProvider;
        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            // 앱 시작 시 기본 뷰를 HomeViewModel로 설정
            CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
