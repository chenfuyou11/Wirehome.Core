using System;
using System.Collections.Generic;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Storage;
using Newtonsoft.Json.Linq;

namespace Wirehome.Notifications
{
    [ApiServiceClass(typeof(INotificationService))]
    public class NotificationService : ServiceBase, INotificationService
    {
        private const string StorageFilename = "NotificationService.json";

        private readonly object _syncRoot = new object();
        private readonly List<Notification> _notifications = new List<Notification>();
        private readonly IDateTimeService _dateTimeService;
        private readonly IStorageService _storageService;
        private readonly IResourceService _resourceService;
        private readonly ILogger _log;

        public NotificationService(
            IDateTimeService dateTimeService, 
            IApiDispatcherService apiService, 
            ISchedulerService schedulerService, 
            ISettingsService settingsService,
            IStorageService storageService,
            IResourceService resourceService,
            IScriptingService scriptingService,
            ILogService logService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));

            _log = logService.CreatePublisher(nameof(NotificationService));
            settingsService.CreateSettingsMonitor<NotificationServiceSettings>(s => Settings = s.NewSettings);

            apiService.StatusRequested += HandleApiStatusRequest;

            schedulerService.Register("NotificationCleanup", TimeSpan.FromMinutes(15), () => Cleanup());

            scriptingService.RegisterScriptProxy(s => new NotificationScriptProxy(this));
        }

        public NotificationServiceSettings Settings { get; private set; }

        public void Initialize()
        {
            lock (_syncRoot)
            {
                TryLoadNotifications();
            }
        }

        public void Create(NotificationType type, string message, TimeSpan timeToLive)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            lock (_syncRoot)
            {
                var notification = new Notification(Guid.NewGuid(), type, _dateTimeService.Now, message, timeToLive);
                _notifications.Add(notification);

                SaveNotifications();
            }
        }

        public void CreateInfo(string text)
        {
            Create(NotificationType.Information, text, Settings.InformationTimeToLive);
        }

        public void CreateInformation(Enum resourceId, params object[] formatParameterObjects)
        {
            CreateInfo(_resourceService.GetText(resourceId, formatParameterObjects));
        }

        public void CreateWarning(string text)
        {
            Create(NotificationType.Warning, text, Settings.WarningTimeToLive);
        }

        public void CreateError(string text)
        {
            Create(NotificationType.Error, text, Settings.ErrorTimeToLive);
        }

        [ApiMethod]
        public void Create(IApiCall apiCall)
        {
            var parameter = apiCall.Parameter.ToObject<ApiParameterForCreate>();
            if (parameter == null)
            {
                apiCall.ResultCode = ApiResultCode.InvalidParameter;
                return;
            }

            Create(parameter.Type, parameter.Text, parameter.TimeToLive);
        }

        [ApiMethod]
        public void Delete(IApiCall apiCall)
        {
            var notificationUid = (string)apiCall.Parameter["Uid"];
            if (string.IsNullOrEmpty(notificationUid))
            {
                // TODO BadRequestException
                throw new Exception("Parameter 'Uid' is not specified.");
            }

            lock (_syncRoot)
            {
                var uid = Guid.Parse(notificationUid);
                var removedItems = _notifications.RemoveAll(n => n.Uid.Equals(uid));

                if (removedItems > 0)
                {
                    _log.Verbose($"Manually deleted notification '{notificationUid}'");
                    SaveNotifications();
                }
            }
        }

        private void Cleanup()
        {
            lock (_syncRoot)
            {
                _log.Verbose("Starting notification cleanup.");

                var now = _dateTimeService.Now;
                var removedItems = _notifications.RemoveAll(n => n.Timestamp.Add(n.TimeToLive) < now);

                _log.Verbose($"Deleted {removedItems} obsolete notifications.");
                if (removedItems > 0)
                {
                    SaveNotifications();
                }
            }
        }

        private void HandleApiStatusRequest(object sender, ApiRequestReceivedEventArgs e)
        {
            lock (_syncRoot)
            {
                e.ApiContext.Result["Notifications"] = JArray.FromObject(_notifications);
            }
        }

        private void SaveNotifications()
        {
            _storageService.Write(StorageFilename, _notifications);
        }
        
        private void TryLoadNotifications()
        {
            List<Notification> persistedNotifications;
            if (_storageService.TryRead(StorageFilename, out persistedNotifications))
            {
                _notifications.AddRange(persistedNotifications);
            }
        }
    }
}
