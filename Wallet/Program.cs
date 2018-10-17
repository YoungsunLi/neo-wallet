using System;
using System.Threading;

namespace Wallet {
    class Program {
        static void Main(string[] args) {
            //Timer t = new Timer(SyncBlock, null, 0, 1500);
            SyncBlock syncBlock = new SyncBlock();
            while(true) {
                if(State.SyncBlock) {
                    ThreadPool.QueueUserWorkItem((o) => { syncBlock.Run(); });
                }
                Thread.Sleep(15000);
            }
        }

        //private static void SyncBlock(object state) {
        //    Console.WriteLine(DateTime.Now);
        //}

    }
}
