using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sentry;
using Sentry.Protocol;
using SentryNetXamarinAddon.Enums;
using SentryNetXamarinAddon.Models;
using SentryNetXamarinAddon.Services.Interface;


namespace SentryNetXamarinAddon.Services
{
    public class SentryXamarinService : ISentryXamarinService
    {
        private readonly ISentryXamarinHelper _sentryXamarinHelper;
        private readonly IOfflineCacheHelper _offlineCacheHelper;

        private Random rand = new Random();

        private static string _key = null;

        public SentryXamarinService(ISentryXamarinHelper sentryXamarinHelper,
            IOfflineCacheHelper offlineCacheHelper)
        {
            _sentryXamarinHelper = sentryXamarinHelper;
            _offlineCacheHelper = offlineCacheHelper;
            if (_key != null)
            {
                _key = $"{_sentryXamarinHelper.GetDeviceInfo()}{_sentryXamarinHelper.GetRamAmount()}{_sentryXamarinHelper.IsEmulator()}";
            }

        }

            public void BackupLog(SentryEvent arg)
            {
                /*Faremos backup de todas as partes importantes para a visualização de erro no sentry.io
                 não será feito o backup completo do objeto pois partes dele não são necessários já
                */
                var unhandledError = false;
                var sentryExceptions = arg.SentryExceptions.ToList();
                var StackModule = new List<string>();
                var StackType = new List<string>();
                var StackValue = new List<string>();
                var StackStackTrace = new List<string>();

                /*Store the stack frames*/
                var stackFrameContextLine = new List<string>();
                var stackFrameFileName = new List<string>();
                var stackFrameFunction = new List<string>();
                var stackFrameImgAddress = new List<long>();
                var stackFrameInApp = new List<bool>();
                var stackFrameInstOffset = new List<long>();
                var stackFrameLineNumb = new List<long>();
                var stackFrameModule = new List<string>();
                var stackFramePackage = new List<string>();
                var stackFrameTotalPerStack = new List<int>();

                for (int i = 0, framecnt = 0; i < sentryExceptions.Count(); i++)
                {
                    StackModule.Add(sentryExceptions[i].Module);
                    StackType.Add(sentryExceptions[i].Type);
                    StackValue.Add(sentryExceptions[i].Value);

                    if (sentryExceptions[i].Stacktrace != null)
                    {
                        for (int j = 0; j < sentryExceptions[i].Stacktrace.Frames.Count; j++)
                        {
                            stackFrameContextLine.Add(sentryExceptions[i].Stacktrace.Frames[j].ContextLine);
                            stackFrameFileName.Add(sentryExceptions[i].Stacktrace.Frames[j].FileName);
                            stackFrameFunction.Add(sentryExceptions[i].Stacktrace.Frames[j].Function);
                            stackFrameImgAddress.Add(sentryExceptions[i].Stacktrace.Frames[j].ImageAddress);
                            stackFrameInApp.Add(sentryExceptions[i].Stacktrace.Frames[j].InApp.GetValueOrDefault());
                            stackFrameInstOffset.Add(sentryExceptions[i].Stacktrace.Frames[j].InstructionOffset.GetValueOrDefault());
                            stackFrameLineNumb.Add(sentryExceptions[i].Stacktrace.Frames[j].LineNumber.GetValueOrDefault());
                            stackFrameModule.Add(sentryExceptions[i].Stacktrace.Frames[j].Module);
                            stackFramePackage.Add(sentryExceptions[i].Stacktrace.Frames[j].Package);
                            framecnt++;
                        }
                    }
                    stackFrameTotalPerStack.Add(framecnt);
                }
                var tagsKeys = new List<string>(arg.Tags.Keys.ToList());
                var tagsValues = new List<string>();
                for (int i = 0; i < tagsKeys.Count(); i++)
                {
                    if (tagsKeys[i] == "error.handled")
                    {
                        unhandledError = arg.Tags[tagsKeys[i]] == "False";
                    }
                    tagsValues.Add(arg.Tags[tagsKeys[i]]);
                }
                var stack = arg.SentryExceptions?.First().Stacktrace;

                var extraKeys = new List<string>(arg.Extra.Keys.ToList());
                var extraValues = new List<string>();
                for (int i = 0; i < extraKeys.Count(); i++)
                {
                    extraValues.Add((string)arg.Extra[extraKeys[i]]);
                }
                extraKeys.Add("Original Time");
                extraValues.Add(DateTime.Now.ToString("dddd, MMM dd yyyy HH:mm:ss zzz"));

                //A última tag (que ficou em ordem invertida) sempre será a mensagem da viewmodel
                //as demais informações extras serão coletadas mediante o novo envio online
                string viewModelMessage;
                if (arg.Extra.ContainsKey("viewModelMessage"))
                {
                    viewModelMessage = arg.Extra["viewModelMessage"]?.ToString();
                }
                var user = arg.User;
                var cacheLog = new CacheLog()
                {
                    StackModule = StackModule,
                    StackStackTrace = StackStackTrace,
                    StackType = StackType,
                    StackValue = StackValue,
                    StackTrace = stack,
                    StackFrameContextLine = stackFrameContextLine,
                    StackFrameFileName = stackFrameFileName,
                    StackFrameFunction = stackFrameFunction,
                    StackFrameImgAddress = stackFrameImgAddress,
                    StackFrameInApp = stackFrameInApp,
                    StackFrameInstOffset = stackFrameInstOffset,
                    StackFrameLineNumb = stackFrameLineNumb,
                    StackFrameModule = stackFrameModule,
                    StackFramePackage = stackFramePackage,
                    StackFrameTotalPerStack = stackFrameTotalPerStack,
                    TagsKeys = tagsKeys,
                    TagsValues = tagsValues,
                    User = user,
                    ExtraKeys = extraKeys,
                    ExtraValues = extraValues,
                    Message = arg.Message,
                    Level = arg.Level,
                    Data = arg.Timestamp.ToString("dddd, MMM dd yyyy HH:mm:ss zzz"),

                };

                //Since SaveErrorCache is a heavy task, we run it in a new task in case the error was handled else
                //We need to run syncronously else the app will close before saving the error on disk.
                var saveTask = new Task(() =>
                {
                    _offlineCacheHelper.SaveCache(cacheLog, _key);
                });
                saveTask.Start();
                if (unhandledError)
                {
                    saveTask.Wait();
                }
            }

