using Sentry;
using SentryNetXamarinAddon.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentryNetXamarinAddon.Services.Interface
{
    public interface ISentryXamarinService
    {
        /// <summary>
        /// Should be called when the internet connection is back
        /// </summary>
        void SendCachedLog();

        /// <summary>
        /// Should be called in before send in case you dont have internet connection
        /// </summary>
        /// <param name="arg"></param>
        void BackupLog(SentryEvent arg);

        void SetEncryptionKey(string key);

        /// <summary>
        /// Register a navigation breadcrumb and a navigation event
        /// </summary>
        /// <param name="from">The actual page name</param>
        /// <param name="to">The page to be navigated name</param>
        /// <param name="report">False to log as a Breadcrumb, True to log as a Breadcrumb and an Event</param>
        /// <param name="reportChance">the chance to report the eventm 100 = Always, 0 = Never</param>
        void RegisterNavigate(string from, string to, bool report = false, short reportChance = 100);

        /// <summary>
        /// Register an action as a breadcrumb and as an event
        /// Normally used to register click events, warnings or some aditional debug info
        /// </summary>
        /// <param name="message">The message to be send</param>
        /// <param name="actionType">What resulted the action, ui.click, console,warning and exception normally</param>
        /// <param name="report">False to log as a Breadcrumb, True to log as a Breadcrumb and an Event</param>
        /// <param name="type">The type of the event</param>
        /// <param name="ExtraTags">Extra information as Tags used for metrics and analytics. It'll be sent in case report = True</param>
        /// <param name="ExtraData">Extra information as Data that will not be used for metrics and analytics. It'll be sent in case report = True</param>
        /// <param name="reportChance">the chance to report the eventm 100 = Always, 0 = Never</param>
        void RegisterAction(string message, string actionType, bool report = false, ELogType type = ELogType.User, Dictionary<string, string> ExtraTags = null, Dictionary<string, object> ExtraData = null, short reportChance = 100);

        /// <summary>
        /// Register an event that involves data manipulation with the business logic
        /// </summary>
        /// <param name="request">A url or the name of the  request</param>
        /// <param name="operation">the http type (get,post,...) or the name that you prefer to use</param>
        /// <param name="report">False to log as a Breadcrumb, True to log as a Breadcrumb and an Event</param>
        /// <param name="reportChance">the chance to report the eventm 100 = Always, 0 = Never</param>
        void RegisterRequest(string request, string operation = "GET", bool report = false, short reportChance = 100);

        //Set Tags in the global scope
        void SetTag(string tag, string value);
        //Removes a Tag in the global scope
        void UnsetTag(string tag);
        //Removes multiple Tags in the global scope
        void UnsetTags(List<string> tags);

        void Initialize();
    }
}
