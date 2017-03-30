namespace AzPerf
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using System.Threading.Tasks;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        public static CloudBlobContainer[] GetRandomContainers()
        {
            string connectionString = "";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer[] blobContainers = new CloudBlobContainer[8];
            for (int i = 0; i < blobContainers.Length; i++)
            {
                blobContainers[i] = blobClient.GetContainerReference(GenerateString(5, new Random((int)DateTime.Now.Ticks), LowerCaseAlphabet));
                Console.WriteLine(blobContainers[i].Uri);
                blobContainers[i].CreateIfNotExistsAsync();
            }
            return blobContainers;
        }

        public const string LowerCaseAlphabet = "abcdefghijklmnopqrstuvwyxz";
        public static string GenerateString(int size, Random rng, string alphabet)
        {
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = alphabet[rng.Next(alphabet.Length)];
            }
            return new string(chars);
        }

        static async void MainAsync(string path, CloudBlobContainer[] containers)
        {
            
            try { 
            double runMinutes = 0;
            double progressReportMinutes = 0;
            progressReportMinutes = 1;

            Console.WriteLine("iterating in directiory:", path);

            // Seed the Random value using the Ticks representing current time and date
            // Since int is used as seen we cast (loss of long data)
            Random r = new Random((int)DateTime.Now.Ticks);

            String s = (r.Next() % 1000).ToString("X3");



            int count = 0;
            List<Task> Tasks = new List<Task>();

            foreach (string fileName in Directory.GetFiles(path))
            {
                Console.WriteLine("Starting {0}", fileName);
                var container = containers[count % 8];
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(s);
                Tasks.Add(Task.Run(() => blockBlob.UploadFromFileAsync(fileName, null, new BlobRequestOptions(){ ParallelOperationThreadCount=8},null)));
                count++;
            }

           //StartAndWaitAllThrottled(Tasks, 8);

           Task.WhenAll(Tasks).Wait();

            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void StartAndWaitAllThrottled(List<Task> tasksToRun, int maxTasksToRunInParallel, CancellationToken cancellationToken = new CancellationToken())
        {
            StartAndWaitAllThrottled(tasksToRun, maxTasksToRunInParallel, -1, cancellationToken);
        }
        public static void StartAndWaitAllThrottled(List<Task> tasks, int maxTasksToRunInParallel, int timeoutInMilliseconds, CancellationToken cancellationToken = new CancellationToken())
        {
            using (var throttler = new SemaphoreSlim(maxTasksToRunInParallel))
            {
                var postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                tasks.ForEach(t => postTaskTasks.Add(t.ContinueWith(tsk => throttler.Release())));

                // Start running each task.
                foreach (var task in tasks)
                {
                    // Increment the number of tasks currently running and wait if too many are running.
                    throttler.Wait(timeoutInMilliseconds, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                    task.Start();
                }

                // Wait for all of the provided tasks to complete.
                // We wait on the list of "post" tasks instead of the original tasks, otherwise there is a potential race condition where the throttler&#39;s using block is exited before some Tasks have had their "post" action completed, which references the throttler, resulting in an exception due to accessing a disposed object.
                Task.WaitAll(postTaskTasks.ToArray(), cancellationToken);
            }
        }
        static void Main(string[] args)
        {

            CloudBlobContainer[] containers = GetRandomContainers();

            // path to the directory to upload
            string path = "./";
            if (args.Length > 0)
            {
                path = System.Convert.ToString(args[0]);
            }

            Stopwatch s = Stopwatch.StartNew();
            MainAsync(path, containers);
            s.Stop();

            Console.WriteLine("Upload has been completed in {0} seconds.", s.Elapsed.TotalSeconds.ToString());

            Console.ReadLine();


        }
    }
}
