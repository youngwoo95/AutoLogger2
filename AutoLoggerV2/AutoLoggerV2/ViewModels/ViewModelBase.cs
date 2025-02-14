using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoLoggerV2.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isHomeSelected;
        public bool IsHomeSelected
        {
            get => _isHomeSelected;
            set
            {
                if (_isHomeSelected != value)
                {
                    _isHomeSelected = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        IsLockSelected = false;
                    }
                }
            }
        }

        private bool _isLockSelected;
        public bool IsLockSelected
        {
            get => _isLockSelected;
            set
            {
                if (_isLockSelected != value)
                {
                    _isLockSelected = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        _isHomeSelected = false;
                    }
                }

            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 프로퍼티 변경 시 이벤트를 발생시키는 메서드
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}