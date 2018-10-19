using ThinNeo;

namespace Wallet {
    class UTXO {
        public Hash256 txid;
        public int n;
        public string asset;
        public string address;
        public decimal value;

        public UTXO(Hash256 txid, int n, string asset, string address, decimal value) {
            this.txid = txid;
            this.n = n;
            this.address = address;
            this.asset = asset;
            this.value = value;
        }
    }
}
