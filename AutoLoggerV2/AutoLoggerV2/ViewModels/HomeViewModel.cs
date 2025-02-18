using AutoLoggerV2.Commands;
using AutoLoggerV2.Models;
using AutoLoggerV2.Services.Common;
using AutoLoggerV2.Services.Logger;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace AutoLoggerV2.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        /// <summary>
        /// 시작일
        /// </summary>
        private DateTime startdate;
        public DateTime StartDate
        {
            get
            {
                return startdate;
            }
            set
            {
                if (startdate != value)
                {
                    startdate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        private ObservableCollection<string> _querylist = new ObservableCollection<string>();
        public ObservableCollection<string> QueryList
        {
            get => _querylist;
            set
            {
                if(_querylist != value)
                {
                    _querylist = value;
                    OnPropertyChanged(nameof(QueryList));
                }
            }
        }

        /// <summary>
        /// 종료일
        /// </summary>
        private DateTime enddate;
        public DateTime EndDate
        {
            get
            {
                return enddate;
            }
            set
            {
                if(enddate != value)
                {
                    enddate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

        /// <summary>
        /// 출입 커맨드
        /// </summary>
        public ICommand EnterCommand { get; }
        /// <summary>
        /// 근태 커맨드
        /// </summary>
        public ICommand AttendanceCommand { get; }

        /// <summary>
        /// 식수 커맨드
        /// </summary>
        public ICommand RestaurantCommand { get; }

        /// <summary>
        /// 방범 커맨드
        /// </summary>
        public ICommand SecurityCommand { get; }

        /// <summary>
        /// 조회 커맨드
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// 상수 파일 주입
        /// </summary>
        private readonly IAppSettings AppSettings;
        
        /// <summary>
        /// 출입 & 근태 & 식수 & 방범 메뉴선택
        /// </summary>
        private int MenuType { get; set; }

        /// <summary>
        /// 로그 의존성 주입
        /// </summary>
        private readonly ILoggers Loggers;
        public HomeViewModel(IAppSettings _appsettings,
            ILoggers _loggers)
        {
            this.AppSettings = _appsettings;
            this.Loggers = _loggers;
            FileLoad();  // Setting File Load
            
            
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // 시작일 default
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // 종료일 default

            EnterCommand = new RelayCommand<object>(_ => MenuType = 1); // 출입
            AttendanceCommand = new RelayCommand<object>(_ => MenuType = 2); // 근태
            RestaurantCommand = new RelayCommand<object>(_ => MenuType = 3); // 식수
            SecurityCommand = new RelayCommand<object>(_ => MenuType = 4); // 방범

            SearchCommand = new RelayCommand<object>(async _ => await GetLogFileSearch());
            
            Console.WriteLine(typeof(HomeViewModel));
        }

        /// <summary>
        /// Setting File Load
        /// </summary>
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


        private async Task GetLogFileSearch()
        {

            string connStr = $"Data Source=(DESCRIPTION=(ADDRESS = (PROTOCOL = TCP)(HOST = {DBIP})(PORT = {DBPORT})) (CONNECT_DATA=(SERVER = DBDICATED) (SERVICE_NAME = {DBNAME})));User Id={DBID};Password={DBPassword};Pooling=true;Min Pool Size=5;Connection Lifetime=120;Validate Connection=true;";
            IsBusy = true;
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                try
                {
                    await conn.OpenAsync();
                    Console.WriteLine("데이터베이스 연결성공");

                    /* 연결실패 - Catch() */

                    // 1. 식수 리스트를 구한다.
                    // 2. Log를 Converter 한다.
                    // 3. 식수 list와 Convert된 로그를 비교해서 화면에 보여준다.
                    // 4. 저장버튼을 누르면 ISNERT 된다.
                    // 5. 중복제거 버튼을 누르면 update 된다.

                    // 1. 식수 리스트를 구한다. - 식수가 판단 기준이됨.
                    var SiksuList = new List<string>();
                    string GetSiksuQuery = "SELECT MC_ID||SC_ID as MC_ID FROM INFO_SC WHERE USED_SIKSU = 1 GROUP BY MC_ID,SC_ID ORDER BY MC_ID, SC_ID";
                    using (OracleCommand comm = new OracleCommand(GetSiksuQuery, conn))
                    {
                        using (var reader = await comm.ExecuteReaderAsync())
                        {
                            while(await reader.ReadAsync())
                            {
                                SiksuList.Add(reader.GetString(0));
#if DEBUG
                                Console.WriteLine(reader.GetString(0));
#endif
                            }
                            Console.WriteLine($"식수 리스트 수 {SiksuList.Count}");
                        }
                    }

                    // 2. LogConverter
                    var TargetFiles = new List<string>();
                    if(Directory.Exists(FolderPath))
                    {
                        var di = new DirectoryInfo(FolderPath); // 해당 경로의 디렉터리
                        var dir = di.GetDirectories().ToList(); // 디렉터리를 가져온다.

                        // 파일의 명칭이 LM 인것.
                        var LMCheck = dir.Where(m => m.Name.Substring(0, 2) == "LM").ToList();

                        // 파일의 길이가 15 이상인것.
                        var LengthCheck = LMCheck.Where(m => m.Name.Length > 15);

                        // 조회조건 범위안의 날짜 인것.
                        var DateCheck = LengthCheck
                            .Where(item => {
                                // item.Name의 3번째 인덱스부터 10개의 문자를 추출
                                string str = item.Name.Substring(3, 10);
                                // 날짜 문자열 파싱 (파싱 실패 시 false를 리턴하도록 TryParse 사용 가능)
                                DateTime dt = DateTime.Parse(str);
                                // starttime과 endtime 범위 내에 있는지 확인
                                return StartDate <= dt && dt <= EndDate;
                            })
                            .Select(item => item.FullName)
                            .ToList();

                        // 폴더명 규칙이 정규식에 맞는지 검사.
                        Regex regex = new Regex(@"^[A-Z]{2}\sLog\s[0-9]{4}-[0-9]{2}-[0-9]{2}.*");
                        TargetFiles = DateCheck.SelectMany(folder => new DirectoryInfo(folder).GetFiles())
                                        .Where(file => regex.IsMatch(file.Name))
                                        .Select(file => file.FullName)
                                        .ToList();
                    }

                    var pattern = @"\u0002([^>]*)\u0003";

                    var LogConverterData = TargetFiles
                        .SelectMany(file =>
                        {
                            // 파일의 전체 내용을 읽고 정규식 매치를 가져옵니다.
                            string content = File.ReadAllText(file);
                            return Regex.Matches(content, pattern).Cast<Match>();
                        })
                        .Select(m =>
                        {
                            // 제어문자(\u0002, \u0003)를 제거한 실제 코드 문자열
                            string code = m.Value.Substring(1, m.Length - 2);
                            return code;
                        })
                        .Where(code => code.Substring(25, 1) == "c" || code.Substring(25, 1) == "A")
                        .Select(code =>
                        {
                            // DATA를 먼저 추출합니다.
                            var data = code.Substring(27, code.Length - 29); // (code.Length - 2) - 27 == code.Length - 29
                            return new DataFormatModel
                            {
                                LEN = code.Substring(0, 3),       // 0,1,2 인덱스 --> LEN
                                RAN = code.Substring(3, 1),         // 3 인덱스 --> RAN
                                M_CODE = code.Substring(4, 2),      // 4,5 인덱스 --> M_CODE
                                LCC = code.Substring(6, 8),         // 6~13 인덱스 --> LCC
                                MC = code.Substring(14, 11),        // 14~24 인덱스 --> MC 또는 SC
                                SC = code.Substring(23, 2),         // 23,24 인덱스 --> SC
                                COMMAND = code.Substring(25, 1),    // 25 인덱스 --> Command
                                MSG = code.Substring(26, 1),        // 26 인덱스 --> MSG
                                DATA = data,                        // 추출한 DATA
                                DATATYPE = data.Substring(0, 1),    // DATA의 0번 인덱스 --> dataType
                                CS = code.Substring(code.Length - 2) // 마지막 2글자 --> 체크섬
                            };
                        })
                        .ToList();
                    
                    // 3.식수 list와 Convert된 로그를 비교해서 화면에 보여준다.

                    switch (MenuType)
                    {
                        // 출입
                        case 1:
                            var result = LogConverterData.Where(m => !SiksuList.Contains(m.MC) && m.DATATYPE == "E" && m.COMMAND == "c" && Convert.ToChar(m.DATA.Substring(22, 1)) > 0x38 && m.DATA.Substring(25, 1) != "*").ToList();

                            var AccessList = result.Select(m => new ECodeFormatModel
                            {
                                LEN = m.LEN,
                                RAN = m.RAN,
                                M_CODE = m.M_CODE,
                                LCC = m.LCC,
                                MC = m.MC.Substring(0, 9), // 아까붙힌 MC || SC 에서 SC를 떼야함.
                                SC = m.SC,
                                COMMAND = m.COMMAND,
                                MSG = m.MSG,
                                DATATYPE = m.DATATYPE,
                                DATA = m.DATA,
                                CS = m.CS,

                                // ================ CardFormat ======================= //
                                Code = m.DATA.Substring(0, 1),
                                Spare = m.DATA.Substring(1, 1),
                                DoorNumber = m.DATA.Substring(2, 1),
                                CardReaderNumber = m.DATA.Substring(3, 1),
                                CardReaderPosition = m.DATA.Substring(4, 1),
                                YYMMDD = m.DATA.Substring(5, 6),
                                HHMMSS = m.DATA.Substring(11, 6),
                                Posi = m.DATA.Substring(17, 1),
                                Mode = m.DATA.Substring(18, 1),
                                Reason = m.DATA.Substring(19, 1),
                                Result = m.DATA.Substring(20, 1),
                                DoorState = m.DATA.Substring(21, 1),
                                ButtonState = m.DATA.Substring(22, 1),
                                CardLen = m.DATA.Substring(23, 2),
                                CardData = m.DATA.Substring(25, (m.DATA.Length - 5) - 25),
                                CardID = m.DATA.Substring(m.DATA.Length - 5)
                            });

                            foreach(var item in AccessList)
                            {
                                string query = $"INSERT INTO INOUT_EVENT(MC_ID, SC_ID, DOOR_ID, EVT_TIME, CR_NO, CR_LOCATION, DOOR_MODE, CARD_AUTH, DOOR_STATUS, BUTTON_STATUS, CARD_LENGTH, CARD_NO, CARD_ID, HUMAN_ID, EVT_REASON, COMMAND_CODE, EVT_CODE, MSG_ID) " +
                                           $"VALUES('{item.MC}'," + // MC_ID
                                           $"'{item.SC}'," + // SC_ID
                                           $"'{item.DoorNumber}'," + // DOOR_ID
                                           $"timestamp'20{item.YYMMDD.Substring(0, 2)}-{item.YYMMDD.Substring(2, 2)}-{item.YYMMDD.Substring(4, 2)} {item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}'," + // EVT_TIME
                                           $"'0{item.CardReaderNumber}', " + // CR_NO
                                           $"'{item.CardReaderPosition}', " + // CR_LOCATION ====================== 1
                                           $"'{item.Mode}'," + // DOOR_MODE ================= 0
                                           $"'0', " + // CARD_AUTO ========================== 0
                                           $"'{item.DoorState}', " + // DOOR_STATUS
                                           $"'{item.ButtonState}'," + // BUTTON_STATUS
                                           $"{item.CardLen}, " + // CARD_LENGTH  ================09
                                           $"'{item.CardData}', " + // CARD_NO
                                           $"'{item.CardID}', " + // CARD_ID
                                           $"FN_GET_HUMANID('{item.CardData}'), " + // HUMAN_ID
                                           $"'{item.Reason}', " + // EVT_REASON
                                           $"'{item.Code}', " + // COMMAND_CODE ======================== E
                                           $"'{item.Result}', " + // EVT_CODE
                                           $"'{item.CS}')"; // MSG_ID

                                QueryList.Add(query);

                            }

                            break;
                        // 근태
                        case 2:
                            break;
                        // 식수
                        case 3:
                            break;
                        // 방범
                        case 4:
                            break;
                    }

                 
                    Console.WriteLine("");

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    await Loggers.ERRORMessage($"데이터베이스 연결실패: {ex}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

    }
}
