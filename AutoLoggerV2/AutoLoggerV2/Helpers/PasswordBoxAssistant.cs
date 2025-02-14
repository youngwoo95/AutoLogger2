using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoLoggerV2.Helpers
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPasswordProperty, value);
        }


        private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                // 만약 내부 업데이트 중이 아니라면 PasswordBox의 Password를 새 값으로 설정
                if (!GetIsUpdating(passwordBox))
                {
                    passwordBox.Password = e.NewValue as string;
                }
            }
        }

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxAssistant),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static bool GetBindPassword(DependencyObject dp) =>
            (bool)dp.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject dp, bool value) =>
            dp.SetValue(BindPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is PasswordBox passwordBox)
            {
                bool wasBound = (bool)e.OldValue;
                bool needToBind = (bool)e.NewValue;

                if (wasBound)
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                    passwordBox.IsVisibleChanged -= PasswordBox_IsVisibleChanged;
                }
                if (needToBind)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                    // 추가: Visibility 변화 시 Binding 업데이트를 위해 이벤트 핸들러 등록
                    passwordBox.IsVisibleChanged += PasswordBox_IsVisibleChanged;
                }
            }
        }

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

        private static bool GetIsUpdating(DependencyObject dp) =>
            (bool)dp.GetValue(IsUpdatingProperty);

        private static void SetIsUpdating(DependencyObject dp, bool value) =>
            dp.SetValue(IsUpdatingProperty, value);

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetIsUpdating(passwordBox, true);
                SetBoundPassword(passwordBox, passwordBox.Password);
                SetIsUpdating(passwordBox, false);
            }
        }

        // 추가된 핸들러: PasswordBox의 Visibility가 변경될 때 Binding 업데이트를 강제함.
        private static void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var binding = passwordBox.GetBindingExpression(BoundPasswordProperty);
                binding?.UpdateSource();
            }
        }

    }
}
