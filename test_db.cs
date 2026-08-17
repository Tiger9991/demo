using System;
using System.Data;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        var connStr = "Server=db56929.public.databaseasp.net; Database=db56929; User Id=db56929; Password=8k-Zz+S5#3bJ; Encrypt=False; MultipleActiveResultSets=True";
        using var conn = new SqlConnection(connStr);
        conn.Open();
        
        string[] tables = new string[] { "Traps", "TrapGroups", "TrapBaitMeasurements", "BaitMeasurements", "CaptureEvents", "Customers" };
        foreach (var tbl in tables) {
            try {
                using var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{tbl}]", conn);
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"TABLE [{tbl}]: {count} rows");
            } catch (Exception ex) {
                Console.WriteLine($"TABLE [{tbl}] ERROR: {ex.Message}");
            }
        }

        Console.WriteLine("\n--- SAMPLE TRAPS ---");
        using (var cmd = new SqlCommand("SELECT TOP 10 Id, TrapGroup, TrapNumber, status, Latitude, Longitude, LastEntryDate FROM Traps", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"Trap: Group={reader["TrapGroup"]}, Number={reader["TrapNumber"]}, Status={reader["status"]}, Lat={reader["Latitude"]}, Lng={reader["Longitude"]}, LastEntry={reader["LastEntryDate"]}");
            }
        }

        Console.WriteLine("\n--- SAMPLE TRAP GROUPS ---");
        using (var cmd = new SqlCommand("SELECT TOP 10 Id, TrapGroup, TrapNumber, CustomerId FROM TrapGroups", conn))
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                Console.WriteLine($"Group: Group={reader["TrapGroup"]}, Number={reader["TrapNumber"]}, Cust={reader["CustomerId"]}");
            }
        }
    }
}
