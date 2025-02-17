using AutoLoggerV2.Services;
using AutoLoggerV2.Services.Common;
using AutoLoggerV2.Services.Logger;
using AutoLoggerV2.ViewModels;
using AutoLoggerV2.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AutoLoggerV2
{
    public partial class App : Application
    {
        /*
            DI 컨테이너 관련 속성을 인스턴스 멤버로 보관
                - 전역 static 사용 대신 Application.Current를 활용
        */
        public IServiceProvider ServiceProvider { get; private set; }
        public IServiceCollection Services { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            // DataContext를 MainViewModel로 설정
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        public void ConfigureServices()
        {
            Services = new ServiceCollection();

            // 각 페이지를 Transient(필요할 때 마다 새 인스턴스)로 등록
            Services.AddTransient<MainWindow>();
            Services.AddTransient<MainViewModel>();


            Services.AddTransient<HomeView>();
            Services.AddTransient<SettingView>();
            Services.AddTransient<LockView>();


            // 각 페이지의 ViewModel도 등록
            Services.AddTransient<HomeViewModel>();
            Services.AddTransient<SettingViewModel>();
            Services.AddTransient<LockViewModel>();

            // 서비스 등록
            Services.AddSingleton<INavigationService, NavigationService>();
            Services.AddSingleton<IAppSettings, AppSettings>();
            Services.AddSingleton<ILoggers, Loggers>();

            // 추가 서비스나 기타 의존성이 있다면 여기서 등록
            ServiceProvider = Services.BuildServiceProvider();
        }
    }
}
