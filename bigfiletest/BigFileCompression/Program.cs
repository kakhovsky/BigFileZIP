using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BigFileCompression
{
    class Program
    {

        static void Main(string[] args)
        {
            // Size of file to generate can be given as argument -s <s>, where s is size in megabytes
            // Additional feature is that if file size provided by argument, application will not wait for manual exit
            int fileSize = -1;
            bool argumentRun = false;
        

            if (args[0]?.ToString() == "-s" && args[1]?.Length > 0)
            {
                int.TryParse(args[1], out fileSize);
                argumentRun = true;
            }
            else
            {
                Console.Write("Please, enter size of file to create in megabytes (1-10000) : ");
                int.TryParse(Console.ReadLine(), out fileSize);
            }
            
            if(fileSize <= 0 || fileSize>15000) { Console.WriteLine("Entered data is not valid, press enter to exit"); Console.ReadLine(); Environment.Exit(-1); }

            // Main logic

            if (CreateBigFile(fileSize))
            {
                if (Compress())
                {
                    if (Decompress())
                    {
                        CompareFiles();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nDecompression failed");
                        Environment.Exit(90);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nCompression failed");
                    Environment.Exit(80);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nFile creation failed");
                Environment.Exit(70);
            }

            //Wait for return if no argument given
            if (!argumentRun)
            {
                Console.WriteLine("Press enter to exit");
                Console.ReadLine(); 
            }

        }

        private static void CompareFiles()
        {
            Console.WriteLine();
            var orgiginalHashArray = ComputeCrc("HugeFile.test");
            var decompressedHashArray = ComputeCrc("HugeFile.unpacked");


            string originalFileHash = System.Convert.ToBase64String(orgiginalHashArray);
            string decompressedFileHash = System.Convert.ToBase64String(decompressedHashArray);

            Console.WriteLine();

            Console.ForegroundColor=ConsoleColor.White;
            Console.WriteLine($"Compressed file checksum   : {originalFileHash}");
            Console.WriteLine($"Decompressed file checksum : {decompressedFileHash}");

            if (orgiginalHashArray.SequenceEqual<byte>(decompressedHashArray))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nChecksums of given files are identical. Files are the same.");
                Console.ForegroundColor = ConsoleColor.Gray;
                Environment.Exit(1000);
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCHECKSUMS ARE NOT THE SAME"); 
                Console.ForegroundColor = ConsoleColor.Gray;
                Environment.Exit(100);
            }
                
        }

        /// <summary>
        /// Function creates or rewrites a file filled with random data in local directory
        /// </summary>
        /// <param name="fileSize">Size of file in megabytes</param>
        /// <returns></returns>

        static bool CreateBigFile(int fileSize)
        {

            const int buferSize = 65536;

            byte[] buffer = new byte[buferSize];
            int kibCount = 0;
            int megaByteCoefficient = 1024 * 1024 / buferSize;

            Random rnd = new Random();

            Console.Write($"{System.DateTime.Now.TimeOfDay} Writing file  : ");

            try
            {

                using (Stream output = File.Create("HugeFile.test"))
                {

                    rnd.NextBytes(buffer);
                    int cursorPosition = Console.CursorLeft;

                    for (int i = 0; i < fileSize * megaByteCoefficient; i++)
                    {
                        kibCount++;
                        if (kibCount == megaByteCoefficient)
                        {

                            kibCount = 0;
                           
                            Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                            Console.Write($"{output.Position.ToString("N0")} bytes written");
                            
                        }

                        output.Write(buffer, 0, buferSize);
                      
                    }

                    output.Close();

                    var fileInfo = new FileInfo("HugeFile.test");

                   
                    Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{fileInfo.Length.ToString("N0")} bytes written");
                    Console.ForegroundColor = ConsoleColor.Gray;

                        // Console.WriteLine($"\n{ System.DateTime.Now.TimeOfDay} Created successfully HugeFie.test in local directory");
                    
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"File creation failed. \n\n{ex.Message}");
            }

            return false;

        }
        /// <summary>
        /// Compressing target file to a Gzip located in the same directory
        /// </summary>
        static bool Compress()
        {

            Console.Write($"\n{ System.DateTime.Now.TimeOfDay} Compressing   : ");
            bool processing = true;

            using (FileStream originalFileStream = File.OpenRead("HugeFile.test"))
            {

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken cancelToken = cancelTokenSource.Token;

                int cursorPosition = Console.CursorLeft;

                using (FileStream compressedFileStream = File.Create("HugeFile.gz"))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                       CompressionMode.Compress))
                    {

                        Task copy = originalFileStream.CopyToAsync(compressionStream, 81920, cancelToken);
                        Task copyCompleted = copy.ContinueWith(x =>
                        {
                            
                            processing = false;

                            
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);

                      
                        while (processing)
                        {

                            Console.SetCursorPosition(cursorPosition, Console.CursorTop); Console.Write($"{originalFileStream.Position.ToString("N0")} bytes compressed");

                            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                            {
                                cancelTokenSource.Cancel();
                                return false;
                            }
                            System.Threading.Thread.Sleep(500);
                        }

                        var fileInfo = new FileInfo("HugeFile.test");

                        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{fileInfo.Length.ToString("N0")} bytes compressed");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        return true;

                    }
                }

            }

          
        }

        static bool Decompress()
        {

            Console.Write($"\n{System.DateTime.Now.TimeOfDay} Decompressing : ");
            bool processing = true;

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;
            
            using (FileStream originalFileStream = File.Open("HugeFile.gz", FileMode.Open))
            {

                using (FileStream decompressedFileStream = File.Create("HugeFile.unpacked"))
                {
                    int cursorPosition = Console.CursorLeft;

                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        var copy = decompressionStream.CopyToAsync(decompressedFileStream);
                        var copyCompleted = copy.ContinueWith(x =>
                        {
                            processing = false;

                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                        

                        while (processing)
                        {
                            Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                           
                            Console.Write($"{originalFileStream.Position.ToString("N0")} bytes decompressed");
                            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                            {
                                cancelTokenSource.Cancel();
                                return false;
                            }
                            System.Threading.Thread.Sleep(500);
                        }

                        var fileInfo = new FileInfo("HugeFile.unpacked");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.SetCursorPosition(cursorPosition, Console.CursorTop);
                        Console.WriteLine($"{fileInfo.Length.ToString("N0")} decompressed successfully");
                        Console.ForegroundColor = ConsoleColor.Gray;

                        return true;

                    }
                }
            }

            
        }

        static byte[] ComputeCrc(string filename)
        {

            Console.WriteLine($"{ System.DateTime.Now.TimeOfDay} Calculating checksum for {filename}");

            try
            {
                using (FileStream filestream = File.Open(filename, FileMode.Open))
                {
                    System.Security.Cryptography.SHA256CryptoServiceProvider shaProvider = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                    byte[] result = shaProvider.ComputeHash(filestream);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{System.DateTime.Now.TimeOfDay.ToString()} {ex.Message}");
                return new byte[0];
            }


        }

    }
}