            public void Initialize()
            {
                //you must disable DisableAppDomainUnhandledExceptionCapture in sentry options before calling this function
                //we are dealing with the unhandled exception by ourselves

                SentrySdk.ConfigureScope(scope =>
                {
                    scope.Contexts.OperatingSystem.Name = _sentryXamarinHelper.GetOsName();
                    scope.Contexts.OperatingSystem.Version = _sentryXamarinHelper.GetDeviceInfo();
                    scope.SetTag("page_locale", CultureInfo.CurrentCulture.ToString());
                    scope.Contexts.Device.Family = _sentryXamarinHelper.GetDeviceManufacture();
                    scope.Contexts.Device.Model = _sentryXamarinHelper.GetDeviceModel();
                    scope.Contexts.Device.Simulator = _sentryXamarinHelper.IsEmulator();
                    scope.Contexts.Device.FreeStorage = _sentryXamarinHelper.GetAvaliableSpace();
                    scope.Contexts.Device.MemorySize = _sentryXamarinHelper.GetRamAmount();
                    scope.SetTag("device.hascamera", _sentryXamarinHelper.HasCamera().ToString());
                    scope.User.IpAddress = "{{auto}}";

                });
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }

            private void SendMessage(string message, Dictionary<string, string> ExtraTags = null, Dictionary<string, object> ExtraData = null, SentryLevel level = SentryLevel.Info)
            {
                try
                {

                    SentrySdk.WithScope(scope =>
                    {
                        scope.SetTags(ExtraTags);
                        scope.SetExtras(ExtraData);
                        SentrySdk.CaptureMessage(message, level);
                    });
                }
                catch { }
            }


            public void RegisterAction(string message, string actionType, bool report = false, ELogType type = ELogType.User, Dictionary<string, string> ExtraTags = null, Dictionary<string, object> ExtraData = null, short reportChance = 100)
            {
                SentrySdk.AddBreadcrumb(message, actionType, level: BreadcrumbLevelConverter(type));
                if (report && rand.Next(0, 100) <= reportChance)
                {
                    SendMessage(message, ExtraTags, ExtraData, SentryLevelConverter(type));
                }
            }

            public void RegisterNavigate(string from, string to, bool report = false, short reportChance = 100)
            {
                SentrySdk.AddBreadcrumb(null, "navigation", "navigation", new Dictionary<string, string>() { { "to", $"/{to}" }, { "from", $"/{from}" } });
                if (report && rand.Next(100) <= reportChance)
                {
                    SendMessage($"Navigate {to}", null);
                }
            }

            public void RegisterRequest(string request, string operation = "GET", bool report = false, short reportChance = 100)
            {
                SentrySdk.AddBreadcrumb(null, "app", "http",
                    new Dictionary<string, string>()
                    {
                    { "method", operation },
                    { "url", $"{request} " }
                    });
                if (report && rand.Next(0, 100) <= reportChance)
                {
                    SendMessage($"Request {operation} {request}", null);
                }
            }

