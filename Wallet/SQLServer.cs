using System;
using System.Data;
using System.Data.SqlClient;

namespace Wallet {
    class SQLServer {
        SqlConnection sqlConnection = new SqlConnection();

        //根据address asset查询utxo
        public SqlDataReader GetUTXO(string address, string asset) {
            string cmd =
                "select txid,n,address,asset,value "
                + "from vout "
                + "where id not in "
                + "(select vout.id "
                + "from vout vout "
                + "join vin vin "
                + "on vin.txidRef= vout.txid and vin.nRef= vout.n) "
                + "and[address] = '" + address + "' "
                + "and asset= '" + asset + "'";
            SqlDataReader sqlDataReader = CommandReader(cmd);
            return sqlDataReader;
        }

        //根据address查询utxo
        public SqlDataReader GetUTXO(string address) {
            string cmd =
                "select txid,n,address,asset,value "
                + "from vout "
                + "where id not in "
                + "(select vout.id "
                + "from vout vout "
                + "join vin vin "
                + "on vin.txidRef= vout.txid and vin.nRef= vout.n) "
                + "and[address] = '" + address + "'";
            SqlDataReader sqlDataReader = CommandReader(cmd);
            return sqlDataReader;
        }

        //更新高度
        public void UpdateBlockHeight(long blockNum) {
            string cmd = "update blockHeight set blockHeight=" + blockNum + " where net='LsunTsetnet'";
            CommandNonQuery(cmd);
        }

        //储存vout
        public void AddVout(string blockIndex, string txid, int n, string asset, string address, string value) {
            string cmd = "insert into lsunVout(blockIndex, txid, n, asset, address, value)values('" + blockIndex + "','" + txid + "','" + n + "','" + asset + "','" + address + "','" + value + "')";
            CommandNonQuery(cmd);
        }
        //储存vout
        public void AddVin(string blockIndex, string txidRef, int nRef) {
            string cmd = "insert into lsunVin(blockIndex, txidRef, nRef)values('" + blockIndex + "','" + txidRef + "','" + nRef + "')";
            CommandNonQuery(cmd);
        }

        //获取上次爬到的高度
        public long GetStartBlock() {
            string cmd = "select blockHeight from blockHeight where net='LsunTsetnet'";
            SqlDataReader sqlDataReader = CommandReader(cmd);
            if(sqlDataReader.Read()) {
                try {
                    return (long)sqlDataReader["blockHeight"] + 1;
                } catch(Exception) {
                    return 0;
                }
            } else {
                return 0;
            }
        }



        //有返回结果的查询
        private SqlDataReader CommandReader(string cmd) {
            SqlCommand sqlCommand = new SqlCommand(cmd, sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            return sqlDataReader;
        }

        //仅查询
        private void CommandNonQuery(string cmd) {
            SqlCommand sqlCommand = new SqlCommand(cmd, sqlConnection);
            sqlCommand.ExecuteNonQuery();
        }

        //连接数据库
        public void Open() {
            sqlConnection.ConnectionString = "server=127.0.0.1; database=NEO; integrated security=SSPI";
            sqlConnection.Open();
            if(sqlConnection.State == ConnectionState.Closed) { Console.WriteLine("sqlConnection error"); }
        }

        //关闭数据库
        public void Close() {
            sqlConnection.Close();
        }

    }
}
