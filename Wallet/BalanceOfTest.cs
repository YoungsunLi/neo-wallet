using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;

namespace Wallet {
    class BalanceOfTest {
        public void Run() {
            string address = "AN6HX6NxNsQaLdcbtqjTCP2z4XxTy1GNSr";
            byte[] data = null;
            using(ScriptBuilder sb = new ScriptBuilder()) {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address);//who
                sb.EmitParamJson(array);
                sb.EmitPushString("balanceOf");
                sb.EmitAppCall(new Hash160("0xbab964febd82c9629cc583596975f51811f25f47"));//合约hash
                data = sb.ToArray();
            }
            string script = Helper.Bytes2HexString(data);


            //invokescript方法
            //通过虚拟机传递脚本之后返回结果
            //此方法用于测试你的虚拟机脚本，如同在区块链上运行一样。这个RPC调用对区块链没有任何影响。
            HttpRequest httpRequest = new HttpRequest();
            string url = "http://127.0.0.1:20337/?jsonrpc=2.0&id=1&method=invokescript&params=[\"" + script + "\"]";
            JObject jObject = httpRequest.Get(url);
            string info = jObject.ToString();
            Console.WriteLine(jObject);

        }
    }
}
