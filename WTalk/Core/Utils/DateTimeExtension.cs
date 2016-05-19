using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Core.Utils
{
    public static class DateTimeExtension
    {
        //public static TimeSpan TimeIntervalSince1970(this DateTime date)
        //{
        //    TimeSpan t = date - new DateTime(1970, 1, 1);
        //    return t;
        //}

        //public static DateTime FromMillisecondsSince1970(this DateTime date, double milliseconds)
        //{
        //    DateTimeOffset.
        //    date = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(milliseconds);
        //    return date;
        //}

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime/1000);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }
    }
}
