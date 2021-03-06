﻿using System;
using System.Threading;

namespace server
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(4, 8888);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        if((nextLoop - DateTime.Now).CompareTo(new TimeSpan()) == 1 ) {
                            Thread.Sleep(nextLoop - DateTime.Now);
                        } else {
                            Console.Write($"\n{nextLoop - DateTime.Now}\n");
                            Console.Out.Flush();
                        }
                    }
                }
            }
        }
    }
}
