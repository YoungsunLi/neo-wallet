using System;
using Newtonsoft.Json.Linq;

namespace Wallet {
    class SyncBlock {

        public void Run() {
            //记录状态
            State.SyncBlock = false;

            long startBlock = GetStartBlock();
            long endBlock = GetEndBlock();
            SQLServer sQLServer = new SQLServer();
            sQLServer.Open();

            //同步主循环
            for(long blockNum = startBlock; blockNum < endBlock; blockNum++) {
                //获取块信息
                JObject blockJson = GetBlock(blockNum);
                //遍历tx 处理vin vout
                foreach(JObject tx in blockJson["result"]["tx"]) {
                    //接收index
                    string blockindex = blockJson["result"]["index"].Value<string>();
                    //接收txid
                    string txid = tx["txid"].Value<string>();
                    //处理vin
                    foreach(JObject vin in tx["vin"]) {
                        string txidRef = vin["txid"].Value<string>();
                        int nRef = vin["vout"].Value<int>();
                        //入库
                        sQLServer.AddVin(blockindex, txidRef, nRef);
                    }

                    //处理vout
                    foreach(JObject vout in tx["vout"]) {
                        int n = vout["n"].Value<int>();
                        string asset = vout["asset"].Value<string>();
                        string address = vout["address"].Value<string>();
                        string value = vout["value"].Value<string>();
                        //入库
                        sQLServer.AddVout(blockindex, txid, n, asset, address, value);
                    }
                }
                //记录更新
                sQLServer.UpdateBlockHeight(blockNum + 1);
            }
            sQLServer.Close();
            //记录状态
            State.SyncBlock = true;
        }
        HttpRequest httpRequest = new HttpRequest();
        //获取块信息
        private JObject GetBlock(long blockNum) {
            JObject blockJson = httpRequest.GetBlock(blockNum);
            Console.WriteLine("正在处理的块= " + blockNum);
            return blockJson;
        }

        //获取起始块
        private long GetStartBlock() {
            SQLServer sQLServer = new SQLServer();
            sQLServer.Open();
            long startBlock = sQLServer.GetStartBlock();
            sQLServer.Close();
            return startBlock;
        }

        //获取最高块
        private long GetEndBlock() {
            JObject json = httpRequest.Get("getblockcount", "");
            long endBlock = json["result"].Value<long>();
            return endBlock;
        }
    }
}
