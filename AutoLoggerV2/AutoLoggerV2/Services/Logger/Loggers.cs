using System.Diagnostics;
using System.IO;

namespace AutoLoggerV2.Services.Logger
{
    public class Loggers : ILoggers
    {
        /// <summary>
        /// 정보성 로그
        /// </summary>
        /// <param name="message"></param>
        public async Task INFOMEssage(string message)
        {
            try
            {
                DateTime Today = DateTime.Now;

                // 년도 폴더 경로
                string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemLog", Today.Year.ToString());

                var di = new DirectoryInfo(dirPath);

                // 년도 폴더 없으면 생성
                if(!di.Exists)
                {
                    di.Create();
                }

                // 월 폴더 경로
                dirPath = Path.Combine(dirPath, Today.Month.ToString());
                di = new DirectoryInfo(dirPath);

                // 월 폴더 없으면 생성
                if(!di.Exists)
                {
                    di.Create();
                }

                // 일 파일 경로
                dirPath = Path.Combine(dirPath, $"{Today.Year}_{Today.Month}_{Today.Day}.txt");

                // 일.txt + 로그내용
                using (StreamWriter writer = new StreamWriter(dirPath, true))
                {
                    await writer.WriteLineAsync($"[ERROR]_[{Today.ToString()}]\t{message}");
                }
#if DEBUG
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
#endif
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 에러 로그
        /// </summary>
        /// <param name="message"></param>
        public async Task ERRORMessage(string message)
        {
            try
            {
                DateTime Today = DateTime.Now;

                // 년도 폴더 경로
                string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemLog", Today.Year.ToString());
                var di = new DirectoryInfo(dirPath);

                // 년도 폴더 없으면 생성
                if(!di.Exists)
                {
                    di.Create();
                }

                // 월 폴더 경로
                dirPath = Path.Combine(dirPath, Today.Month.ToString());
                di = new DirectoryInfo(dirPath);

                // 월 폴더 없으면 생성
                if(!di.Exists)
                {
                    di.Create();
                }

                // 일 파일 경로
                dirPath = Path.Combine(dirPath, $"{Today.Year}_{Today.Month}_{Today.Day}.txt");

                // 일.txt + 로그내용
                using (StreamWriter writer = new StreamWriter(dirPath, true))
                {
                    //var objStackTrace = new StackTrace(new System.Diagnostics.StackFrame(1));

                    //// 호출한 함수 위치
                    //var methodPath = objStackTrace.ToString();
                    await writer.WriteLineAsync($"[ERROR]_[{Today.ToString()}]\t{message}");
                }
#if DEBUG
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
#endif
            }
            catch (Exception ex)
            {
                throw;
            }
        }

       
    }
}
