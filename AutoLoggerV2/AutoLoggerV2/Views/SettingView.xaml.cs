﻿using System.Diagnostics;
using System.Windows.Controls;

namespace AutoLoggerV2.Views
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();
            Debug.WriteLine(typeof(SettingView));
        }
    }
}
