using AutoLoggerV2.Commands;
using AutoLoggerV2.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Windows.Navigation;

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

        private void Commit()
        {
            Console.WriteLine("Commit 실행");
            Console.WriteLine(LoginPassword);
        }
    }

}
