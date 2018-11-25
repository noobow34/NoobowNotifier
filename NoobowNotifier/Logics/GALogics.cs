using jafleet.Constants;
using jafleet.EF;
using NoobowNotifier.Constants;
using NoobowNotifier.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Logics
{
    public class GALogics
    {
        public static string GetReportStringMyNormal1()
        {
            var ga = new GA(GAConstant.CERT_JSON_PATH, GAConstant.JAFLEET_ID);
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

            string todaySqlite = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterdaySqlite = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            int todayPhoto = 0, todayLine = 0, todaySearch = 0, todayEx = 0;
            int yesterdayPhoto = 0, yesterdayLine = 0, yesterdaySearch = 0, yesterdayEx = 0;

            using (var context = new jafleetContext())
            {
                todayPhoto = context.Log.Where(q => q.LogDate.StartsWith(todaySqlite)).Where(q => q.LogType == LogType.PHOTO).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todayLine = context.Log.Where(q => q.LogDate.StartsWith(todaySqlite)).Where(q => q.LogType == LogType.LINE).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todaySearch = context.Log.Where(q => q.LogDate.StartsWith(todaySqlite)).Where(q => q.LogType == LogType.SEARCH).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                todayEx = context.Log.Where(q => q.LogDate.StartsWith(todaySqlite)).Where(q => q.LogType == LogType.EXCEPTION).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayPhoto = context.Log.Where(q => q.LogDate.StartsWith(yesterdaySqlite)).Where(q => q.LogType == LogType.PHOTO).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayLine = context.Log.Where(q => q.LogDate.StartsWith(yesterdaySqlite)).Where(q => q.LogType == LogType.LINE).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdaySearch = context.Log.Where(q => q.LogDate.StartsWith(yesterdaySqlite)).Where(q => q.LogType == LogType.SEARCH).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
                yesterdayEx = context.Log.Where(q => q.LogDate.StartsWith(yesterdaySqlite)).Where(q => q.LogType == LogType.EXCEPTION).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).Count();
            }
            return $"今日：u{todayUsers},s{todaySessions},pv{todayPageViews}\n" +
                $"photo{todayPhoto},line{todayLine},search{todaySearch},ex{todayEx}\n" +
                $"昨日：u{yesterdayUsers},s{yesterdaySessions},pv{yesterdayPageViews}\n" +
                $"photo{yesterdayPhoto},line{yesterdayLine},search{yesterdaySearch},ex{yesterdayEx}";
        }
    }
}
