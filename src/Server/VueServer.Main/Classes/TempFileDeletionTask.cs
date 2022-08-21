using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VueServer.Classes
{
    public class TempFileDeletionTask : IScheduledTask
    {
        public string Schedule => "* */6 * * *";

        private string TempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tmp");

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            DirectoryInfo di = new DirectoryInfo(TempPath);
            var files = di.GetFiles();

            foreach (var file in files)
            {
                await file.DeleteAsync();
            }
        }
    }

    static internal class FileExtensions
    {
        public static Task DeleteAsync(this FileInfo fi)
        {
            Task task = null;

            try
            {
                task = Task.Factory.StartNew(() => fi.Delete());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while deleting, in cron task");
                Console.WriteLine(e.StackTrace);
            }

            return task;
        }
    }
}
