using AutoLoggerV2.Commands;
using AutoLoggerV2.Models;
using AutoLoggerV2.Services.Common;
using AutoLoggerV2.Services.Logger;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
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

        // 원본 데이터 컬렉션
        private ObservableCollection<string> _originalCollection = new ObservableCollection<string>();
        public ObservableCollection<string> OriginalCollection
        {
            get => _originalCollection;
            set
            {
                if (_originalCollection != value)
                {
                    _originalCollection = value;
                    OnPropertyChanged(nameof(OriginalCollection));
                }
            }
        }

        private ICollectionView _querylist;
        /// <summary>
        /// 필터링된 결과를 제공할 ICollectionView (QueryList로 바인딩)
        /// </summary>
        public ICollectionView QueryList 
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

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    // TextBox 내용이 변경되면 필터를 갱신한다.
                    QueryList.Refresh();
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
        /// 출입 LIST
        /// </summary>
        private List<string> AccessResult;

        /// <summary>
        /// 근태 LIST
        /// </summary>
        private List<string> AttendanceResult;

        /// <summary>
        /// 식수 LIST
        /// </summary>
        private List<string> SiksuResult;

        /// <summary>
        /// 방범 LIST
        /// </summary>
        private List<string> PreventionResult;

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
        /// 중복제거 커맨드
        /// </summary>
        public ICommand DupleCommand { get; }

        /// <summary>
        /// 저장 커맨드
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// 상수 파일 주입
        /// </summary>
        private readonly IAppSettings AppSettings;
        
        /// <summary>
        /// 출입 & 근태 & 식수 & 방범 메뉴선택
        /// </summary>
        private int MenuType { get; set; }

        /// <summary>
        /// 데이터베이스 연결문자열
        /// </summary>
        private string connStr { get; set; }

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

            connStr = $"Data Source=(DESCRIPTION=(ADDRESS = (PROTOCOL = TCP)(HOST = {DBIP})(PORT = {DBPORT})) (CONNECT_DATA=(SERVER = DBDICATED) (SERVICE_NAME = {DBNAME})));User Id={DBID};Password={DBPassword};Pooling=true;Min Pool Size=5;Connection Lifetime=120;Validate Connection=true;";


            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // 시작일 default
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); // 종료일 default

            EnterCommand = new RelayCommand<object>(_ => MenuType = 1); // 출입
            AttendanceCommand = new RelayCommand<object>(_ => MenuType = 2); // 근태
            RestaurantCommand = new RelayCommand<object>(_ => MenuType = 3); // 식수
            SecurityCommand = new RelayCommand<object>(_ => MenuType = 4); // 방범

            SearchCommand = new RelayCommand<object>(async _ => await GetLogFileSearch());

            DupleCommand = new RelayCommand<object>(_ => RemoveDuple()); // 중복제거
            SaveCommand = new RelayCommand<object>(async _ => await Save()); // 저장

            Console.WriteLine(typeof(HomeViewModel));
        }

        /// <summary>
        /// Setting File Load
        /// </summary>
        private void FileLoad()
        {
            try
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
            }catch(Exception ex)
            {
                Loggers.ERRORMessage(ex.ToString());
            }
        }

        private async Task Save()
        {
            if (MenuType == 0)
            {
                CommDATA.ERROR_MESSAGE("메뉴를 선택해주세요");
                return;
            }
           
            switch (MenuType)
            {
                // 출입
                case 1:
                    try
                    {
                        IsBusy = true;
                        int Success = 0;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();

                            int batchSize = 1000;
                            int totalCount = AccessResult.Count;
                            
                            for (int i = 0; i < totalCount; i += batchSize)
                            {
                                using (var transaction = conn.BeginTransaction())
                                {
                                    Console.WriteLine($"Processing batch starting at index {i}");
                                    for (int j = i; j < i + batchSize && j < totalCount; j++)
                                    {                             
                                        string query = AccessResult[j];
                                        using (OracleCommand comm = new OracleCommand(query, conn))
                                        {
                                            comm.Transaction = transaction;
                                            try
                                            {
                                                await comm.ExecuteNonQueryAsync();
                                                Success++;
                                            }
                                            catch (OracleException ex) when (ex.Number == 1) // ORA-00001
                                            {
                                                // 중복 키 오류 발생 시 해당 쿼리 건너뜀
                                                Console.WriteLine($"중복키를 건너뜁니다.\n{query}");
                                            }
                                        }
                                    }
                                    transaction.Commit();
                                    Console.WriteLine($"Batch ending at index {Math.Min(i + batchSize, totalCount) - 1} committed.");
                                }
                            }

                            var result = CommDATA.YESNO_MESSAGE($"{Success}건 성공하셨습니다\n중복데이터 삭제하시겠습니까?");
                            if (result == MessageBoxResult.Yes)
                            {
                                // 바로 중복 데이터 삭제
                                using (var transaction = conn.BeginTransaction())
                                {
                                    string query = $"DELETE FROM INOUT_EVENT A WHERE A.ROWID > (" +
                                            $"SELECT MIN(B.ROWID) FROM INOUT_EVENT B WHERE A.MC_ID = B.MC_ID " +
                                            $"AND A.SC_ID = B.SC_ID " +
                                            $"AND A.DOOR_ID = B.DOOR_ID " +
                                            $"AND A.EVT_TIME = B.EVT_TIME " +
                                            $"AND A.CARD_NO = B.CARD_NO " +
                                            $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                            $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                            $"AND B.HUMAN_ID != '-1')";

                                    using (OracleCommand comm = new OracleCommand(query, conn))
                                    {
                                        comm.Transaction = transaction;
                                        try
                                        {
                                            await comm.ExecuteNonQueryAsync();
                                            transaction.Commit();
                                            CommDATA.OK_MESSAGE("중복데이터 삭제성공");
                                        }
                                        catch (OracleException ex)
                                        {
                                            await Loggers.ERRORMessage(ex.ToString());
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;

                // 근태
                case 2:
                    try
                    {
                        IsBusy = true;
                        int Success = 0;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();

                            int batchSize = 100;
                            int totalCount = AttendanceResult.Count;

                            for (int i = 0; i < totalCount; i += batchSize)
                            {
                                using (var transaction = conn.BeginTransaction())
                                {
                                    Console.WriteLine($"Processing batch starting at index {i}");
                                    for (int j = i; j < 1 + batchSize && j < totalCount; j++)
                                    {
                                        string query = AttendanceResult[j];
                                        using (OracleCommand comm = new OracleCommand(query, conn))
                                        {
                                            comm.Transaction = transaction;
                                            try
                                            {
                                                await comm.ExecuteNonQueryAsync();
                                                Success++;
                                            }
                                            catch(OracleException ex) when (ex.Number == 1) // ORA-00001
                                            {
                                                // 중복 키 오류 발생 시 해당 쿼리 건너뜀
                                                Console.WriteLine($"중복키를 건너뜁니다.\n{query}");
                                            }
                                        }
                                    }
                                    transaction.Commit();
                                    Console.WriteLine($"Batch ending at index {Math.Min(i + batchSize, totalCount) - 1} committed.");
                                }
                            }

                            var result = CommDATA.YESNO_MESSAGE($"{Success}건 성공하셧습니다\n중복데이터 삭제하시겠습니까?");
                            if(result == MessageBoxResult.Yes)
                            {
                                // 중복데이터 삭제
                                using (var transaction = conn.BeginTransaction())
                                {
                                    string query = $"DELETE FROM GUNTAE_EVENT A WHERE A.ROWID > (" +
                                           $"SELECT MIN(B.ROWID) FROM GUNTAE_EVENT B WHERE A.MC_ID = B.MC_ID " +
                                           $"AND A.SC_ID = B.SC_ID " +
                                           $"AND A.DOOR_ID = B.DOOR_ID " +
                                           $"AND A.EVT_TIME = B.EVT_TIME " +
                                           $"AND A.CARD_NO = B.CARD_NO " +
                                           $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                           $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                           $"AND B.HUMAN_ID != '-1')";

                                    using (OracleCommand comm = new OracleCommand(query, conn))
                                    {
                                        comm.Transaction = transaction;
                                        try
                                        {
                                            await comm.ExecuteNonQueryAsync();
                                            transaction.Commit();
                                            CommDATA.OK_MESSAGE("중복데이터 삭제성공");
                                        }
                                        catch(OracleException ex)
                                        {
                                            await Loggers.ERRORMessage(ex.ToString());
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;

                // 식수
                case 3:
                    try
                    {
                        IsBusy = true;
                        int Success = 0;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();

                            int batchSize = 1000;
                            int totalCount = SiksuResult.Count;

                            for(int i =0; i< totalCount; i+= batchSize)
                            {
                                using (var transaction = conn.BeginTransaction())
                                {
                                    Console.WriteLine($"Processing batch starting at index {i}");
                                    for (int j = i; j < i + batchSize && j < totalCount; j++)
                                    {
                                        string query = SiksuResult[j];
                                        using (OracleCommand comm = new OracleCommand(query, conn))
                                        {
                                            comm.Transaction = transaction;
                                            try
                                            {
                                                await comm.ExecuteNonQueryAsync();
                                                Success++;
                                            }
                                            catch (OracleException ex) when (ex.Number == 1) // ORA-00001
                                            {
                                                // 중복 키 오류 발생 시 해당 쿼리 건너뜀
                                                Console.WriteLine($"중복키를 건너뜁니다.\n{query}");
                                            }
                                        }
                                    }
                                    transaction.Commit();
                                    Console.WriteLine($"Batch ending at index {Math.Min(i + batchSize, totalCount) - 1} committed.");
                                }
                            }

                            var result = CommDATA.YESNO_MESSAGE($"{Success}건 성공하셧습니다\n중복데이터 삭제하시겠습니까?");
                            if (result == MessageBoxResult.Yes)
                            {
                                using (var transaction = conn.BeginTransaction())
                                {
                                    string query = $"DELETE FROM SIKSU_EVENT A WHERE A.ROWID > (" +
                                                    $"SELECT MIN(B.ROWID) FROM SIKSU_EVENT B WHERE A.MC_ID = B.MC_ID " + // 여기 오류
                                                    $"AND A.SC_ID = B.SC_ID " +
                                                    $"AND A.DOOR_ID = B.DOOR_ID " +
                                                    $"AND A.EVT_TIME = B.EVT_TIME " +
                                                    $"AND A.CARD_NO = B.CARD_NO " +
                                                    $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                                    $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                                    $"AND B.HUMAN_ID !='-1')";

                                    using (OracleCommand comm = new OracleCommand(query, conn))
                                    {
                                        comm.Transaction = transaction;
                                        try
                                        {
                                            await comm.ExecuteNonQueryAsync();
                                            transaction.Commit();
                                            CommDATA.OK_MESSAGE("중복데이터 삭제성공");
                                        }
                                        catch(OracleException ex)
                                        {
                                            await Loggers.ERRORMessage(ex.ToString());
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;

                // 방범
                case 4:
                    try
                    {
                        IsBusy = true;
                        int Success = 0;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();

                            int batchSize = 1000;
                            int totalCount = PreventionResult.Count;

                            for (int i = 0; i < totalCount; i += batchSize)
                            {
                                using (var transaction = conn.BeginTransaction())
                                {
                                    Console.WriteLine($"Processing batch starting at index {i}");
                                    for (int j = i; j < i + batchSize && j < totalCount; j++)
                                    {
                                        string query = PreventionResult[j];
                                        using (OracleCommand comm = new OracleCommand(query, conn))
                                        {
                                            comm.Transaction = transaction;
                                            try
                                            {
                                                await comm.ExecuteNonQueryAsync();
                                                Success++;
                                            }
                                            catch (OracleException ex) when (ex.Number == 1) // ORA-00001
                                            {
                                                // 중복 키 오류 발생 시 해당 쿼리 건너뜀
                                                Console.WriteLine($"중복키를 건너뜁니다.\n{query}");
                                            }
                                        }
                                    }
                                    transaction.Commit();
                                    Console.WriteLine($"Batch ending at index {Math.Min(i + batchSize, totalCount) - 1} committed.");
                                }
                            }
                            CommDATA.OK_MESSAGE($"{Success}건 성공하셧습니다");
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// 중복제거 버튼
        /// </summary>
        private async void RemoveDuple()
        {
            if (MenuType == 0)
            {
                CommDATA.ERROR_MESSAGE("메뉴를 선택해주세요");
                return;
            }

            switch (MenuType)
            {
                // 출입
                case 1:
                    try
                    {
                        IsBusy = true;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();
                            using (var transaction = conn.BeginTransaction())
                            {
                                string query = $"DELETE FROM INOUT_EVENT A WHERE A.ROWID > (" +
                                   $"SELECT MIN(B.ROWID) FROM INOUT_EVENT B WHERE A.MC_ID = B.MC_ID " +
                                   $"AND A.SC_ID = B.SC_ID " +
                                   $"AND A.DOOR_ID = B.DOOR_ID " +
                                   $"AND A.EVT_TIME = B.EVT_TIME " +
                                   $"AND A.CARD_NO = B.CARD_NO " +
                                   $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                   $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                   $"AND B.HUMAN_ID != '-1')";

                                using (OracleCommand comm =new OracleCommand(query, conn))
                                {
                                    comm.Transaction = transaction;
                                    try
                                    {
                                        await comm.ExecuteNonQueryAsync();
                                        transaction.Commit();
                                    }
                                    catch(OracleException ex)
                                    {
                                        await Loggers.ERRORMessage(ex.ToString());
                                        Console.WriteLine(ex.Message);
                                    }
                                }

                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;

                // 근태
                case 2:
                    try
                    {
                        IsBusy = true;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();
                            using (var transaction = conn.BeginTransaction())
                            {
                                string query = $"DELETE FROM GUNTAE_EVENT A WHERE A.ROWID > (" +
                                   $"SELECT MIN(B.ROWID) FROM GUNTAE_EVENT B WHERE A.MC_ID = B.MC_ID " +
                                   $"AND A.SC_ID = B.SC_ID " +
                                   $"AND A.DOOR_ID = B.DOOR_ID " +
                                   $"AND A.EVT_TIME = B.EVT_TIME " +
                                   $"AND A.CARD_NO = B.CARD_NO " +
                                   $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                   $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                   $"AND B.HUMAN_ID != '-1')";

                                using (OracleCommand comm = new OracleCommand(query, conn))
                                {
                                    comm.Transaction = transaction;
                                    try
                                    {
                                        await comm.ExecuteNonQueryAsync();
                                        transaction.Commit();
                                    }
                                    catch (OracleException ex)
                                    {
                                        await Loggers.ERRORMessage(ex.ToString());
                                        Console.WriteLine(ex.Message);
                                    }
                                }

                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;

                // 식수
                case 3:
                    try
                    {
                        IsBusy = true;
                        using (OracleConnection conn = new OracleConnection(connStr))
                        {
                            await conn.OpenAsync();
                            using (var transaction = conn.BeginTransaction())
                            {
                                string query = $"DELETE FROM SIKSU_EVENT A WHERE A.ROWID > (" +
                                            $"SELECT MIN(B.ROWID) FROM SIKSU_EVENT B WHERE A.MC_ID = B.MC_ID " + // 여기 오류
                                            $"AND A.SC_ID = B.SC_ID " +
                                            $"AND A.DOOR_ID = B.DOOR_ID " +
                                            $"AND A.EVT_TIME = B.EVT_TIME " +
                                            $"AND A.CARD_NO = B.CARD_NO " +
                                            $"AND A.HUMAN_ID = B.HUMAN_ID " +
                                            $"AND TO_CHAR(B.EVT_TIME, 'YYYYMMDD') BETWEEN '{StartDate}' AND '{EndDate}' " +
                                            $"AND B.HUMAN_ID !='-1')";

                                using (OracleCommand comm = new OracleCommand(query, conn))
                                {
                                    comm.Transaction = transaction;
                                    try
                                    {
                                        await comm.ExecuteNonQueryAsync();
                                        transaction.Commit();
                                    }
                                    catch (OracleException ex)
                                    {
                                        await Loggers.ERRORMessage(ex.ToString());
                                        Console.WriteLine(ex.Message);
                                    }
                                }

                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        await Loggers.ERRORMessage(ex.ToString());
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                    break;
            }
            CommDATA.OK_MESSAGE("중복데이터 삭제성공");
        }

        private async Task GetLogFileSearch()
        {
            if(MenuType == 0)
            {
                CommDATA.ERROR_MESSAGE("메뉴를 선택해주세요");
                return;
            }

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
                            AccessResult = LogConverterData
                                    .Where(m => !SiksuList.Contains(m.MC)
                                            && m.DATATYPE == "E"
                                            && m.COMMAND == "c"
                                            && Convert.ToChar(m.DATA.Substring(22, 1)) > 0x38
                                            && m.DATA.Substring(25, 1) != "*")
                                    .Select(m => new ECodeFormatModel
                                    {
                                        LEN = m.LEN,
                                        RAN = m.RAN,
                                        M_CODE = m.M_CODE,
                                        LCC = m.LCC,
                                        // MC와 SC가 붙어있는 문자열에서 MC만 추출 (SC는 별도 필드에 이미 들어있음)
                                        MC = m.MC.Substring(0, 9),
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
                                        CardData = m.DATA.Substring(25, m.DATA.Length - 5 - 25),
                                        CardID = m.DATA.Substring(m.DATA.Length - 5)
                                    })
                                    .Select(item =>
                                        $"INSERT INTO INOUT_EVENT(MC_ID, SC_ID, DOOR_ID, EVT_TIME, CR_NO, CR_LOCATION, DOOR_MODE, CARD_AUTH, DOOR_STATUS, BUTTON_STATUS, CARD_LENGTH, CARD_NO, CARD_ID, HUMAN_ID, EVT_REASON, COMMAND_CODE, EVT_CODE, MSG_ID) " +
                                        $"VALUES(" +
                                            $"'{item.MC}'," +                  // MC_ID
                                            $"'{item.SC}'," +                  // SC_ID
                                            $"'{item.DoorNumber}'," +          // DOOR_ID
                                            $"timestamp'20{item.YYMMDD.Substring(0, 2)}-{item.YYMMDD.Substring(2, 2)}-{item.YYMMDD.Substring(4, 2)} " +
                                            $"{item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}'," + // EVT_TIME
                                            $"'0{item.CardReaderNumber}', " + // CR_NO
                                            $"'{item.CardReaderPosition}', " + // CR_LOCATION
                                            $"'{item.Mode}'," +                // DOOR_MODE
                                            $"'0', " +                        // CARD_AUTH (고정값 '0')
                                            $"'{item.DoorState}', " +          // DOOR_STATUS
                                            $"'{item.ButtonState}'," +         // BUTTON_STATUS
                                            $"{item.CardLen}, " +              // CARD_LENGTH
                                            $"'{item.CardData}', " +           // CARD_NO
                                            $"'{item.CardID}', " +             // CARD_ID
                                            $"FN_GET_HUMANID('{item.CardData}'), " + // HUMAN_ID (함수 호출)
                                            $"'{item.Reason}', " +             // EVT_REASON
                                            $"'{item.Code}', " +               // COMMAND_CODE
                                            $"'{item.Result}', " +             // EVT_CODE
                                            $"'{item.CS}')"                    // MSG_ID
                                    )
                                    .ToList();

                            
                            // 최종적으로 생성한 쿼리 문자열 리스트를 CollectionView에 할당
                            QueryList = CollectionViewSource.GetDefaultView(AccessResult); // 실제 데이터는 AccessList에 있다
                            QueryList.Filter = FilterQueries;
                            
                            CommDATA.OK_MESSAGE($"{AccessResult.Count}건 검색되었습니다.");
                            break;

                        // 근태
                        case 2:
                            var AttendanceResult = await Task.Run(() =>
                            {
                                // 여기서 전체 데이터 처리 작업을 수행합니다.
                                var result = LogConverterData
                                    .Where(m => !SiksuList.Contains(m.MC)
                                            && m.DATATYPE == "E"
                                            && m.COMMAND == "c"
                                            && Convert.ToChar(m.DATA.Substring(22, 1)) < 0x39
                                            && m.DATA.Substring(25, 1) != "*")
                                    .Select(m => new ECodeFormatModel
                                    {
                                        LEN = m.LEN,
                                        RAN = m.RAN,
                                        M_CODE = m.M_CODE,
                                        LCC = m.LCC,
                                        MC = m.MC.Substring(0, 9),
                                        SC = m.SC,
                                        COMMAND = m.COMMAND,
                                        MSG = m.MSG,
                                        DATATYPE = m.DATATYPE,
                                        DATA = m.DATA,
                                        CS = m.CS,
                                        // ========= CardFormat =========== //
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
                                        CardData = m.DATA.Substring(25, m.DATA.Length - 5 - 25),
                                        CardID = m.DATA.Substring(m.DATA.Length - 5)
                                    })
                                    .Select(item =>
                                        $"INSERT INTO GUNTAE_EVENT(MC_ID, SC_ID, DOOR_ID, EVT_TIME, CR_NO, CR_LOCATION, DOOR_MODE, CARD_AUTH, DOOR_STATUS, BUTTON_STATUS, CARD_LENGTH, CARD_NO, CARD_ID, HUMAN_ID, EVT_REASON, COMMAND_CODE, EVT_CODE, MSG_ID) " +
                                        $"VALUES('{item.MC}', " +
                                        $"'{item.SC}', " +
                                        $"'{item.DoorNumber}', " +
                                        $"timestamp'20{item.YYMMDD.Substring(0, 2)}-{item.YYMMDD.Substring(2, 2)}-{item.YYMMDD.Substring(4, 2)} {item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}', " +
                                        $"'0{item.CardReaderNumber}'," +
                                        $"'{item.CardReaderPosition}'," +
                                        $"'{item.Mode}'," +
                                        $"'0'," +
                                        $"'{item.DoorState}'," +
                                        $"DECODE('{item.ButtonState}','5','1','6','2','7','3','8','4','{item.ButtonState}')," +
                                        $"{item.CardLen}," +
                                        $"'{item.CardData}'," +
                                        $"'{item.CardID}'," +
                                        $"FN_GET_HUMANID('{item.CardData}')," +
                                        $"'{item.Reason}'," +
                                        $"'{item.Code}'," +
                                        $"'{item.Result}'," +
                                        $"'{item.CS}')"
                                    )
                                    .ToList();
                                return result;
                            });

                            // UI 업데이트: UI 스레드에서 QueryList 업데이트
                            QueryList = CollectionViewSource.GetDefaultView(AttendanceResult);
                            QueryList.Filter = FilterQueries;
                            CommDATA.OK_MESSAGE($"{AttendanceResult.Count}건 검색되었습니다.");
                            break;
                        // 식수
                        case 3:
                            SiksuResult = LogConverterData
                                .Where(m => SiksuList.Contains(m.MC) && m.DATATYPE == "E" && m.COMMAND == "c")
                                .Select(m => new ECodeFormatModel
                                {
                                    LEN = m.LEN,
                                    RAN = m.RAN,
                                    M_CODE = m.M_CODE,
                                    LCC = m.LCC,
                                    MC = m.MC.Substring(0, 9),
                                    SC = m.SC,
                                    COMMAND = m.COMMAND,
                                    MSG = m.MSG,
                                    DATATYPE = m.DATATYPE,
                                    DATA = m.DATA,
                                    CS = m.CS,

                                    // ========== CardFormat ============= //
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
                                    CardData = m.DATA.Substring(25, m.DATA.Length - 5 - 25),
                                    CardID = m.DATA.Substring(m.DATA.Length - 5)
                                })
                                .Select(item =>
                                    $"INSERT INTO SIKSU_EVENT(MC_ID, SC_ID, DOOR_ID, EVT_TIME, CR_NO, CR_LOCATION, DOOR_MODE, CARD_AUTH, DOOR_STATUS, BUTTON_STATUS, CARD_LENGTH, CARD_NO, CARD_ID, HUMAN_ID, EVT_REASON, COMMAND_CODE, EVT_CODE, MSG_ID) " +
                                    $"VALUES('{item.MC}', " + // MC_ID -- MD_ID||SC_ID 인채로 들어가야하는지?
                                    $"'{item.SC}', " + // SC_ID
                                    $"'{item.DoorNumber}', " + // DOOR_ID
                                    $"timestamp'20{item.YYMMDD.Substring(0, 2)}-{item.YYMMDD.Substring(2, 2)}-{item.YYMMDD.Substring(4, 2)} {item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}', " + // EVT_TIME
                                    $"'0{item.CardReaderNumber}', " + // CR_NO
                                    $"'{item.CardReaderPosition}', " + // CR_LOCATION ======================= 1
                                    $"'{item.Mode}', " + // DOOR_MODE ========================= 0
                                    $"'0', " + // CARD_AUTH  ============================ 0
                                    $"'{item.DoorState}', " + // DOOR_STATUS =================== 'O'
                                    $"'{item.ButtonState}', " + // BUTTON_STATUS ===================== 'C'
                                    $"{item.CardLen}, " + // CARD_LENGTH
                                    $"'{item.CardData}', " + // CARD_NO
                                    $"'{item.CardID}', " + // CARD_ID
                                    $"FN_GET_HUMANID('{item.CardData}'), " + // HUMAN_ID
                                    $"'{item.Reason}', " + // EVT_REASON
                                    $"'{item.Code}', " + // COMMAND_CODE
                                    $"'{item.Result}', " + // EVT_CODE
                                    $"'{item.CS}')" // MSG_ID
                                ).ToList();

                            // 최종적으로 생성한 쿼리 문자열 리스트를 CollectionView에 할당
                            QueryList = CollectionViewSource.GetDefaultView(SiksuResult);
                            QueryList.Filter = FilterQueries;

                            CommDATA.OK_MESSAGE($"{SiksuResult.Count}건 검색되었습니다.");
                            break;
                        // 방범
                        case 4:
                            PreventionResult = LogConverterData
                                .Where(m => !SiksuList.Contains(m.MC) && m.COMMAND == "A")
                                .Select(m => new ACodeFormatModel
                                {
                                    LEN = m.LEN,
                                    RAN = m.RAN,
                                    M_CODE = m.M_CODE,
                                    LCC = m.LCC,
                                    MC = m.MC,
                                    SC = m.SC,
                                    COMMAND = m.COMMAND,
                                    MSG = m.MSG,
                                    DATATYPE = m.DATATYPE,
                                    DATA = m.DATA,
                                    CS = m.CS,

                                    // =========== CardFormat ================== //
                                    Code = m.DATA.Substring(0, 1), // ON/OFF
                                    YYMMDD = m.DATA.Substring(1, 8), // YYMMDD
                                    HHMMSS = m.DATA.Substring(9, 6), // HHMMSS
                                    SubClass = m.DATA.Substring(15, 2), // Sub Class (컨트롤 및 기기 구분)
                                    SubAddress = m.DATA.Substring(17, 2), // SUb Addresss (기기 Code)
                                    Spare = m.DATA.Substring(19, 2), // 예비
                                    Mode = m.DATA.Substring(21, 1), // MODE
                                    Status = m.DATA.Substring(22, 2), // Status
                                    LoopPosition = m.DATA.Substring(24, 2), // Loop 위치
                                    LoopStatus = m.DATA.Substring(26, 1), // Loop 상태
                                    CardLength = m.DATA.Substring(27, 2), // 카드길이
                                    CardData = m.DATA.Substring(29, m.DATA.Length - 29) // 카드데이터
                                })
                                .Select(item =>
                                    $"INSERT INTO ALARM_EVENT(MC_ID, SC_ID, EVT_CODE, EVT_TIME, OCCUR_ON, SUB_CLASS, SUB_ADDR, SECURITY_MODE, LOOP_NO, LOOP_STATUS, CARD_FLAG, CARD_LENGTH, CARD_NO, DEAL_YN, SEND_SMS, COMMAND_CODE, IS_ALARM, HUMAN_ID, WORK_DATE, MSG_ID, EVT_ID) " +
                                        $"VALUES('{item.MC}', " + // MC_ID
                                        $"'{item.SC}', " + // SC_ID
                                        $"'{item.Status}', " + // EVT_CODE
                                        $"timestamp'{item.YYMMDD.Substring(0, 4)}-{item.YYMMDD.Substring(4, 2)}-{item.YYMMDD.Substring(6, 2)} {item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}', " + // EVT_TIME
                                        $"'{item.Code}', " + // OCCUR_ON
                                        $"'{item.SubClass}', " + // SUB_CLASS
                                        $"'{item.SubAddress}', " + // SUB_ADDR
                                        $"'{item.Mode}', " + // SECURITY_MODE
                                        $"'{item.LoopPosition}', " + // LOOP_NO
                                        $"'{item.LoopStatus}', " + // LOOP_STATUS
                                        $"'00', " + // CARD_FLAG
                                        $"'{item.CardLength}', " + // CARD_LENGTH
                                        $"'{item.CardData}', " + // CARD_NO
                                        $"'0', " + // DEAL_YN
                                        $"'0', " + // SEND_SMS
                                        $"'{item.COMMAND}', " + // COMMAND_CODE
                                        $"'0', " + // IS_ALARM
                                        $"FN_GET_HUMANID('{item.CardData}'), " + // HUMAN_ID
                                        $"timestamp'{item.YYMMDD.Substring(0, 4)}-{item.YYMMDD.Substring(4, 2)}-{item.YYMMDD.Substring(6, 2)} {item.HHMMSS.Substring(0, 2)}:{item.HHMMSS.Substring(2, 2)}:{item.HHMMSS.Substring(4, 2)}', " + // WORK_DATE
                                        $"'{item.CS}', " + // MSG_ID
                                        $"ALARM_EVENT_SEQ.NEXTVAL)"
                                ).ToList();

                            QueryList = CollectionViewSource.GetDefaultView(PreventionResult);
                            QueryList.Filter = FilterQueries;

                            CommDATA.OK_MESSAGE($"{PreventionResult.Count}건 검색되었습니다.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await Loggers.ERRORMessage(ex.ToString());
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private bool FilterQueries(object obj)
        {
            if (obj is string query)
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                    return true;  // 검색어가 없으면 전체 표시

                // 대소문자 구분 없이 검색어 포함 여부 검사
                return query.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return false;
        }

    }
}
