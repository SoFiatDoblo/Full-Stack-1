using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace TestGetHTTPChNU
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string url = "https://api.agify.io?name=";
        class Root
        {
            public string name { get; set; }
            public int age { get; set; }
            public int count { get; set; }
            public Root(string name, int age, int count)
            {
                this.name = name;
                this.age = age;
                this.count = count;
            }
            public override string ToString()
            {
                return "Name = " + name +
                    "\nPredicted Age = " + age.ToString() +
                    "\nCount = " + count.ToString();
            }
        }        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter name:");
            string name = Console.ReadLine();            
            SQLiteConnection sqlite_conn = CreateConnection();
            CreateTable(sqlite_conn);
            //await - async wait for task to complete
            await getAgePrediction(name, sqlite_conn);            
            ReadData(sqlite_conn);
            sqlite_conn.Close();
            Console.ReadKey();
        }
        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=agify.db; Version = 3; New = True; Compress = True;");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }
        static void CreateTable(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd;
            string createSql = "CREATE TABLE IF NOT EXISTS AgePrediction (Name TEXT, Age INT, Count INT)";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = createSql;
            sqlite_cmd.ExecuteNonQuery();
        }
        static void InsertData(SQLiteConnection conn, string name, int age, int count)
        {
            SQLiteCommand sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"INSERT INTO AgePrediction (Name, Age, Count) VALUES('{name}', {age}, {count})";
            sqlite_cmd.ExecuteNonQuery();
            Console.WriteLine();
            Console.WriteLine($"Data inserted: Name = {name}, Age = {age}, Count = {count}");
        }
        static void ReadData(SQLiteConnection conn)
        {
            SQLiteCommand sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM AgePrediction";
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            Console.WriteLine();
            while (sqlite_datareader.Read())
            {
                string name = sqlite_datareader.GetString(0);
                int age = sqlite_datareader.GetInt32(1);
                int count = sqlite_datareader.GetInt32(2);
                Console.WriteLine($"Name: {name}, Predicted Age: {age}, Count: {count}");
            }
        }        
        public static async Task getAgePrediction(string name, SQLiteConnection conn)
        {
            string response = await client.GetStringAsync(url + name);

            Console.WriteLine("API Response: " + response);
            Root root = JsonSerializer.Deserialize<Root>(response);

            if (root != null)
            {
                Console.WriteLine(root.ToString());
                InsertData(conn, root.name, root.age, root.count);  
            }
        }
    }
}
