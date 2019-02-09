using jafleet.Commons.EF;
using NoobowNotifier.Constants;
using NoobowNotifier.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoobowNotifier.Logics
{
    public static class GALogics
    {
        private static GA ga = new GA(GAConstant.CERT_JSON_PATH, GAConstant.JAFLEET_ID);
        public static string GetReportStringMyNormal1(jafleetContext context)
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

            string today = DateTime.Now.Date.ToString("yyyyMMdd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            List<DailyStatistics> ds;

            ds = context.DailyStatistics.Where(q => q.LogDateYyyyMmDd == today || q.LogDateYyyyMmDd == yesterday).ToList();

            var dsToday = ds.Where(q => q.LogDateYyyyMmDd == today).FirstOrDefault();
            var dsYesterday = ds.Where(q => q.LogDateYyyyMmDd == yesterday).FirstOrDefault();

            return $"今日：u{todayUsers},s{todaySessions},pv{todayPageViews}\n" +
                $"　　　p{dsToday?.PhotoCount ?? 0},l{dsToday?.LineCount ?? 0},s{dsToday?.SearchCount ?? 0},e{dsToday?.ExCount ?? 0}\n" +
                $"昨日：u{yesterdayUsers},s{yesterdaySessions},pv{yesterdayPageViews}\n" +
                $"　　　p{dsYesterday?.PhotoCount ?? 0},l{dsYesterday?.LineCount ?? 0},s{dsYesterday?.SearchCount ?? 0},e{dsYesterday?.ExCount ?? 0}";
        }
    }
}
