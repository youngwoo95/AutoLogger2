using AutoLoggerV2.Commands;
using AutoLoggerV2.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;

namespace AutoLoggerV2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// HOME 화면 호출 버튼
        /// </summary>
        public ICommand ShowHomeCommand { get; }

        /// <summary>
        /// 잠금화면 호출 버튼
        /// </summary>
        public ICommand ShowLockCommand { get; }

        private ViewModelBase currentviewmodel;
        public ViewModelBase CurrentViewModel
        {
            get
            {
                return currentviewmodel;
            }
            set
            {
                if (currentviewmodel != value)
                {
                    currentviewmodel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }
        public INavigationService NavigationService { get; }
        public MainViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            // 예시: ShowHomeCommand도 추가 가능
            ShowHomeCommand = new RelayCommand<object>(_ =>
            {
                NavigationService.NavigateTo<HomeViewModel>();
            });

            ShowLockCommand = new RelayCommand<object>(_ =>
            {
                NavigationService.NavigateTo<LockViewModel>();
            });
        }

    }
}
