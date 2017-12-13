using System;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.MarketMaker.Enums;
using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    public class TraceLog: ILog
    {
        private readonly string _component;

        public TraceLog()
        {
            _component = PlatformServices.Default.Application.ApplicationName;
        }

        private static Task Log(TraceLevelGroupEnum traceLevelGroup, string component, string process, string context, string info, DateTime? dateTime)
        {
            Trace.Write(traceLevelGroup, null, info, new
            {
                Component = component,
                Process = process,
                Context = context,
            });
            return Task.CompletedTask;
        }

        private static Task Log(TraceLevelGroupEnum traceLevelGroup, string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            Trace.Write(traceLevelGroup, null, exception.Message, new
            {
                Component = component,
                Process = process,
                Context = context,
                ExceptionData = exception.Data,
                Exception = exception.ToString(),
            });
            return Task.CompletedTask;
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Info, component, process, context, info, dateTime);
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Monitor, component, process, context, info, dateTime);
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Warn, component, process, context, info, dateTime);
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Error, component, process, context, exception, dateTime);
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Error, component, process, context, exception, dateTime);
        }

        public Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Info, _component, process, context, info, dateTime);
        }

        public Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Monitor, _component, process, context, info, dateTime);
        }

        public Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Warn, _component, process, context, info, dateTime);
        }

        public Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Error, _component, process, context, exception, dateTime);
        }

        public Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return Log(TraceLevelGroupEnum.Error, _component, process, context, exception, dateTime);
        }
    }
}