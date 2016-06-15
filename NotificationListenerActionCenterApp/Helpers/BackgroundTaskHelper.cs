using NotificationListenerActionCenterApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NotificationListenerActionCenterApp.Helpers
{
    public static class BackgroundTaskHelper
    {
        public const string TASK_NAME = "ListenerTask";

        public static void RegisterIfNotRegistered()
        {
            // If background task isn't registered yet
            if (!IsBackgroundTaskRegistered())
            {
                // Specify the background task
                var builder = new BackgroundTaskBuilder()
                {
                    Name = TASK_NAME
                };

                // Set the trigger for Listener, listening to Toast Notifications
                builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));

                // Register the task
                builder.Register();
            }
        }

        private static bool IsBackgroundTaskRegistered()
        {
            return BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(TASK_NAME));
        }

        public static void HandleBackgroundTaskActivated(BackgroundActivatedEventArgs args)
        {
            switch (args.TaskInstance.Task.Name)
            {
                case TASK_NAME:
                    HandleListenerTaskActivated();
                    break;
            }
        }

        public static void HandleListenerTaskActivated()
        {
            try
            {
                MainPageViewModel viewModel = ((Window.Current.Content as Frame)?.Content as MainPage)?.ViewModel;

                if (viewModel != null)
                {
                    viewModel.UpdateNotifications();
                }
            }

            catch { }
        }
    }
}
