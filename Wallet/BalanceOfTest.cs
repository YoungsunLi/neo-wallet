using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ThinNeo;

namespace Wallet {
    class BalanceOfTest {
        public void Run() {
            string address = "AQye22dcXV1jCrzzC4iGbyM68LADwPSs11";
            byte[] data = null;
            using(ScriptBuilder sb = new ScriptBuilder()) {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address);//who
                sb.EmitParamJson(array);
                sb.EmitPushString("balanceOf");
                sb.EmitAppCall(new Hash160("0x8e64b705793e59b9e8b9ce9412092a3881eaa5fd"));//合约hash(我的Youngsun币:0x8e64b705793e59b9e8b9ce9412092a3881eaa5fd)
                data = sb.ToArray();
            }
            string script = Helper.Bytes2HexString(data);


            //invokescript方法
            //通过虚拟机传递脚本之后返回结果
            //此方法用于测试你的虚拟机脚本，如同在区块链上运行一样。这个RPC调用对区块链没有任何影响。
            HttpRequest httpRequest = new HttpRequest();
            JObject jObject = httpRequest.Get("invokescript", script);
            string info = jObject.ToString();
            Console.WriteLine(jObject);

        }
    }
}
