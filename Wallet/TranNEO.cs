using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using ThinNeo;

namespace Wallet {
    class TranNEO {
        public void Run() {
            string wif = "KwwJMvfFPcRx2HSgQRPviLv4wPrxRaLk7kfQntkH8kCXzTgAts8t";//自己
            string targetAddress = "AQye22dcXV1jCrzzC4iGbyM68LADwPSs11";//别人
            string asset = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";//币种(NEO)
            //string asset = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";//币种(GAS)
            decimal sendCount = new decimal(1);

            byte[] prikey = Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = Helper.GetAddressFromPublicKey(pubkey);

            SQLServer sQLServer = new SQLServer();
            sQLServer.Open();
            //接收一个读取对象
            SqlDataReader reader = sQLServer.GetUTXO(address, asset);

            //整理utxo
            Dictionary<string, List<UTXO>> dic_UTXO = GetUTXO(reader);
            sQLServer.Close();

            //拼交易
            Transaction transaction = MakeTransaction(dic_UTXO, address, targetAddress, new Hash256(asset), sendCount);

            transaction.version = 0;
            transaction.attributes =new ThinNeo.Attribute[0];
            transaction.type = TransactionType.ContractTransaction;//转账类型
            byte[] msg = transaction.GetMessage();
            string msgStr = Helper.Bytes2HexString(msg);
            byte[] signdata = Helper.Sign(msg, prikey);//签名
            transaction.AddWitness(signdata, pubkey, address);
            string txid = transaction.GetHash().ToString();
            byte[] data = transaction.GetRawData();
            string rawdata = Helper.Bytes2HexString(data);

            //广播
            HttpRequest httpRequest = new HttpRequest();
            string url = "http://127.0.0.1:20337/?jsonrpc=2.0&id=1&method=sendrawtransaction&params=[\"" + rawdata + "\"]";
            JObject jObject = httpRequest.Get(url);
            string info = jObject.ToString();
            Console.WriteLine(info);
        }

        private Transaction MakeTransaction(Dictionary<string, List<UTXO>> dic_UTXO, string fromAddress, string targetAddress, Hash256 asset, decimal sendCount) {
            //从字典取出utxo列表
            List<UTXO> uTXOs = dic_UTXO[asset.ToString()];

            Transaction transaction = new Transaction();

            decimal count = decimal.Zero;
            List<TransactionInput> transactionInputs = new List<TransactionInput>();
            for(int i = 0; i < uTXOs.Count; i++) {
                TransactionInput transactionInput = new TransactionInput();
                transactionInput.hash = uTXOs[i].txid;
                transactionInput.index = (ushort)uTXOs[i].n;

                transactionInputs.Add(transactionInput);
                count += uTXOs[i].value;
                if(count >= sendCount) {
                    break;
                }
            }

            transaction.inputs = transactionInputs.ToArray();

            //输入大于等于输出
            if(count >= sendCount) {
                List<TransactionOutput> transactionOutputs = new List<TransactionOutput>();
                //输出
                if(sendCount > decimal.Zero) {
                    TransactionOutput transactionOutput = new TransactionOutput();
                    transactionOutput.assetId = asset;
                    transactionOutput.value = sendCount;
                    transactionOutput.toAddress = Helper.GetPublicKeyHashFromAddress(targetAddress);
                    transactionOutputs.Add(transactionOutput);
                }

                //找零
                decimal change = count - sendCount;
                if(change > decimal.Zero) {
                    TransactionOutput transactionOutput = new TransactionOutput();
                    transactionOutput.toAddress = Helper.GetPublicKeyHashFromAddress(fromAddress);
                    transactionOutput.value = change;
                    transactionOutput.assetId = asset;
                    transactionOutputs.Add(transactionOutput);
                }
                transaction.outputs = transactionOutputs.ToArray();
            } else {
                throw new Exception("余额不足!");
            }
            return transaction;
        }

        private Dictionary<string, List<UTXO>> GetUTXO(SqlDataReader reader) {
            //建一个以asset为key,utxo对象为value的字典
            Dictionary<string, List<UTXO>> dic = new Dictionary<string, List<UTXO>>();

            //读取reader并写入字典
            while(reader.Read()) {
                Hash256 txid = new Hash256(reader["txid"].ToString());
                int n = int.Parse(reader["n"].ToString());
                string asset = reader["asset"].ToString();
                string address = reader["address"].ToString();
                decimal value = decimal.Parse(reader["value"].ToString());

                UTXO uTXO = new UTXO(txid, n, asset, address, value);

                if(dic.ContainsKey(asset)) {
                    dic[asset].Add(uTXO);
                } else {
                    List<UTXO> uTXOs = new List<UTXO>();
                    uTXOs.Add(uTXO);
                    dic[asset] = uTXOs;
                }
            }
            if(dic.Count == 0) {
                throw new Exception("你都没有这种钱");//须添加币种显示
            }
            return dic;
        }
    }
}
