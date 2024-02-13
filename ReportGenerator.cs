using AnalysisAPISample;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace script
{
    public class ReportGenerator
    {
        public void ExportToCSV(List<TransactionData> data, string filePath)
        {
            WriteToCsv(data, filePath);
        }

        public void ExportTransactionsAvgToCSV(List<TransactionData> data, string filePath)
        {
            var simplifiedData = data.Select(t => new { t.Transaction, t.Average }).ToList();
            WriteToCsv(simplifiedData, filePath);
        }

        private void WriteToCsv<T>(IEnumerable<T> records, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    if (records.Any())
                    {
                        // Générer les en-têtes de colonnes, conversion en tableau pour compatibilité avec .NET 3.5
                        var header = string.Join(",", typeof(T).GetProperties().Select(p => p.Name).ToArray());
                        writer.WriteLine(header);

                        // Générer les lignes de données, conversion des valeurs en tableau également
                        foreach (var record in records)
                        {
                            var row = string.Join(",", typeof(T).GetProperties().Select(p => GetValueFormatted(p.GetValue(record, null))).ToArray());
                            writer.WriteLine(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting data to CSV at '{filePath}': {ex.Message}");
            }
        }

        private string GetValueFormatted(object value)
        {
            if (value == null) return "";
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("o", CultureInfo.InvariantCulture); // Format ISO 8601
            }
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }
            return value.ToString().Replace(",", ";"); // Échappement des virgules dans les données
        }

        public List<TransactionData> FilterTransactions(List<TransactionData> transactions, List<string> filter)
        {
            return transactions.FindAll(t => filter.Contains(t.Transaction));
        }
    }
}
