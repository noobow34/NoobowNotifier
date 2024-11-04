using Google.Apis.Auth.OAuth2;
using System.IO;

namespace NoobowNotifier.Services
{
    public class GC
    {
        GoogleCredential credential = null;
        string jsonPath = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonPath">jsonファイルのパス</param>
        public GC(string jsonPath)
        {
            this.jsonPath = jsonPath;
        }
        

        public GoogleCredential GetCredential()
        {
            if(credential == null)
            {
                using var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
                credential = GoogleCredential.FromStream(stream).CreateScoped(Google.Apis.Calendar.v3.CalendarService.Scope.Calendar);
            }
            return credential;
        }
    }
}
