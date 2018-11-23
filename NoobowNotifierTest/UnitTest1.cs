using Google.Apis.AnalyticsReporting.v4.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoobowNotifier.Constants;
using NoobowNotifier.Logics;
using NoobowNotifier.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoobowNotifierTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GATest1()
        {
            GA ga = new GA(@"C:\pgroot\NoobowNotifier\keys\noobow-ga.json", GAConstant.JAFLEET_ID);
            var resutl1 = ga.GetReportMyNormal1();

            foreach (Report report in resutl1)
            {
                ColumnHeader header = report.ColumnHeader;
                List<string> dimensionHeaders = (List<string>)header.Dimensions;

                List<MetricHeaderEntry> metricHeaders = (List<MetricHeaderEntry>)header.MetricHeader.MetricHeaderEntries;
                List<ReportRow> rows = (List<ReportRow>)report.Data.Rows;

                foreach (ReportRow row in rows)
                {
                    List<string> dimensions = (List<string>)row.Dimensions;
                    List<DateRangeValues> metrics = (List<DateRangeValues>)row.Metrics;

                    for (int i = 0; i < dimensionHeaders.Count() && i < dimensions.Count(); i++)
                    {
                        Debug.WriteLine(dimensionHeaders[i] + ": " + dimensions[i]);
                    }

                    for (int j = 0; j < metrics.Count(); j++)
                    {
                        Debug.WriteLine("Date Range (" + j + "): ");
                        DateRangeValues values = metrics[j];
                        for (int k = 0; k < values.Values.Count() && k < metricHeaders.Count(); k++)
                        {
                            Debug.WriteLine(metricHeaders[k].Name + ": " + values.Values[k]);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void JPTestAsync()
        {
            JPLogics.GetJetPhotosFromRegistrationNumberAsync("JA801A");
        }

    }
}
