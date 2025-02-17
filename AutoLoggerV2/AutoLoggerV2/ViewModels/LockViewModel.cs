using AutoLoggerV2.Commands;
using AutoLoggerV2.Services;
using AutoLoggerV2.Services.Common;
using System.Windows;
using System.Windows.Input;

namespace AutoLoggerV2.ViewModels
{
    public class LockViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        private string loginPassword;
        public string LoginPassword
        {
            get => loginPassword;
            set
            {
                if (loginPassword != value)
                {
                    loginPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        
        public LockViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;


            SubmitCommand = new RelayCommand<object>(_ => Commit());
            CancelCommand = new RelayCommand<object>(_ =>
            {
                // 전역 NavigationService를 통해 HomeViewModel로 전환
                _navigationService.NavigateTo<HomeViewModel>();
            });
        }

        /// <summary>
        /// 확인버튼
        /// </summary>
        private void Commit()
        {
            if (LoginPassword.ToLower().Equals(CommDATA.LockPassword))
            {
                _navigationService.NavigateTo<SettingViewModel>();
            }
            else
            {
                MessageBox.Show("비밀번호가 올바르지 않습니다");
            }
        }
    }

}
