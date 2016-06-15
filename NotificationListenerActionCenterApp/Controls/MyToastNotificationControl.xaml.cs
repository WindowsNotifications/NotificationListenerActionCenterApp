using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NotificationListenerActionCenterApp.Controls
{
    public sealed partial class MyToastNotificationControl : UserControl
    {
        public MyToastNotificationControl()
        {
            this.InitializeComponent();
        }

        public UserNotification UserNotification
        {
            get { return (UserNotification)GetValue(UserNotificationProperty); }
            set { SetValue(UserNotificationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserNotification.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNotificationProperty =
            DependencyProperty.Register("UserNotification", typeof(UserNotification), typeof(MyToastNotificationControl), new PropertyMetadata(null, OnUserNotificationChanged));

        private static void OnUserNotificationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyToastNotificationControl).OnUserNotificationChanged(e);
        }

        private void OnUserNotificationChanged(DependencyPropertyChangedEventArgs e)
        {
            if (UserNotification != null)
                UpdateFromUserNotification();
        }

        private void UpdateFromUserNotification()
        {
            UpdateAppIcon();
            UpdateAppName();
            UpdateTimeStamp();
            UpdateTextContent();
        }

        private void UpdateTextContent()
        {
            this.StackPanelTextElements.Children.Clear();

            string[] textElements = null;

            try
            {
                var binding = UserNotification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);

                if (binding == null)
                {
                    // In an actual app, we should simply exclude this notification if the binding is null
                    textElements = new string[] { "ToastGeneric binding was null" };
                }

                else
                {
                    textElements = binding.GetTextElements().Where(i => !string.IsNullOrWhiteSpace(i.Text)).Select(i => i.Text).ToArray();
                }
            }

            catch (Exception ex) { textElements = new string[] { "Exception: " + ex.ToString() }; }
            
            for (int i = 0; i < textElements.Length; i++)
            {
                string text = textElements[i];

                TextBlock tb = new TextBlock()
                {
                    Text = text,
                    Opacity = i == 0 ? 1 : 0.6,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxLines = i == 0 ? 2 : 4,
                    Style = (Style)Resources["BodyTextBlockStyle"]
                };

                this.StackPanelTextElements.Children.Add(tb);
            }
        }

        private void UpdateTimeStamp()
        {
            try
            {
                DateTime timeReceived = UserNotification.CreationTime.DateTime;

                if (timeReceived.Date == DateTime.Today)
                {
                    this.TextBlockTime.Text = timeReceived.ToString("t");
                }

                else
                {
                    this.TextBlockTime.Text = timeReceived.ToString("d");
                }
            }

            catch { }
        }

        private void UpdateAppName()
        {
            try
            {
                this.TextBlockAppName.Text = UserNotification.AppInfo.DisplayInfo.DisplayName;
            }

            catch { }
        }

        private async void UpdateAppIcon()
        {
            try
            {
                this.ImageAppIcon.Source = null;

                AppInfo appInfo = UserNotification.AppInfo;
                RandomAccessStreamReference streamReference = appInfo.DisplayInfo.GetLogo(new Size(16, 16));

                // In an actual app, we would probably load these images before the notification is displayed
                // so that the images don't pop in
                BitmapImage appLogo = new BitmapImage();
                this.ImageAppIcon.Source = appLogo;
                await appLogo.SetSourceAsync(await streamReference.OpenReadAsync());
            }

            catch { }
        }
    }
}
