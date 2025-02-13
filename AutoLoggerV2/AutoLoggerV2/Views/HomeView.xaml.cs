using System.Windows.Controls;

namespace AutoLoggerV2.Views
{
    /// <summary>
    /// HomeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            Console.WriteLine("홈뷰호출");
        }
    }
}
