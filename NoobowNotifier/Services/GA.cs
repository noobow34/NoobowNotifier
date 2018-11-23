using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NoobowNotifier.Services
{
    public class GA
    {
        GoogleCredential credential = null;
        string jsonPath = null;
        string viewId = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonPath">jsonファイルのパス</param>
        /// <param name="viewId">ビューID</param>
        public GA(string jsonPath,string viewId)
        {
            this.jsonPath = jsonPath;
            this.viewId = viewId;
            this.credential = GetCredential();
        }
        
        public IList<Report> GetReportMyNormal1()
        {
            var credential = GetCredential();
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            var service = new AnalyticsReportingService(new AnalyticsReportingService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "NoobowNotifier",
            });
            ;

            var request = new GetReportsRequest
            {
                ReportRequests = new[] {
                    new ReportRequest
                    {
                        ViewId = this.viewId,
                        Metrics = new[] { new Metric { Expression = "ga:users" } },
                        Dimensions = new[] { new Dimension { Name = "ga:date" } },
                        DateRanges = new[] { new DateRange { StartDate = yesterday, EndDate = today } },
                    },
                    new ReportRequest
                    {
                        ViewId = this.viewId,
                        Metrics = new[] { new Metric { Expression = "ga:sessions" } },
                        Dimensions = new[] { new Dimension { Name = "ga:date" } },
                        DateRanges = new[] { new DateRange { StartDate = yesterday, EndDate = today } },
                    },
                    new ReportRequest
                    {
                        ViewId = this.viewId,
                        Metrics = new[] { new Metric { Expression = "ga:pageviews" } },
                        Dimensions = new[] { new Dimension { Name = "ga:date" } },
                        DateRanges = new[] { new DateRange { StartDate = yesterday, EndDate = today } },
                    }
                }
            };

            var batchRequest = service.Reports.BatchGet(request);
            return  batchRequest.Execute().Reports;
        }

        private GoogleCredential GetCredential()
        {
            if(credential == null)
            {
                using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Google.Apis.AnalyticsReporting.v4.AnalyticsReportingService.Scope.AnalyticsReadonly);
                }
            }
            return credential;
        }
    }
}
