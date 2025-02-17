using System.Windows;

namespace AutoLoggerV2.Services.Common
{
    public class AppSettings : IAppSettings
    {
        /// <summary>
        /// 프로그램 설치파일 경로
        /// </summary>
        public string ROOT => AppDomain.CurrentDomain.BaseDirectory;

        public string SETTINGPATH => $"{ROOT}\\Settings.json";

        
    }
}