            public void SendCachedLog()
            {
            var names = _offlineCacheHelper.GetFileNames();
            //TODO: FIX
            for (int i = 0; i < names.Count /*&& CrossConnectivity.Current.IsConnected*/; i++)
            {
                try
                {
                    var @event = _offlineCacheHelper.GetCache(names[i],_key);
                    if (@event != null)
                    {
                        //Not the best way of doing that because it ll mix with the previous scope
                        SentrySdk.WithScope(scope =>
                        {
                            scope.User = @event.User;
                            //TODO: save inside the event
//                            scope.Environment = EnvironmentConfig.AppEnvironment.ToString();

                            for (int j = 0; j < @event.TagsKeys.Count; j++)
                            {
                                scope.SetTag(@event.TagsKeys[j], @event.TagsValues[j]);
                            }
                            for (int j = 0; j < @event.ExtraKeys.Count; j++)
                            {
                                scope.SetExtra(@event.ExtraKeys[j], @event.ExtraValues[j]);
                            }
                            SentryEvent evento = new SentryEvent();

                            if (@event.StackModule != null)
                            {
                                List<SentryException> se = new List<SentryException>();
                                for (int j = 0, k = 0; j < @event.StackModule.Count; j++)
                                {
                                    se.Add(new SentryException()
                                    {
                                        Module = @event.StackModule[j],
                                        Type = @event.StackType[j],
                                        Value = @event.StackValue[j],
                                        ThreadId = 1
                                    });
                                    if (@event.StackFrameTotalPerStack.Count() > 0 && k < @event.StackFrameTotalPerStack[j])
                                    {
                                        var stackLast = se.Last(); //get reference
                                        stackLast.Stacktrace = new SentryStackTrace();
                                        var listaFrames = new List<SentryStackFrame>();
                                        while (k < @event.StackFrameTotalPerStack[j])
                                        {
                                            var frame = new SentryStackFrame()
                                            {
                                                ContextLine = @event.StackFrameContextLine[k],
                                                FileName = @event.StackFrameFileName[k],
                                                Function = @event.StackFrameFunction[k],
                                                ImageAddress = @event.StackFrameImgAddress[k],
                                                InApp = @event.StackFrameInApp[k],
                                                InstructionOffset = @event.StackFrameInstOffset[k],
                                                LineNumber = (int)@event.StackFrameLineNumb[k],
                                                Module = @event.StackFrameModule[k],
                                                Package = @event.StackFramePackage[k],
                                            };
                                            stackLast.Stacktrace.Frames.Add(frame);
                                            k++;
                                        }
                                    }

                                }
                                evento.SentryExceptions = se;
                            }
                            evento.Platform = "csharp";
                            evento.Level = @event.Level;
                            evento.Message = @event.Message;
                            //evento.Release = _deviceInfo.GetVersion();

                            SentrySdk.CaptureEvent(evento);
                        });
                        _offlineCacheHelper.RemoveLogCache(names[i]);
                    }
                }
                catch
                {
                    //o documento existe, mas houve algum problema no processamento dele, então exclua ele e faça
                    //o parse dos demais
                    _offlineCacheHelper.RemoveLogCache(names[i]);
                }
            }
        }

            public void SetTag(string tag, string value)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetTag(tag, value);
                });
            }

            public void UnsetTag(string tag)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.UnsetTag(tag);
                });
            }

            public void UnsetTags(List<string> tags)
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    foreach (string tag in tags)
                    {
                        scope.UnsetTag(tag);
                    }
                });
            }

            public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
            {
                SentrySdk.ConfigureScope(scope =>
                 scope.SetTag("Handled", "False")
                );
                SentrySdk.CaptureException((Exception)args.ExceptionObject);
                SentrySdk.FlushAsync(new TimeSpan(0, 1, 0)).Wait();// 1 minute to send or to make an offline backup
            }

            private BreadcrumbLevel BreadcrumbLevelConverter(ELogType logType)
            {
                switch (logType)
                {
                    case ELogType.Debug:
                        return BreadcrumbLevel.Debug;
                    case ELogType.System:
                    case ELogType.User:
                        return BreadcrumbLevel.Info;
                    case ELogType.Warning:
                        return BreadcrumbLevel.Warning;
                    case ELogType.Error:
                        return BreadcrumbLevel.Error;
                    default:
                        return BreadcrumbLevel.Critical;
                }
            }
            private SentryLevel SentryLevelConverter(ELogType logType)
            {
                switch (logType)
                {
                    case ELogType.Debug:
                        return SentryLevel.Debug;
                    case ELogType.System:
                    case ELogType.User:
                        return SentryLevel.Info;
                    case ELogType.Warning:
                        return SentryLevel.Warning;
                    case ELogType.Error:
                        return SentryLevel.Error;
                    default:
                        return SentryLevel.Fatal;
                }
            }

        public void SetEncryptionKey(string key)
        {
            _key = key;
        }
    }
}
