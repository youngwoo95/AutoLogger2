using AutoLoggerV2.Commands;
using Microsoft.Extensions.DependencyInjection;
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

        public MainViewModel(IServiceProvider serviceProvider)
        {
            CurrentViewModel = serviceProvider.GetRequiredService<HomeViewModel>();

            ShowHomeCommand = new RelayCommand<object>(_ =>
            {
                CurrentViewModel = serviceProvider.GetRequiredService<HomeViewModel>();
            });

            ShowLockCommand = new RelayCommand<object>(_ =>
            {
                CurrentViewModel = serviceProvider.GetRequiredService<LockViewModel>();
            });
        }

    }
}
