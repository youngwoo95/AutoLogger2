using AutoLoggerV2.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace AutoLoggerV2.ViewModels
{
    public class LockViewModel : ViewModelBase
    {
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

        private ViewModelBase currentviewmodel;
        public ViewModelBase CurrentViewModel
        {
            get
            {
                return currentviewmodel;
            }
            set
            {
                if(currentviewmodel != value)
                {
                    currentviewmodel = value;
                    OnPropertyChanged(nameof(CurrentViewModel));
                }
            }
        }

        /// <summary>
        /// 확인 버튼
        /// </summary>
        public ICommand SubmitCommand { get; }

        public ICommand CancelCommand { get; }
 
        public LockViewModel(IServiceProvider serviceProvider)
        {
            SubmitCommand = new RelayCommand<object>(_ => Commmit());
            CancelCommand = new RelayCommand<object>(_ =>
            {
                CurrentViewModel = serviceProvider.GetRequiredService<HomeViewModel>();
            });
        }

        private void Commmit()
        {
            Console.WriteLine("얘가 실행되냐?");
            Console.WriteLine(LoginPassword);
        }
    }
}
