using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Contract {
    public class NEP5 : SmartContract {
        public static string Name() => "Youngsun";//以字符串格式返回代币名称。
        public static string Symbol() => "YUN";//以字符串格式返回代币符号。
        public static readonly byte[] Owner = "AQye22dcXV1jCrzzC4iGbyM68LADwPSs11".ToScriptHash();//所有者
        public static byte Decimals() => 2;//返回代币的小数位数值，显示代币的分割单位。
        private const ulong factor = 100;
        private const ulong total_amount = 277013309 * factor;

        [DisplayName("transfer")]
        public static event Action<byte[], byte[], BigInteger> Transferred;

        public static object Main(string cmd, params object[] args) {
            if(Runtime.Trigger == TriggerType.Application) {
                if(cmd == "deploy") return Deploy();
                if(cmd == "totalSupply") return TotalSupply();
                if(cmd == "name") return Name();
                if(cmd == "symbol") return Symbol();
                if(cmd == "decimals") return Decimals();
                if(cmd == "transfer") {
                    if(args.Length != 3 || args[0] == null || ((byte[])args[0]).Length == 0 || args[1] == null || ((byte[])args[1]).Length == 0) return "args error";
                    byte[] from = (byte[])args[0];
                    byte[] to = (byte[])args[1];
                    BigInteger value = (BigInteger)args[2];
                    return Transfer(from, to, value);
                }
                if(cmd == "balanceOf") {
                    if(args.Length != 1 || args[0] == null || ((byte[])args[0]).Length == 0) return "args error";
                    byte[] account = (byte[])args[0];
                    return BalanceOf(account);
                };
            }
            return "args error";
        }
        //返回大数：返回账户余额。
        public static BigInteger BalanceOf(byte[] address) {
            return Storage.Get(Storage.CurrentContext, address).AsBigInteger();
        }

        //返回从一个地址转入另一地址的代币量。
        public static bool Transfer(byte[] from, byte[] to, BigInteger value) {
            if(value <= 0) return false;//一块都不转进来干嘛
            if(!Runtime.CheckWitness(from)) return false;//你是不是在操纵自己的钱
            if(to.Length != 20) return false;//简单对方校验地址

            BigInteger from_value = Storage.Get(Storage.CurrentContext, from).AsBigInteger();//获取自己的钱
            if(from_value < value) return false;//钱都不够转
            if(from == to) return true;//自己转给自己
            if(from_value == value) {//如果钱刚刚够
                Storage.Delete(Storage.CurrentContext, from);//删除自己的全部
            } else {
                Storage.Put(Storage.CurrentContext, from, from_value - value);//不然就给你剩下的
            }
            BigInteger to_value = Storage.Get(Storage.CurrentContext, to).AsBigInteger();//先拿到对方余额
            Storage.Put(Storage.CurrentContext, to, to_value + value);//然后用余额加收到的重新存入
            Transferred(from, to, value);
            return true;
        }

        //返回大数：返回代币总量。
        public static BigInteger TotalSupply() {
            return Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        }

        //初始化
        public static bool Deploy() {
            if(!Runtime.CheckWitness(Owner)) return false;
            byte[] total_supply = Storage.Get(Storage.CurrentContext, "totalSupply");
            if(total_supply.Length != 0) return false;//验证只能deploy一次
            Storage.Put(Storage.CurrentContext, Owner, total_amount);//存进去我有多少钱
            Storage.Put(Storage.CurrentContext, "totalSupply", total_amount);//存入发行总量
            Transferred(null, Owner, total_amount);
            return true;
        }
    }
}