using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using NoobowNotifier.Constants;
using NoobowNotifier.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Logics
{
    public static class GALogics
    {
        private static GA ga = new GA(GAConstant.CERT_JSON_PATH, GAConstant.JAFLEET_ID);
        public static string GetReportStringMyNormal1()
        {
            var garesults = ga.GetReportMyNormal1();

            int todayUsers=0, todaySessions=0, todayPageViews = 0;
            int yesterdayUsers=0, yesterdaySessions=0, yesterdayPageViews=0;

            foreach (var result in garesults)
            {
                string metricsName = result.ColumnHeader.MetricHeader.MetricHeaderEntries?[0].Name;
                foreach (var row in result.Data.Rows)
                {
                    if (row.Dimensions?[0] == DateTime.Now.ToString("yyyyMMdd"))
                    {
                        switch (metricsName)
                        {
                            case "ga:users":
                                todayUsers = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            case "ga:sessions":
                                todaySessions = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            case "ga:pageviews":
                                todayPageViews = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (row.Dimensions?[0] == DateTime.Now.AddDays(-1).ToString("yyyyMMdd"))
                    {
                        switch (metricsName)
                        {
                            case "ga:users":
                                yesterdayUsers = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            case "ga:sessions":
                                yesterdaySessions = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            case "ga:pageviews":
                                yesterdayPageViews = Int32.Parse(row.Metrics[0].Values[0]);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            DateTime today = DateTime.Now.Date;
            DateTime tomorrow = today.AddDays(1);
            DateTime yesterday = today.AddDays(-1);
            string yesterdaySqlite = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            int todayPhoto = 0, todayLine = 0, todaySearch = 0, todayEx = 0;
            int yesterdayPhoto = 0, yesterdayLine = 0, yesterdaySearch = 0, yesterdayEx = 0;

            using (var context = new jafleetContext())
            {
                todayPhoto = context.Log.Where(q => q.LogDate >= today && q.LogDate < tomorrow && q.LogType == LogType.PHOTO && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todayLine = context.Log.Where(q => q.LogDate >= today && q.LogDate < tomorrow && q.LogType == LogType.LINE &&  q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todaySearch = context.Log.Where(q => q.LogDate >= today && q.LogDate < tomorrow && q.LogType == LogType.SEARCH && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todayEx = context.Log.Where(q => q.LogDate >= today && q.LogDate < tomorrow && q.LogType == LogType.EXCEPTION && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayPhoto = context.Log.Where(q => q.LogDate >= yesterday && q.LogDate < today && q.LogType == LogType.PHOTO && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayLine = context.Log.Where(q => q.LogDate >= yesterday && q.LogDate < today && q.LogType == LogType.LINE && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdaySearch = context.Log.Where(q => q.LogDate >= yesterday && q.LogDate < today && q.LogType == LogType.SEARCH && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayEx = context.Log.Where(q => q.LogDate >= yesterday && q.LogDate < today && q.LogType == LogType.EXCEPTION && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
            }
            return $"今日：u{todayUsers},s{todaySessions},pv{todayPageViews}\n" +
                $"　　　p{todayPhoto},l{todayLine},s{todaySearch},e{todayEx}\n" +
                $"昨日：u{yesterdayUsers},s{yesterdaySessions},pv{yesterdayPageViews}\n" +
                $"　　　p{yesterdayPhoto},l{yesterdayLine},s{yesterdaySearch},e{yesterdayEx}";
        }
    }
}
