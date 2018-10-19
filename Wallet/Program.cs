using System;
using System.Threading;

namespace Wallet {
    class Program {
        static void Main(string[] args) {
            Timer timerSyncBlock = new Timer(SyncBlock, null, 0, 15000);
            Tran1 tran1 = new Tran1();
            ShowMenu();

            while(true) {
                string s = Console.ReadLine().ToLower();
                if(s == "tran1") {
                    tran1.Run();
                }
                Console.WriteLine(DateTime.Now);
                Thread.Sleep(200);
            }
        }

        private static void SyncBlock(object state) {
            SyncBlock syncBlock = new SyncBlock();
            if(State.SyncBlock) {
                syncBlock.Run();
            }
        }

        private static void ShowMenu() {
            Console.WriteLine("输入 tran1");
        }

    }
}
