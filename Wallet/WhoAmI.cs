using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace WhoAmI_Contract {
    public class Contract1 : SmartContract {
        public static byte[] Main(string cmd = "get", string address = "", string name = "") {
            var magicstr = "who am i - youngsun";
            if(cmd == "put") {
                Storage.Put(Storage.CurrentContext, address, name);
            } else if(cmd == "get") {
                byte[] bytes = Storage.Get(Storage.CurrentContext, address);
                return bytes;
            } else if(cmd == "delete") {
                Storage.Delete(Storage.CurrentContext, address);
            }
            return null;
        }
    }
}