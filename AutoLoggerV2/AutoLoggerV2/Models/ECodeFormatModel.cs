using AutoLoggerV2.Models;

namespace AutoLoggerV2.ViewModels
{
    /// <summary>
    /// 7.5 카드 / 출입 이벤트
    /// </summary>
    public class ECodeFormatModel : DataFormatModel
    {
        /// <summary>
        /// [Code]
        /// 'E' (0x45)
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// [예비]
        /// 0x30
        /// </summary>
        public string? Spare { get; set; }

        /// <summary>
        /// [출입문의 번호]
        /// 1번 문(0x31), 2번 문(0x32)
        /// </summary>
        public string? DoorNumber { get; set; }

        /// <summary>
        /// [Card Reader 번호]
        /// 해당사항없음(0x30), 1~8 Card Reader(0x31 ~ 0x38)
        /// </summary>
        public string? CardReaderNumber { get; set; }

        /// <summary>
        /// [Card Reader 위치]
        /// 내부(0x30), 외부(0x31)
        /// </summary>
        public string? CardReaderPosition { get; set; }

        /// <summary>
        /// [YYMMDD 년월일 (10진수)]
        /// </summary>
        public string? YYMMDD { get; set; }

        /// <summary>
        /// [HHMMSS 시분초 (10진수)]
        /// </summary>
        public string? HHMMSS { get; set; }

        /// <summary>
        /// [Posi/Nega]
        /// Positive(0x30), Negative(0x31)
        /// </summary>
        public string? Posi { get; set; }

        /// <summary>
        /// [운용모드]
        /// 운영(0x30), 개방(0x31), 폐쇄(0x32)
        /// </summary>
        public string? Mode { get; set; }

        /// <summary>
        /// [변경사유]
        /// 해당사항없음(0x30) / 원격제어('R') / 버튼('B') / 카드 ('C') / 스케쥴동작 ('S') / 화재 ('F') / 원격제어화재 ('f') / 순찰 ('V')
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// [출입승인결과]
        /// 해당사항없음(0x30) / 출입승인('1') / 방범승인('2') / 미등록카드('A') / 출입불가('B') / 방범불가('C') / 경계모드출입불가('D') / 출입제한시간('E') / 유효기간만료('F') / Duress Code 출입승인('G') / Duress Code 방범승인('H')
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// [출입문 현재상태]
        /// 해당사항없음(0x30) / 닫힘('C') / 열림('0' : 0x4F)
        /// </summary>
        public string? DoorState { get; set; }

        /// <summary>
        /// [버튼 상태]
        /// 해당사항없음(0x7F) / 출근(0x31) / 퇴근(0x32) / 외출(0x33) / 복귀(0x34) / 'a' : 경계버튼
        /// </summary>
        public string? ButtonState { get; set; }

        /// <summary>
        /// [Card Len]
        /// </summary>
        public string? CardLen { get; set; }

        /// <summary>
        /// [Card Data]
        /// </summary>
        public string? CardData { get; set; }

        /// <summary>
        /// [Card ID]
        /// </summary>
        public string? CardID { get; set; }

    }
}
