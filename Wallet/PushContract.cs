using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using ThinNeo;

namespace Wallet {
    class PushContract {
        public void Run() {
            string wif = "KwwJMvfFPcRx2HSgQRPviLv4wPrxRaLk7kfQntkH8kCXzTgAts8t";//自己
            string asset = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";//币种(GAS)

            byte[] prikey = Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = Helper.GetAddressFromPublicKey(pubkey);

            SQLServer sQLServer = new SQLServer();
            sQLServer.Open();
            //接收一个读取对象
            SqlDataReader reader = sQLServer.GetUTXO(address);

            //整理utxo
            Dictionary<string, List<UTXO>> dic_UTXO = GetUTXO(reader);
            sQLServer.Close();

            //从文件中读取合约脚本
            byte[] script = System.IO.File.ReadAllBytes("C:\\Neo\\SmartContracts\\0x35eac9327df0a34f2302a1c7832d888b6a366c0e.avm"); //这里填你的合约所在地址
            byte[] parameter__list = Helper.HexString2Bytes("0710");  //这里填合约入参  例：0610代表（string，[]）
            byte[] return_type = Helper.HexString2Bytes("05");  //这里填合约的出参
            int need_storage = 1;
            int need_nep4 = 0;
            int need_canCharge = 4;
            string name = "NEO Name Credit(CLI)";
            string version = "2.0";
            string auther = "Youngsun";
            string email = "lsun@live.cn";
            string description = "0";
            using(ScriptBuilder sb = new ScriptBuilder()) {
                //倒叙插入数据
                sb.EmitPushString(description);
                sb.EmitPushString(email);
                sb.EmitPushString(auther);
                sb.EmitPushString(version);
                sb.EmitPushString(name);
                sb.EmitPushNumber(need_storage | need_nep4 | need_canCharge);
                sb.EmitPushBytes(return_type);
                sb.EmitPushBytes(parameter__list);
                sb.EmitPushBytes(script);
                sb.EmitSysCall("Neo.Contract.Create");

                string scriptPublish = Helper.Bytes2HexString(sb.ToArray());
                //用invokescript试运行并得到消耗
                HttpRequest httpRequest = new HttpRequest();
                JObject result = httpRequest.Get("invokescript", scriptPublish);
                string consume = result["result"]["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                InvokeTransData extdata = new InvokeTransData();
                extdata.script = sb.ToArray();
                
                extdata.gas = Math.Ceiling(gas_consumed - 10);

                //拼装交易体
                Transaction tran = MakeTransaction(dic_UTXO, null, new Hash256(asset), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = Helper.Bytes2HexString(data);
                
                //广播
                JObject jObject = httpRequest.Post("sendrawtransaction", rawdata);

                string info = jObject.ToString();
                Console.WriteLine(info);
            }
        }

        Transaction MakeTransaction(Dictionary<string, List<UTXO>> dir_utxos, string targetaddr, Hash256 assetid, decimal sendcount) {
            if(!dir_utxos.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<UTXO> utxos = dir_utxos[assetid.ToString()];
            var tran = new Transaction();
            tran.type = TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            var scraddr = "";
            utxos.Sort((a, b) => {
                if(a.value > b.value)
                    return 1;
                else if(a.value < b.value)
                    return -1;
                else
                    return 0;
            });
            decimal count = decimal.Zero;
            List<TransactionInput> list_inputs = new List<TransactionInput>();
            for(var i = 0; i < utxos.Count; i++) {
                TransactionInput input = new TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].address;
                if(count >= sendcount) {
                    break;
                }
            }
            tran.inputs = list_inputs.ToArray();
            if(count >= sendcount)//输入大于等于输出
            {
                List<TransactionOutput> list_outputs = new List<TransactionOutput>();
                //输出
                if(sendcount > decimal.Zero && targetaddr != null) {
                    TransactionOutput output = new TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendcount;
                    output.toAddress = Helper.GetPublicKeyHashFromAddress(targetaddr);
                    list_outputs.Add(output);
                }

                //找零
                var change = count - sendcount;
                if(change > decimal.Zero) {
                    TransactionOutput outputchange = new TransactionOutput();
                    outputchange.toAddress = Helper.GetPublicKeyHashFromAddress(scraddr);
                    outputchange.value = change;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);

                }
                tran.outputs = list_outputs.ToArray();
            } else {
                throw new Exception("no enough money.");
            }
            return tran;
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
