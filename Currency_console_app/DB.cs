using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;
using Oracle.ManagedDataAccess.Client;


namespace Currency_console_app
{
    public class DB
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        
        static string connectionString = "";

        public static string Init(DbConfig dbConfig){

            return connectionString = $"Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST ={dbConfig.Host})(PORT={dbConfig.Port})))" + $"(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={dbConfig.ServiceName}))); " +
                            $"User Id ={dbConfig.Username}; Password ={dbConfig.Password}; Min Pool Size = 1; Max Pool Size = 10; Pooling = True;" +
                            $" Validate Connection = true; Connection Lifetime = 300; Connection Timeout = 300;";
        }

        //ASBANK
        //TEST_SRV


        public static OracleConnection GetConnection()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string not initialized!");
            }

            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static List<SavedData> GetFileData()
        {
            var savedDatas = new List<SavedData>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT * FROM sbnk_prl.v_azericard_mezenne_file";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var savedData = new SavedData();
                                savedData.localCurrency = reader["LOCAL_CURRENCY"].ToString();
                                savedData.RateCurrency = reader["RATE_CURRENCY"].ToString();
                                savedData.RateType = reader["RATE_TYPE"].ToString().PadRight(32, ' ');
                                savedData.middleRate = reader["MIDDLE_RATE"].ToString().Replace(",", "").PadRight(9, '0').PadLeft(20, '0');
                                savedData.buyRate = reader["BUY_RATE"].ToString().Replace(",", "").PadRight(9, '0').PadLeft(20, '0');
                                savedData.sellRate = reader["SELL_RATE"].ToString().Replace(",", "").PadRight(9, '0').PadLeft(20, '0');
                                savedData.cbRate = reader["CB_RATE"].ToString().Replace(",", "").PadRight(9, '0').PadLeft(20, '0');
                                savedData.buyMultiplier = reader["BUY_MULTIPLIER"].ToString().Replace(",", "").PadLeft(7, '0');
                                savedData.sellMultiplier = reader["SELL_MULTIPLIER"].ToString().Replace(",", "").PadLeft(7, '0');
                                savedDatas.Add(savedData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return savedDatas;
        }

        public static string GetBankingDate()
        {
            string dt = "";
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT sbnk_prl.carigun FROM dual";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dt = Convert.ToDateTime(reader["CARIGUN"]).ToString("yyyyMMdd");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return dt;
        }

        public static string GetFileNumberInCurrentDay()
        {
            string fileNumber = "";
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT file_number FROM dual";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fileNumber = reader["file_number"].ToString().PadLeft(2, '0');
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return fileNumber;
        }

    }
}
