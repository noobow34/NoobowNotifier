using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using jafleet.Commons.EF;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using NoobowNotifier.Constants;
using NoobowNotifier.Services;
using NuGet.DependencyResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoobowNotifier.Logics
{
    public static class GCLogics
    {
        private static Services.GC gc = new Services.GC(GAConstant.CERT_JSON_PATH);
        const string GymRecordCalendar = "03b90723cb8ffd63806bb3dab57b315747d7094d59a0ab5e6140d65a92f3ee5b@group.calendar.google.com";
        const string SwimMark = "○";
        readonly static string[] SwimMarkCountTargeet = new string[] { "○", "◯" };


        public static string CountSwim(string countTarget)
        {
            StringBuilder result = new();

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = gc.GetCredential(),
                ApplicationName = "SwimRecord",
            });

            var splitYM = countTarget.Split("-");
            int year = int.Parse(splitYM[0]);
            int month = int.Parse(splitYM[1]);
            DateTime min = new DateTime(year, month, 1);
            DateTime max = new DateTime(year, month,DateTime.DaysInMonth(year,month));

            var listRequest = new EventsResource.ListRequest(service, GymRecordCalendar)
            {
                TimeMin = min,
                TimeMax = max
            };

            int swimCount = 0;
            try
            {
                var eventList = listRequest.Execute();
                if (eventList.Items.Count > 0)
                {
                    foreach (var e in eventList.Items)
                    {
                        if (SwimMarkCountTargeet.Contains(e.Summary))
                        {
                            swimCount++;
                            result.Append(e.Start.Date + "<br>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "検索失敗(;_;)";
            }
            return  $"{swimCount}回泳いだよ！<br>" + result.ToString();
        }
        public static int RecordSwim(string recordDate)
        {
            int swimCount = 0;
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = gc.GetCredential(),
                ApplicationName = "SwimRecord",
            });

            Event swimReccord = new()
            {
                Summary = SwimMark,
                Start = new EventDateTime() { Date = recordDate },
                End = new EventDateTime() { Date = recordDate },
                Transparency = "transparent"
            };
            var recordRequest = new EventsResource.InsertRequest(service,swimReccord,GymRecordCalendar);

            try
            {
                recordRequest.Execute();

                string[] splitDate = recordDate.Split("-");
                int year = int.Parse(splitDate[0]);
                int month = int.Parse(splitDate[1]);
                DateTime min = new DateTime(year, month, 1);
                DateTime max = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                var listRequest = new EventsResource.ListRequest(service, GymRecordCalendar)
                {
                    TimeMin = min,
                    TimeMax = max
                };

                var eventList = listRequest.Execute();
                if (eventList.Items.Count > 0)
                {
                    foreach (var e in eventList.Items)
                    {
                        if (SwimMarkCountTargeet.Contains(e.Summary))
                        {
                            swimCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }

            return swimCount;
        }

    }

}