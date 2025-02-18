using AutoLoggerV2.Models;

namespace AutoLoggerV2.ViewModels
{
    /// <summary>
    /// 방범 DataFormat
    /// Command Code : 'A' (0x41)
    /// </summary>
    public class ACodeFormatModel : DataFormatModel
    {
        /// <summary>
        /// [ON / OFF]
        /// 발생시 통신상태유무(MC 기준, SC는 'f'로 고정)
        /// 'n' : ON Line 시 발생, 'f' : Off Line 시 발생
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// [YYYYMMDD]
        /// 년월일
        /// </summary>
        public string? YYMMDD { get; set; }

        /// <summary>
        /// [HHMMSS]
        /// 시분초
        /// </summary>
        public string? HHMMSS { get; set; }

        /// <summary>
        /// [Sub Class (컨트롤 및 기기 구분)]
        /// ** : 해당 없음
        /// MC : main Controller
        /// SC : Sub Controller
        /// CR : Card Reader
        /// BC : Data Backup Center (DBC)
        /// </summary>
        public string? SubClass { get; set; }

        /// <summary>
        /// [Sub Address (기기 Code)]
        /// ** : 해당 없음
        /// 00 ~ 99 : (예 : 카드리더 번호)
        /// </summary>
        public string? SubAddress { get; set; }

        /// <summary>
        /// 예비
        /// 00
        /// </summary>
        public string? Spare { get; set; }

        /// <summary>
        /// [Mode]
        /// a : 경계모드
        /// d : 해제모드
        /// </summary>
        public string? Mode { get; set; }

        /// <summary>
        /// [Status] (추가 상태는 Status 알람표 참조)
        /// 00 : 정상상태
        /// T1 : 일반침입
        /// T4 : 금고침입
        /// F1 : 화재발생
        /// G1 : 가스누출
        /// E1 : 일반비상
        /// E2 : 당사비상
        /// S1 : 일반설비이상
        /// NF : 통신이상
        /// NR : 통신복구
        /// C0 : 뚜껑열림
        /// CC : 뚜껑닫힘
        /// AN : AN 전원 ON
        /// AF : AC 전원 OFF
        /// AR : 재시작
        /// BR : 배터리 복구
        /// BL : 배터리 Low
        /// BF : 배터리 불량
        /// EM : 등록 변경
        /// OV : 이벤트 Overflow
        /// IF : 세트 연동이상
        /// IR : 세트 연동 복구
        /// CF : 확장모듈 연결이상
        /// CR : 확장모듈 연결복구
        /// * 감시존 탐지 시 감시존 상태코드 올라옴
        /// Duress Code 사용시 E2로 올라옴
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// [Loop 위치 (감지 존)]
        /// ** : 해당 없음
        /// 01 ~ 12
        /// </summary>
        public string? LoopPosition { get; set; }

        /// <summary>
        /// [Loop 상태 (선로 상태)]
        /// * : 해당 없음
        /// N : 복구, 정상
        /// S : 단락 이상
        /// 0 : 단선 이상(영문 대문자 '0')
        /// </summary>
        public string? LoopStatus { get; set; }

        /// <summary>
        /// [Card 길이]
        /// 10진수 Card Data 길이 ('09' 고정)
        /// </summary>
        public string? CardLength { get; set; }

        /// <summary>
        /// [Card Data]
        /// 조작한 사람의 카드 번호를 나타낸다
        /// (카드('C'), 지문('F') + 카드번호 8자리)
        /// ********* : 해당 없음, 이상발생 Data
        /// ****RESET : Power ON/Reset 발생
        /// *Recovery : 복구조작시 Data
        /// *00000000 : 서버에서 원격제어로 경계/해제
        /// *ForceRel : 기기간 Relay 연동으로 경계/해제
        /// </summary>
        public string? CardData { get; set; }

    }
}
