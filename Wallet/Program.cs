using System;
using System.Threading;

namespace Wallet {
    class Program {
        static void Main(string[] args) {
            Timer timerSyncBlock = new Timer(SyncBlock, null, 0, 15000);
            TranNEO tranNEO = new TranNEO();
            TranNNC tranNNC = new TranNNC();
            BalanceOfTest balanceOfTest = new BalanceOfTest();
            PushContract pushContract = new PushContract();

            ShowMenu();

            while(true) {
                string s = Console.ReadLine().ToLower();
                if(s == "1") {
                    tranNEO.Run();
                } else if(s == "2") {
                    tranNNC.Run();
                } else if(s == "3") {
                    balanceOfTest.Run();
                } else if(s == "4") {
                    pushContract.Run();
                } else if(s.ToLower() == "exit") {
                    return;
                } else if(s.ToLower() == "help") {
                    ShowMenu();
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
            Console.WriteLine("输入1:转账NEO");
            Console.WriteLine("输入2:转账NNC");
            Console.WriteLine("输入3:BalanceOfTest");
            Console.WriteLine("输入4:测试合约发布");
        }

    }
}
