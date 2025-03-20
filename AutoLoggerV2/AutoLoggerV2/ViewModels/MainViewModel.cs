using AutoLoggerV2.Commands;
using AutoLoggerV2.Services;
using AutoLoggerV2.Services.Logger;
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

        private readonly ILoggers Loggers;
        public INavigationService NavigationService { get; }
        public MainViewModel(INavigationService navigationService, ILoggers _loggers)
        {
            try
            {
                this.Loggers = _loggers;

                NavigationService = navigationService;
                NavigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

                // 예시: ShowHomeCommand도 추가 가능
                ShowHomeCommand = new RelayCommand<object>(_ =>
                {
                    NavigationService.NavigateTo<HomeViewModel>();
                });

                ShowLockCommand = new RelayCommand<object>(_ =>
                {
                    NavigationService.NavigateTo<LockViewModel>();
                });

                IsHomeSelected = true;
            }catch(Exception ex)
            {
                Loggers.ERRORMessage(ex.ToString());
            }
        }


        private void OnCurrentViewModelChanged(object sender, EventArgs e)
        {
            try
            {
                // 화면 전환에 따라 선택 상태를 업데이트
                if (NavigationService.CurrentViewModel is HomeViewModel)
                {
                    IsHomeSelected = true;
                    IsLockSelected = false;
                }
                else if (NavigationService.CurrentViewModel is LockViewModel)
                {
                    IsHomeSelected = false;
                    IsLockSelected = true;
                }

                // ContentControl 바인딩 갱신
                OnPropertyChanged(nameof(CurrentViewModel));
            }catch(Exception ex)
            {
                Loggers.ERRORMessage(ex.ToString());
            }
        }

    }
}
