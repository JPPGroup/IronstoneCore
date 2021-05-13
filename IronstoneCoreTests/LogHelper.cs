using System;
using System.IO;

namespace Jpp.Ironstone.Core.Tests
{
    class LogHelper
    {
        public static string GetLogName()
        {
            DateTime time = DateTime.Now;
            return $"IronstoneLog{time.ToString("yyyyMMdd")}.txt";
        }

        public static TextReader GetLogReader()
        {
            var readStream = File.Open(LogHelper.GetLogName(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new StreamReader(readStream);
        }
    }
}
