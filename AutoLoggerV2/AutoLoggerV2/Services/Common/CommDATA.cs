using System.Windows;

namespace AutoLoggerV2.Services.Common
{
    public class CommDATA
    {
        

        /// <summary>
        /// OK MESSAGE
        /// </summary>
        /// <param name="message">내용</param>
        public static void OK_MESSAGE(string message) => MessageBox.Show(message, "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        
        /// <summary>
        /// ERROR MESSAGE
        /// </summary>
        /// <param name="message"></param>
        public static void ERROR_MESSAGE(string message) => MessageBox.Show(message, "알림", MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>
        /// ORACLE 연결주소
        /// </summary>
        public static string ORACLE_CONNSTR { get; set; }
     

    }
}
