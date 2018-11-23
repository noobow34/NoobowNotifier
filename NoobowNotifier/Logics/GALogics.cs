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
            return $"今日：u{todayUsers},s{todaySessions},pv{todayPageViews}\n" +
                $"昨日：u{yesterdayUsers},s{yesterdaySessions},pv{yesterdayPageViews}";
        }
    }
}
