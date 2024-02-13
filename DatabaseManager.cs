using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
//using CsvHelper;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
//using System.Data.SQLite;
using System;

namespace AnalysisAPISample
{
    public class DatabaseManager
    {
        private string dbPath;
        private string transactionSummaryQuery = @"
            SELECT
                Event_map.[Event Name] AS [Transaction],
                MIN(Event_meter.Value) AS [Minimum],
                AVG(Event_meter.Value) AS [Average],
                MAX(Event_meter.Value) AS [Maximum],
                (CASE
                    WHEN COUNT(Event_meter.Value) > 1
                    THEN
                        ROUND(
                            SQRT(
                                SUM((Event_meter.Value - avg_val.avg_value) * (Event_meter.Value - avg_val.avg_value)) /
                                (COUNT(Event_meter.Value) - 1)
                            ),
                            2
                        )
                    ELSE
                        NULL
                END) AS [Std Deviation],
                COUNT(*) AS [Iterations],
                SUM(CASE WHEN TransactionEndStatus.[Transaction End Status] = 'Pass' THEN 1 ELSE 0 END) AS [Pass],
                SUM(CASE WHEN TransactionEndStatus.[Transaction End Status] = 'Fail' THEN 1 ELSE 0 END) AS [Fail],
                SUM(CASE WHEN TransactionEndStatus.[Transaction End Status] = 'Stop' THEN 1 ELSE 0 END) AS [Stop]
            FROM
                Event_map
            INNER JOIN
                Event_meter ON Event_map.[Event ID] = Event_meter.[Event ID]
            INNER JOIN
                TransactionEndStatus ON TransactionEndStatus.[Status1] = Event_meter.[Status1]
            INNER JOIN (
                SELECT
                    [Event ID],
                    AVG(Value) AS avg_value
                FROM
                    Event_meter
                GROUP BY
                    [Event ID]
            ) AS avg_val ON avg_val.[Event ID] = Event_map.[Event ID]
            WHERE
                Event_map.[Event Type] = 'Transaction'
            GROUP BY
                Event_map.[Event Name]
            ORDER BY
                Event_map.[Event Name];
        ";

        private string testSummaryQuery = @"
        SELECT
            datetime([Start Time], 'unixepoch') AS Started,
            datetime([Result End Time], 'unixepoch') AS Ended,
            ([Result End Time] - [Start Time]) AS Duration
        FROM
            RESULT;
        ";

        public DatabaseManager(string dbPath)
        {
            this.dbPath = dbPath;
            Console.WriteLine("DatabaseManager initialized with DB path: " + dbPath);
        }

        public List<TransactionData> GetTransactionSummary()
        {
            List<TransactionData> transactions = new List<TransactionData>();

            try
            {
                Console.WriteLine("Connecting to SQLite database...");
                using (var connection = new SQLiteConnection($"Data Source={dbPath},Version=3;"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(transactionSummaryQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var transaction = new TransactionData
                                {
                                    Transaction = reader["Transaction"].ToString(),
                                    Minimum = Convert.ToDouble(reader["Minimum"]),
                                    Average = Convert.ToDouble(reader["Average"]),
                                    Maximum = Convert.ToDouble(reader["Maximum"]),
                                    StandardDeviation = reader.IsDBNull(reader.GetOrdinal("Std Deviation")) ? 0 : Convert.ToDouble(reader["Std Deviation"]),
                                    Iterations = Convert.ToInt32(reader["Iterations"]),
                                    Pass = Convert.ToInt32(reader["Pass"]),
                                    Fail = Convert.ToInt32(reader["Fail"]),
                                    Stop = Convert.ToInt32(reader["Stop"])
                                };
                                transactions.Add(transaction);
                            }
                        }
                    }
                }
                Console.WriteLine("Data successfully retrieved from database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving data from database: " + ex);
            }

            return transactions;
        }

    }

    public class TransactionData
    {
        public string Transaction { get; set; }
        public double Minimum { get; set; }
        public double Average { get; set; }
        public double Maximum { get; set; }
        public double StandardDeviation { get; set; }
        public int Iterations { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Stop { get; set; }
    }
}