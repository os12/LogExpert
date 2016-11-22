using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class GLogColumnizer : ILogLineColumnizer
    {
        protected int timeOffset = 0;

        // Message example:
        //
        //  0     1               2     3                           4
        //  I1012 10:22:10.499647 29992 agent_rpc_executor.cpp:244] Returned AgentInfo: agent_id=21317 cluster_id=4471129724439792
        Regex regex = new Regex(@"(\w\d\d\d\d)\s+([^\s]+)\s+(\d+)\s+([^:]+:\d+)\]\s+(.+)");

        public GLogColumnizer()
        {
        }

        public string GetName()
        {
            return "GLog";
        }

        public string GetDescription()
        {
            return "GLog format";
        }

        public int GetColumnCount()
        {
            return 5;
        }

        public string[] GetColumnNames()
        {
            return new string[] { "Severity", "Timestamp", "Thread", "Location", "Message" };
        }

        public bool IsTimeshiftImplemented()
        {
            return false;
        }

        public void SetTimeOffset(int msecOffset)
        {
            this.timeOffset = msecOffset;
        }

        public int GetTimeOffset()
        {
            return this.timeOffset;
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
        {
            string[] cols = SplitLine(callback, line);
            if (cols == null || cols.Length < 8)
                return DateTime.MinValue;

            if (cols[2].Length == 0)
                return DateTime.MinValue;

            try
            {
                DateTime dateTime = DateTime.ParseExact(cols[2], "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                return dateTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            if (column == 2)
            {
                try
                {
                    DateTime newDateTime = DateTime.ParseExact(value, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                    DateTime oldDateTime = DateTime.ParseExact(oldValue, "dd/MMM/yyyy:HH:mm:ss zzz", new CultureInfo("en-US"));
                    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.timeOffset = (int)(mSecsNew - mSecsOld);
                }
                catch (FormatException)
                {
                }
            }
        }

        public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
        {
            string[] cols = new string[5] { "", "", "", "", "" };

            // Message example:
            //
            //  0     1               2     3                           4
            //  I1012 10:22:10.499647 29992 agent_rpc_executor.cpp:244] Returned AgentInfo: agent_id=21317 cluster_id=4471129724439792

            Match match = this.regex.Match(line);
            if (!match.Success)
            {
                cols[4] = line;
                return cols;
            }

            for (int i = 0; i < 5; ++i)
                cols[i] = match.Groups[i + 1].Value;

            switch (match.Groups[1].Value[0])
            {
                case 'I':
                    cols[0] = "INFO";
                    break;
                case 'W':
                    cols[0] = "WARN";
                    break;
                case 'E':
                    cols[0] = "ERROR";
                    break;
                case 'F':
                    cols[0] = "FATAL";
                    break;
                default:
                    cols[0] += match.Groups[1].Value[0];
                    break;
            }

            string date = match.Groups[1].Value.Substring(1);
            string time = match.Groups[2].Value;

            try
            {
                DateTime date_time =
                    DateTime.ParseExact(date + " " + time, "MMdd HH:mm:ss.ffffff",
                                        new CultureInfo("en-US"));
                cols[1] =
                    date_time.ToString("dd/MMM HH:mm:ss.fff", new CultureInfo("en-US"));
            }
            catch (Exception)
            {
            }

            return cols;
        }

        public string Text
        {
            get { return GetName(); }
        }

    }
}
