using System;
using MySql.Data.MySqlClient;

namespace healthmate_backend.Database
{
    public class Db
    {
        private const string ConnectionString = "server=mysql-2dbc541a-healthmate.k.aivencloud.com;" +
                                                "port=15855;" +
                                                "database=defaultdb;" +
                                                "user=avnadmin;" +
                                                "password=AVNS_lhFXyGW3wGvurVtSCVi;" +
                                                "SslMode=Required;";

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                Console.WriteLine("✅ Database connection established.");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                throw;
            }
        }
    }
}