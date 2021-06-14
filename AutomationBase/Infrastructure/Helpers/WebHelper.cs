using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutomationBase.Infrastructure.Helpers
{
    public static class WebHelper
    {
        public static async Task DownloadFile(string fileUri, string downloadPath)
        {
            using var httpClient = new HttpClient();
            using var stream = httpClient.GetStreamAsync(fileUri).Result;
            using var outputStream = new FileStream(downloadPath, FileMode.Create);
            
            await stream.CopyToAsync(outputStream);
        }
    }
}
