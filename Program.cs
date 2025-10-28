using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.Sqlite;

class Program
{
    static async Task Main()
    {
        // посилання на API (обмеження довжини факту — щоб не було роману на екран)
        string apiUrl = "https://catfact.ninja/fact?max_length=100";

        try
        {
            // 1. Отримуємо відповідь від API
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(apiUrl);
                Console.WriteLine("Відповідь від API:");
                Console.WriteLine(response);

                // 2. Парсимо JSON і дістаємо факт
                var jsonDoc = JsonDocument.Parse(response);
                string fact = jsonDoc.RootElement.GetProperty("fact").GetString();

                Console.WriteLine("\nОтриманий факт: " + fact);

                // 3. Підключаємось або створюємо котячу базу даних SQLite
                using (var connection = new SqliteConnection("Data Source=catfacts.db"))
                {
                    connection.Open();

                    var createTable = connection.CreateCommand();
                    createTable.CommandText =
                        @"CREATE TABLE IF NOT EXISTS Facts (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Fact TEXT
                        );";
                    createTable.ExecuteNonQuery();

                    // 4. Додаємо новий факт у базу
                    var insert = connection.CreateCommand();
                    insert.CommandText = "INSERT INTO Facts (Fact) VALUES (@fact)";
                    insert.Parameters.AddWithValue("@fact", fact);
                    insert.ExecuteNonQuery();

                    Console.WriteLine("\nФакт успішно збережено у котячу базу даних!");

                    // 5. Виводимо всі записи з таблиці
                    var select = connection.CreateCommand();
                    select.CommandText = "SELECT * FROM Facts";
                    using (var reader = select.ExecuteReader())
                    {
                        Console.WriteLine("\n--- Усі факти з бази ---");
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string savedFact = reader.GetString(1);
                            Console.WriteLine($"{id}. {savedFact}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Щось пішло не так 😿");
            Console.WriteLine(ex.Message);
        }
    }
}
