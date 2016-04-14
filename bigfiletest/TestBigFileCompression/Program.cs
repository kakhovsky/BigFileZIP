using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace TestBigFileCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            int fileSize=0;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"\n\nRunning on {Environment.OSVersion}");
            
            for (int pass = 1; pass < 4; pass++)
            {
              
                switch (pass)
                {
                    case 1: fileSize=5555; break;
                    case 2: fileSize = 8888; break;
                    case 3: fileSize = 11111; break;
                }

                Console.WriteLine($"\nPass {pass} with file size {fileSize}\n");

                using (Process testProgramm = new Process())
                {
                    testProgramm.StartInfo.FileName = "BigFileCompression.exe";
                    testProgramm.StartInfo.Arguments = $"-s {fileSize}";
                    testProgramm.StartInfo.UseShellExecute = false;
                    testProgramm.Start();

                    while (!testProgramm.HasExited)
                    {
                        System.Threading.Thread.Sleep(50);
                        
                    }
                }

            }
            Console.ReadLine();
        }


    }
}
