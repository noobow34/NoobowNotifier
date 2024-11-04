using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using NoobowNotifier.Constants;
using System;
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
            DateTime min = new(year, month, 1);
            DateTime max = new(year, month,DateTime.DaysInMonth(year,month),23,59,59);

            var listRequest = new EventsResource.ListRequest(service, GymRecordCalendar)
            {
                TimeMinDateTimeOffset = min,
                TimeMaxDateTimeOffset = max
            };

            int swimCount = 0;
            try
            {
                var eventList = listRequest.Execute();
                if (eventList.Items.Count > 0)
                {
                    var swimList = eventList.Items.Select((s,d) => (s.Summary,s.Start.Date)).ToList();
                    swimList.Sort((a, b) => string.Compare(a.Date, b.Date));
                    foreach (var e in swimList)
                    {
                        if (SwimMarkCountTargeet.Contains(e.Summary))
                        {
                            swimCount++;
                            result.Append(e.Date + "<br>");
                        }
                    }
                }
            }
            catch (Exception)
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

            try
            {
                string[] splitDate = recordDate.Split("-");
                int year = int.Parse(splitDate[0]);
                int month = int.Parse(splitDate[1]);
                DateTime min = new(year, month, 1);
                DateTime max = new(year, month, DateTime.DaysInMonth(year, month));

                var listRequest = new EventsResource.ListRequest(service, GymRecordCalendar)
                {
                    TimeMinDateTimeOffset = min,
                    TimeMaxDateTimeOffset = max
                };
                var eventList = listRequest.Execute();
                if(eventList.Items.Where(e => e.Start.Date == recordDate).Any())
                {
                    return 0;
                }
                swimCount = eventList.Items.Where(e => SwimMarkCountTargeet.Contains(e.Summary)).Count();

                Event swimReccord = new()
                {
                    Summary = SwimMark,
                    Start = new EventDateTime() { Date = recordDate },
                    End = new EventDateTime() { Date = recordDate },
                    Transparency = "transparent"
                };
                var recordRequest = new EventsResource.InsertRequest(service, swimReccord, GymRecordCalendar);
                recordRequest.Execute();
            }
            catch (Exception)
            {
                return -1;
            }

            return swimCount+1;
        }

    }

}