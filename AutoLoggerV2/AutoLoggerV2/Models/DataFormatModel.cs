namespace AutoLoggerV2.Models
{
    /// <summary>
    /// 로그 데이터형식 모델클래스
    /// </summary>
    public class DataFormatModel
    {
        /// <summary>
        /// * [3 byte]
        /// STX - ETX 까지의 길이를 나타냄
        /// </summary>
        public string? LEN { get; set; }

        /// <summary>
        /// * [1 byte]
        /// 난수표시
        /// (사용안함, 공백-0x20)
        /// </summary>
        public string? RAN { get; set; }

        /// <summary>
        /// * [2 byte]
        /// Data Format의 Version을 나타냄
        /// [ST = 0x53, 0x54]
        /// </summary>
        public string? M_CODE { get; set; }

        /// <summary>
        /// * [8 byte]
        /// LCC 코드(DBC에서 부여)
        /// LMS N0 -> LCC + MC + SC
        /// </summary>
        public string? LCC { get; set; }

        /// <summary>
        /// * [9 byte]
        /// Main Controller ID
        /// </summary>
        public string? MC { get; set; }

        /// <summary>
        /// * [2 byte]
        /// Controller ID(MC: 00, SC1: 01, SC2:02)
        /// </summary>
        public string? SC { get; set; }

        /// <summary>
        /// * [1 byte]
        /// 작업구분
        /// </summary>
        public string? COMMAND { get; set; }

        /// <summary>
        /// * [1 byte]
        /// DATA 수신 체크
        /// </summary>
        public string? MSG { get; set; }

        /// <summary>
        /// 데이터타입 정의
        /// </summary>
        public string? DATATYPE { get; set; }

        /// <summary>
        /// * [N byte]
        /// DATA 영역
        /// </summary>
        public string? DATA { get; set; }

        /// <summary>
        /// * [2 byte]
        /// </summary>
        public string? CS { get; set; }
    }
}
