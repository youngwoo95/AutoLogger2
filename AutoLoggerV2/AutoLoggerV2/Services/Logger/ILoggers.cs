namespace AutoLoggerV2.Services.Logger
{
    public interface ILoggers
    {
        /// <summary>
        /// 정보성 로그
        /// </summary>
        /// <param name="message"></param>
        public Task INFOMEssage(string message);

        /// <summary>
        /// 에러로그
        /// </summary>
        /// <param name="message"></param>
        public Task ERRORMessage(string message);
    }
}
