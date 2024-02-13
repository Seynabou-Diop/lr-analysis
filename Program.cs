using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Analysis.Api;
using script;

namespace AnalysisAPISample
{
    class Program
    {
        static LrAnalysis analysisApi;
        static Session currentSession;
        static Run currentRun;

        static void Main(string[] args)
        {
            analysisApi = new LrAnalysis();

            // if (args.Length < 1)
            // {
            //     Console.WriteLine("Please provide the test number as a command-line argument.");
            //     return;
            // }

            string testNumber = "4094";
            string lrrRootPath = "C:\\workspace\\results\\nc2\\NNCTRA0103";
            string exportedRootPath = "C:\\workspace\\results\\exported\\NNCTRA0103\\" + testNumber;

            // if (string.IsNullOrEmpty(lrrRootPath) || string.IsNullOrEmpty(exportedRootPath))
            // {
            //     Console.WriteLine("Please set the environment variables LRR_ROOT_PATH and EXPORTED_ROOT_PATH.");
            //     return;
            // }

            string resName = Path.Combine(Path.Combine(lrrRootPath, testNumber), $"{testNumber}.lrr");
            string sessionName = Path.Combine(Path.Combine(exportedRootPath, "LRA"), "lra.lra");

            //if (CreateSession(sessionName, resName))
            //{
                //Console.WriteLine("Session created successfully.");
                //string dbPath = Path.ChangeExtension(sessionName, ".db");
                string dbPath = "C:\\workspace\\results\\exported\\NNCTRA0103\\4094\\LRA\\lra.db";
                ExportDatabaseData(dbPath, testNumber, exportedRootPath);
            //}
            //else
            //{
              //  Console.WriteLine("Error creating session.");
            //}
        }

        private static bool CreateSession(string sessionName, string resName)
        {
            Console.WriteLine("Creating session...");

            bool result = analysisApi.Session.Create(sessionName, resName);
            if (result)
            {
                currentSession = analysisApi.Session;
                if (currentSession.Runs.Count > 0)
                {
                    currentRun = currentSession.Runs[0];
                }
                else
                {
                    Console.WriteLine("Session was created, but no runs were found.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Failed to create session.");
                return false;
            }
            return true;
        }

        private static void ExportDatabaseData(string dbPath, string testNumber, string exportedRootPath)
        {
            Console.WriteLine("Starting database export...");
            // Supposons que vous avez une implémentation fonctionnelle de DatabaseManager et TransactionData
            DatabaseManager dbManager = new DatabaseManager(dbPath);
            ReportGenerator reportGenerator = new ReportGenerator();

            // Simulez la récupération des données. Vous devez implémenter GetTransactionSummary.
            var transactionData = dbManager.GetTransactionSummary();

            if (transactionData == null || !transactionData.Any())
            {
                Console.WriteLine("No data to export.");
                return;
            }

            string basePath = Path.Combine(exportedRootPath, "Summary");
            Directory.CreateDirectory(basePath);

            string summaryCsvPath = Path.Combine(basePath, "summary.csv");
            reportGenerator.ExportToCSV(transactionData, summaryCsvPath);

            // Exemple de filtrage et d'exportation des données
            var filteredData1 = reportGenerator.FilterTransactions(transactionData, TransactionTemplates.NC2_main_kpis_1);
            var filteredData2 = reportGenerator.FilterTransactions(transactionData, TransactionTemplates.NC2_main_kpis_2);

            string summaryFiltered1CsvPath = Path.Combine(basePath, "summary_filtered_1.csv");
            string summaryFiltered2CsvPath = Path.Combine(basePath, "summary_filtered_2.csv");
            string summaryAvgCsvPath = Path.Combine(basePath, "summary_avg.csv");

            reportGenerator.ExportToCSV(filteredData1, summaryFiltered1CsvPath);
            reportGenerator.ExportToCSV(filteredData2, summaryFiltered2CsvPath);
            reportGenerator.ExportTransactionsAvgToCSV(transactionData, summaryAvgCsvPath);

            Console.WriteLine("Data export completed.");
        }
    }
}
