using System.Windows.Input;

namespace AutoLoggerV2.Commands
{
    /// <summary>
    /// ICommand 인터페이스의 구현체이다
    /// 실제 로직과 실행 가능 여부를 결정하는 조건을 지정할 수 있다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        /// <summary>
        /// RelayCommand 생성자이다.
        /// </summary>
        /// <param name="_execute"></param>
        /// <param name="_canExecute"></param>
        public RelayCommand(Action<T> _execute, Predicate<T> _canExecute = null)
        {
            execute = _execute ?? throw new ArgumentNullException(nameof(execute));
            canExecute = _canExecute;
        }

        /// <summary>
        /// 명령이 실행 가능한지 여부를 결정한다.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        /// <summary>
        /// 명령 실행 시 호추된다.
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object? parameter)
        {
            execute((T)parameter);
        }

        /// <summary>
        /// CanExecute 상태가 변경되었음을 알리기 위한 이벤트이다.
        /// CommandManage가 이 이벤트를 이용해 커맨드 활성 상태를 재평가한다.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
