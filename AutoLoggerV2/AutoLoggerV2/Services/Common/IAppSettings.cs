namespace AutoLoggerV2.Services.Common
{
    public interface IAppSettings
    {
        /// <summary>
        /// 프로그램 설치파일 경로
        /// </summary>
        string ROOT { get; }

        /// <summary>
        /// 설정파일 경로
        /// </summary>
        string SETTINGPATH { get; }

    }
}
