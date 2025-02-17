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

        #region 세팅파일 정보
        private string? _folderpath;
        /// <summary>
        /// 폴더 경로
        /// </summary>
        public string? FolderPath
        {
            get => _folderpath;
            set
            {
                if (_folderpath != value)
                {
                    _folderpath = value;
                    OnPropertyChanged(nameof(FolderPath));
                }
            }
        }

        private string? _dbip;
        /// <summary>
        /// 데이터베이스 IP
        /// </summary>
        public string? DBIP
        {
            get => _dbip;
            set
            {
                if (_dbip != value)
                {
                    _dbip = value;
                    OnPropertyChanged(nameof(DBIP));
                }
            }
        }

        private string? _dbport;
        /// <summary>
        /// 데이터베이스 PORT
        /// </summary>
        public string? DBPORT
        {
            get => _dbport;
            set
            {
                if (_dbport != value)
                {
                    _dbport = value;
                    OnPropertyChanged(nameof(DBPORT));
                }
            }
        }


        private string? _dbid;
        /// <summary>
        /// 데이터베이스 ID
        /// </summary>
        public string? DBID
        {
            get => _dbid;
            set
            {
                if (_dbid != value)
                {
                    _dbid = value;
                    OnPropertyChanged(nameof(DBID));
                }
            }
        }

        private string? _dbpassword;
        /// <summary>
        /// 데이터베이스 PW
        /// </summary>
        public string? DBPassword
        {
            get => _dbpassword;
            set
            {
                if (_dbpassword != value)
                {
                    _dbpassword = value;
                    OnPropertyChanged(nameof(DBPassword));
                }
            }
        }

        private string? _dbname;
        /// <summary>
        /// 데이터베이스 테이블명
        /// </summary>
        public string? DBNAME
        {
            get => _dbname;
            set
            {
                if (_dbname != value)
                {
                    _dbname = value;
                    OnPropertyChanged(nameof(DBNAME));
                }
            }
        }
        #endregion

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