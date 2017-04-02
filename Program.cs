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
                blobContainers[i].CreateIfNotExistsAsync().Wait();
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

        static void Main(string[] args)
        {

            CloudBlobContainer[] containers = GetRandomContainers();

            // path to the directory to upload
            string path = "test";
            if (args.Length > 0)
            {
                path = System.Convert.ToString(args[0]);
            }

            Stopwatch time = Stopwatch.StartNew();
            try
            {

                Console.WriteLine("iterating in directiory:", path);

                // Seed the Random value using the Ticks representing current time and date
                // Since int is used as seen we cast (loss of long data)


                int count = 0;
                int max_outstanding = 100;
                int completed_count = 0;
                Semaphore sem = new Semaphore(max_outstanding, max_outstanding);

                List<Task> Tasks = new List<Task>();

                foreach (string fileName in Directory.GetFiles(path))
                {
                    Console.WriteLine("Starting {0}", fileName);
                    var container = containers[count % 8];
                    Random r = new Random((int)DateTime.Now.Ticks);
                    String s = (r.Next() % 10000).ToString("X5");
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(s);
					blockBlob.StreamWriteSizeInBytes = 100 * 1024 * 1024;
                    sem.WaitOne();
                    Tasks.Add(blockBlob.UploadFromFileAsync(fileName, null, new BlobRequestOptions() { ParallelOperationThreadCount = 8, DisableContentMD5Validation = true, StoreBlobContentMD5 = false }, null).ContinueWith((t) => {
                        sem.Release();
                        Interlocked.Increment(ref completed_count);
                    }));
                    count++;
                }

                Task.WhenAll(Tasks).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            time.Stop();

            Console.WriteLine("Upload has been completed in {0} seconds.", time.Elapsed.TotalSeconds.ToString());

            Console.ReadLine();


        }
    }
}
