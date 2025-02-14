namespace AutoLoggerV2.Models
{
    public class SettingModel 
    {
        /// <summary>
        /// 파일경로
        /// </summary>
        public string? FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 IP
        /// </summary>
        public string? DBIp { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 PORT
        /// </summary>
        public string? DBPort { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 ID
        /// </summary>
        public string? DBId { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 PORT
        /// </summary>
        public string? DBPw { get; set; } = string.Empty;

        /// <summary>
        /// 데이터베이스 NAME
        /// </summary>
        public string? DBName { get; set; } = string.Empty;

        
    }
}
