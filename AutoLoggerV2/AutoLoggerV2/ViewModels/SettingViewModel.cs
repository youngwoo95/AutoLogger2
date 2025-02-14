using AutoLoggerV2.Commands;
using AutoLoggerV2.Services.Common;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace AutoLoggerV2.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 숨김 - 보임 트리거
        /// </summary>
        private bool _showPassword;
        public bool ShowPassword
        {
            get => _showPassword;
            set
            {
                if(_showPassword != value)
                {
                    _showPassword = value;
                    OnPropertyChanged(nameof(ShowPassword));
                }
            }
        }

        #region 세팅파일 정보
        private string _folderpath;
        /// <summary>
        /// 폴더 경로
        /// </summary>
        public string FolderPath
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

        private string _dbip;
        /// <summary>
        /// 데이터베이스 IP
        /// </summary>
        public string DBIP
        {
            get => _dbip;
            set
            {
                if(_dbip != value)
                {
                    _dbip = value;
                    OnPropertyChanged(nameof(DBIP));
                }
            }
        }

        private string _dbport;
        /// <summary>
        /// 데이터베이스 PORT
        /// </summary>
        public string DBPORT
        {
            get => _dbport;
            set
            {
                if(_dbport != value)
                {
                    _dbport = value;
                    OnPropertyChanged(nameof(DBPORT));
                }
            }
        }

        
        private string _dbid;
        /// <summary>
        /// 데이터베이스 ID
        /// </summary>
        public string DBID
        {
            get => _dbid;
            set
            {
                if(_dbid != value)
                {
                    _dbid = value;
                    OnPropertyChanged(nameof(DBID));
                }
            }
        }

        private string _dbpassword;
        /// <summary>
        /// 데이터베이스 PW
        /// </summary>
        public string DBPassword
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

        private string _dbname;
        /// <summary>
        /// 데이터베이스 테이블명
        /// </summary>
        public string DBNAME
        {
            get => _dbname;
            set
            {
                if(_dbname != value)
                {
                    _dbname = value;
                    OnPropertyChanged(nameof(DBNAME));
                }
            }
        }


        #endregion

        #region 버튼
        /// <summary>
        /// 검색버튼
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// 비밀번호 숨김-보임 버튼
        /// </summary>
        public ICommand ToggleShowPasswordCommand { get; }

        /// <summary>
        /// 저장버튼
        /// </summary>
        public ICommand SaveCommand { get; }
        #endregion

        /// <summary>
        /// 상수파일 주입
        /// </summary>
        private readonly IAppSettings AppSettings;

        public SettingViewModel(IAppSettings _appSettings)
        {
            this.AppSettings = _appSettings;

            // 초기화는 숨김
            _showPassword = false;
            ToggleShowPasswordCommand = new RelayCommand<object>(_ => 
            {
                ShowPassword = !ShowPassword; 
                
            });
            SearchCommand = new RelayCommand<object>(_ => FilePathSearch());
            SaveCommand = new RelayCommand<object>(_ => FileSave()); // 설정파일 저장

            Console.WriteLine("세팅뷰");
        }

        /// <summary>
        /// 설정파일 저장
        /// </summary>
        private void FileSave()
        {
            var jobj = new JObject();
            jobj.Add("PATH", FolderPath);
            jobj.Add("DBIP", DBIP);
            jobj.Add("DBPORT", DBPORT);
            jobj.Add("DBID", DBID);
            jobj.Add("DBPW", DBPassword);
            jobj.Add("DBNAME", DBNAME);

            if(!File.Exists(AppSettings.SETTINGPATH))
            {
                using (File.Create(AppSettings.SETTINGPATH)) { }
            }
            File.WriteAllText(AppSettings.SETTINGPATH, jobj.ToString());

            Console.WriteLine($"세팅파일 경로:{AppSettings.SETTINGPATH}");
            Console.WriteLine($"저장값 : {jobj.ToString()}");
        }

        // 파일경로 Search
        private void FilePathSearch()
        {
            var di = new OpenFolderDialog();
            if (di.ShowDialog() is true)
            {
                FolderPath = di.FolderName;
                Console.WriteLine(FolderPath);
            }
        }

    
    }
}
