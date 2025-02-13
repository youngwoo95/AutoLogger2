using System.Windows.Controls;

namespace AutoLoggerV2.Views
{
    /// <summary>
    /// LockView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LockView : UserControl
    {
        public LockView()
        {
            InitializeComponent();
            Console.WriteLine("락뷰호출");
        }
    }
}
