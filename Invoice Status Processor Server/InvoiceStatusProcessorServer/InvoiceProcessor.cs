using System;
using System.Collections.Generic;
using System.Data.SqlClient;

public class InvoiceProcessor
{
    private readonly Config _config;
    public int LastProcessedPending { get; private set; }
    public int LastSuccess { get; private set; }
    public int LastError { get; private set; }

    public InvoiceProcessor(Config config)
    {
        _config = config;
    }

    public void ProcessInvoices()
    {
        Console.WriteLine($"[{DateTime.Now}] Запуск цикла обработки инвойсов...");

        int processedPending = 0;
        int successCount = 0;
        int errorCount = 0;

        using SqlConnection connection = new(_config.ConnectionString);
        try
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var invoicesToProcess = SelectInvoices(connection, transaction, _config.MaxErrorRetries);
            processedPending = invoicesToProcess.Count;

            foreach (var invoice in invoicesToProcess)
            {
                bool isSuccess = new Random().NextDouble() < 0.3;
                string newStatus = isSuccess ? "success" : "error";
                int newRetryCount = invoice.RetryCount + (newStatus == "error" ? 1 : 0);

                UpdateInvoice(connection, transaction, invoice.Id, newStatus, newRetryCount);

                Console.WriteLine($"Инвойс {invoice.Id}: {invoice.Status} (попытка {invoice.RetryCount}) -> {newStatus}");
                if (isSuccess) successCount++;
                else errorCount++;
            }

            transaction.Commit();

            LastProcessedPending = processedPending;
            LastSuccess = successCount;
            LastError = errorCount;

            Console.WriteLine($"Цикл завершён: обработано {processedPending}, успех: {successCount}, ошибка: {errorCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке: {ex.Message}");
        }
    }

    private static List<(int Id, string Status, int RetryCount)> SelectInvoices(SqlConnection conn, SqlTransaction tran, int maxRetries)
    {
        string sql = @"
            SELECT Id, Status, RetryCount
            FROM Invoices
            WHERE Status = 'pending'
               OR (Status = 'error' AND RetryCount < @MaxRetries)
            FOR UPDATE";

        using var cmd = new SqlCommand(sql, conn, tran);
        cmd.Parameters.AddWithValue("@MaxRetries", maxRetries);

        var list = new List<(int, string, int)>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        }
        return list;
    }

    private static void UpdateInvoice(SqlConnection conn, SqlTransaction tran, int id, string status, int retryCount)
    {
        string sql = @"
            UPDATE Invoices
            SET Status = @Status,
                UpdatedAt = GETDATE(),
                RetryCount = @RetryCount,
                LastAttemptAt = GETDATE()
            WHERE Id = @Id";

        using var cmd = new SqlCommand(sql, conn, tran);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@RetryCount", retryCount);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }
}