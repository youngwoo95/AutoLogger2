using AutoLoggerV2.Commands;
using AutoLoggerV2.Services.Common;
using AutoLoggerV2.Services.Logger;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
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

        /// <summary>
        /// 연결 확인버튼
        /// </summary>
        public ICommand ConnCheckCommand { get; }
        #endregion

        /// <summary>
        /// 상수파일 주입
        /// </summary>
        private readonly IAppSettings AppSettings;
        
        /// <summary>
        /// 로그 의존성
        /// </summary>
        private readonly ILoggers Loggers;

        public SettingViewModel(IAppSettings _appSettings, ILoggers _loggers)
        {
            this.AppSettings = _appSettings;
            this.Loggers = _loggers;

            FileLoad();

            // 초기화는 숨김
            _showPassword = false;
            ToggleShowPasswordCommand = new RelayCommand<object>(_ => 
            {
                ShowPassword = !ShowPassword; 
                
            });
            SearchCommand = new RelayCommand<object>(_ => FilePathSearch());
            SaveCommand = new RelayCommand<object>(_ => FileSave()); // 설정파일 저장
            ConnCheckCommand = new RelayCommand<object>(async _ => await ConnCheck()); // 연결확인

            Debug.WriteLine(typeof(SettingViewModel));
        }

        private void FileLoad()
        {
            if (File.Exists(AppSettings.SETTINGPATH))
            {
                using (StreamReader reader = File.OpenText(AppSettings.SETTINGPATH))
                {
                    string str = reader.ReadToEnd();

                    var jobj = JObject.Parse(str);
                    FolderPath = (string)jobj["PATH"];
                    DBIP = (string)jobj["DBIP"];
                    DBPORT = (string)jobj["DBPORT"];
                    DBID = (string)jobj["DBID"];
                    DBPassword = (string)jobj["DBPW"];
                    DBNAME = (string)jobj["DBNAME"];
                }
            }
        }

        /// <summary>
        /// 파일경로 Search
        /// </summary>
        private void FilePathSearch()
        {
            var di = new OpenFolderDialog();
            if (di.ShowDialog() is true)
            {
                FolderPath = di.FolderName;
#if DEBUG
                Console.WriteLine(FolderPath);
#endif
            }
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

            CommDATA.OK_MESSAGE("저장이 완료되었습니다");
            
            Console.WriteLine($"세팅파일 경로:{AppSettings.SETTINGPATH}");
            Console.WriteLine($"저장값 : {jobj.ToString()}");
        }

        /// <summary>
        /// 연결확인
        /// </summary>
        private async Task ConnCheck()
        {
            string connStr = $"Data Source=(DESCRIPTION=(ADDRESS = (PROTOCOL = TCP)(HOST = {DBIP})(PORT = {DBPORT})) (CONNECT_DATA=(SERVER = DBDICATED) (SERVICE_NAME = {DBNAME})));User Id={DBID};Password={DBPassword}";

            using (OracleConnection conn = new OracleConnection(connStr))
            {
                try
                {
                    await conn.OpenAsync();
                    await Loggers.INFOMEssage($"데이터베이스 연결성공 {connStr}");
                }
                catch(Exception ex)
                {
                    await Loggers.ERRORMessage($"데이터베이스 연결실패 {ex.ToString()}");
                }
            }
        }
    }
}
