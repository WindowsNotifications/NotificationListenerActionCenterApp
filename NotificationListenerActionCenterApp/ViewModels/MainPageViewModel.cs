using NotificationListenerActionCenterApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;

namespace NotificationListenerActionCenterApp.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private UserNotificationListener _listener;

        private string _error;

        public string Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value); }
        }

        public ObservableCollection<UserNotification> Notifications { get; private set; } = new ObservableCollection<UserNotification>();

        public MainPageViewModel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                BackgroundTaskHelper.RegisterIfNotRegistered();
            }

            catch (Exception ex)
            {
                Error = "Failed to register background task: " + ex.ToString();
                return;
            }

            try
            {
                _listener = UserNotificationListener.Current;

                // Check listener status
                var accessStatus = await _listener.RequestAccessAsync();
                switch (accessStatus)
                {
                    case UserNotificationListenerAccessStatus.Allowed:
                        // Good, continue through
                        break;

                    case UserNotificationListenerAccessStatus.Denied:
                        Error = "Listener permission has been denied. Please go to your settings and allow listener access for this app, then close and re-launch this app.";
                        return;

                    default:
                        Error = "Please close and re-open this app. Listener permission hasn't been allowed or denied yet.";
                        return;
                }
            }

            catch (Exception ex)
            {
                Error = "Failed requesting access to Listener: " + ex.ToString();
                return;
            }

            UpdateNotifications();
        }

        public void RemoveNotification(uint notifId)
        {
            try
            {
                _listener.RemoveNotification(notifId);
            }

            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "Failed to dismiss notification");
            }

            UpdateNotifications();
        }

        public void ClearAll()
        {
            try
            {
                _listener.ClearNotifications();
            }

            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "Failed to clear all");
            }

            UpdateNotifications();
        }

        public async void UpdateNotifications()
        {
            try
            {
                IReadOnlyList<UserNotification> notifsInPlatform = await _listener.GetNotificationsAsync(NotificationKinds.Toast);

                // Reverse their order since the platform returns them with oldest first, we want newest first
                notifsInPlatform = notifsInPlatform.Reverse().ToList();

                // First remove any notifications that no longer exist
                for (int i = 0; i < this.Notifications.Count; i++)
                {
                    UserNotification existingNotif = this.Notifications[i];

                    // If not in platform anymore, remove from our list
                    if (!notifsInPlatform.Any(n => n.Id == existingNotif.Id))
                    {
                        this.Notifications.RemoveAt(i);
                        i--;
                    }
                }

                // Now our list only contains notifications that exist,
                // but it might be missing new notifications.

                for (int i = 0; i < notifsInPlatform.Count; i++)
                {
                    UserNotification platNotif = notifsInPlatform[i];

                    int indexOfExisting = FindIndexOfNotification(platNotif.Id);

                    // If we have an existing
                    if (indexOfExisting != -1)
                    {
                        // And if it's in the wrong position
                        if (i != indexOfExisting)
                        {
                            // Move it to the right position
                            this.Notifications.Move(indexOfExisting, i);
                        }

                        // Otherwise, leave it in its place
                    }

                    // Otherwise, notification is new
                    else
                    {
                        // Insert at that position
                        this.Notifications.Insert(i, platNotif);
                    }
                }
            }

            catch (Exception ex)
            {
                Error = "Error updating notifications: " + ex.ToString();
            }
        }

        private int FindIndexOfNotification(uint notifId)
        {
            for (int i = 0; i < this.Notifications.Count; i++)
            {
                if (this.Notifications[i].Id == notifId)
                    return i;
            }

            return -1;
        }

        private async void ShowMessage(string content, string title)
        {
            try
            {
                await new MessageDialog(content, title).ShowAsync();
            }

            catch { }
        }
    }
}
