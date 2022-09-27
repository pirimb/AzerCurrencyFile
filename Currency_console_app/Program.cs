using Microsoft.Extensions.Configuration;
using NLog;
using System.Reflection;

namespace Currency_console_app
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static int GetJulianDay_JJJ()
        {
            DateTime dt = DateTime.Now;
            DateTime firstJan = new DateTime(dt.Year, 1, 1);

            int daysSinceFirstJan = (dt - firstJan).Days + 1;
            return daysSinceFirstJan;
        }

        public static DbConfig GetDbConfig(string dbName)
        {
            var dbConfig = new DbConfig();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false);
            IConfigurationRoot config = builder.Build();
            var dbConfiguration = "";
            if (dbName == "ASBANK")
            {
                dbConfiguration = "DbConfigurationProd";
            }
            else if (dbName == "TEST_SRV")
            {
                dbConfiguration = "DbConfigurationTest";
            }
            else
            {
                logger.Error($"arg[1] is not ASBANK or TEST_SRV");
            }
            dbConfig.Host = config[$"{dbConfiguration}:Host"];
            dbConfig.Port = config[$"{dbConfiguration}:Port"];
            dbConfig.ServiceName = config[$"{dbConfiguration}:ServiceName"];
            dbConfig.Username = config[$"{dbConfiguration}:Username"];
            dbConfig.Password = config[$"{dbConfiguration}:Password"];
            return dbConfig;
        }

        static void Main(string[] args)
        {
            try
            {
                var dbConfig = GetDbConfig(args[1]);
                DB.Init(dbConfig);

                string folderPath = args[0];

                string bankingDate = DB.GetBankingDate();
                var savedDatas = DB.GetFileData();

                int julianDay = GetJulianDay_JJJ();
                int rowNumber = 1;
                string rowNumberWithPadHeader = rowNumber.ToString().PadLeft(6, '0');
                string fileNumberInCurrentDay = "01"; //DB.GetFileNumberInCurrentDay()  <-----  from dataBase -!
                string fileNumber = fileNumberInCurrentDay.ToString().PadLeft(2, '0');
                string fileLabel = "FX_RATES".PadRight(10, ' ');
                string version = "10".PadRight(3, ' ');
                string fileSender = "0040".PadRight(6, ' ');
                string reserved = "";
                string fillWithSpacesHeader = reserved.PadRight(144, ' ');
                string fillWithSpacesTrailer = reserved.PadRight(183, ' ');
                int countOfRates = savedDatas.Count();
                string countOfRatesWithPad = countOfRates.ToString().PadLeft(6, '0');
                string dateFrom = "".PadLeft(14, ' '); //from db also -
                string baseCurrency = "".PadLeft(3, ' '); //from db also -
                string isCrossRate = " "; //from db also -
                string filledWithSpacesBody = reserved.PadRight(33, ' ');

                string fileName = $"K0040_{fileNumberInCurrentDay}.{julianDay}";
                //string folderPath = @$"C:\Users\p.s.bayramov\Desktop\folderr";
                string filePath = @$"{folderPath}\{fileName}.txt";

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.AppendAllText($"{filePath}", "");

                DateTime fileCreationDate = File.GetLastWriteTime(filePath);
                var fileCreationDateYYYYMMDD = fileCreationDate.ToString("yyyyMMdd");
                var fileCreationTimeHHMISS = fileCreationDate.ToString("HHmmss");


                string fileHeader = $"FH{rowNumberWithPadHeader}{fileLabel}{version}{fileSender}{fileCreationDateYYYYMMDD}{fileCreationTimeHHMISS}00{fileNumber}{bankingDate}{fillWithSpacesHeader}*";
                rowNumber++;
                File.AppendAllText($"{filePath}", fileHeader + Environment.NewLine);


                foreach (var n in savedDatas)
                {
                    var rowNumberWithPadBody = rowNumber.ToString().PadLeft(6, '0');
                    string fileBody = $"RD{rowNumberWithPadBody}{fileSender}{n.localCurrency}{n.RateCurrency}{n.RateType}{n.middleRate}{n.buyRate}{n.sellRate}{n.cbRate}{n.buyMultiplier}{n.sellMultiplier}{dateFrom}{baseCurrency}{isCrossRate}{filledWithSpacesBody}*";
                    rowNumber++;
                    File.AppendAllText($"{filePath}", fileBody + Environment.NewLine);

                }

                var rowNumberWithPadTrailer = rowNumber.ToString().PadLeft(6, '0');
                string fileTrailer = $"FT{rowNumberWithPadTrailer}{countOfRatesWithPad}{fillWithSpacesTrailer}*";
                File.AppendAllText($"{filePath}", fileTrailer + Environment.NewLine);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}