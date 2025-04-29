using healthmate_backend.Database;
using MySql.Data.MySqlClient;

class Program
{
    static void Main(string[] args)
    {
        using var conn = Db.GetConnection();

        using var cmd = new MySqlCommand("SELECT NOW();", conn);
        var now = cmd.ExecuteScalar();
        Console.WriteLine($"🕒 DB Time: {now}");
    }
}