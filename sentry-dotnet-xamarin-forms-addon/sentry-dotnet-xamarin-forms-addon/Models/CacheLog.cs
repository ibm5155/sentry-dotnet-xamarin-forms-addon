using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentryNetXamarinAddon.Models
{
    public class CacheLog
    {
        public List<string> StackModule { get; set; }
        public List<string> StackType { get; set; }
        public List<string> StackValue { get; set; }
        public List<string> StackStackTrace { get; set; }

        /*Store the stack frames*/
        public List<string> StackFrameContextLine { get; set; }
        public List<string> StackFrameFileName { get; set; }
        public List<string> StackFrameFunction { get; set; }
        public List<long> StackFrameImgAddress { get; set; }
        public List<bool> StackFrameInApp { get; set; }
        public List<long> StackFrameInstOffset { get; set; }
        public List<long> StackFrameLineNumb { get; set; }
        public List<string> StackFrameModule { get; set; }
        public List<string> StackFramePackage { get; set; }
        public List<int> StackFrameTotalPerStack { get; set; }
        public SentryStackTrace StackTrace { get; set; }
        public List<string> TagsKeys { get; set; }
        public List<string> TagsValues { get; set; }
        public User User { get; set; }
        public List<string> ExtraKeys { get; set; }
        public List<string> ExtraValues { get; set; }
        public SentryLevel? Level { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
