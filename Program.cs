using System;
using healthmate_backend.Database;
using healthmate_backend.Models;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "server=mysql-2dbc541a-healthmate.k.aivencloud.com;" +
                                  "port=15855;" +
                                  "database=defaultdb;" +
                                  "user=avnadmin;" +
                                  "password=AVNS_lhFXyGW3wGvurVtSCVi;" +
                                  "SslMode=Required;" +
                                  "AllowPublicKeyRetrieval=True;";

        var repo = new UserRepository(connectionString);

        // Create
        repo.CreateUser(new User { Name = "Alice", Email = "alice@aiven.com" });

        // Read
        var users = repo.GetAllUsers();
        Console.WriteLine("👥 Users:");
        foreach (var user in users)
        {
            Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
        }

        // Update
        if (users.Count > 0)
        {
            repo.UpdateUserEmail(users[0].Id, "updated@aiven.com");
        }

        // Delete
        if (users.Count > 1)
        {
            repo.DeleteUser(users[1].Id);
        }
        repo.PrintDatabaseSchema();

    }
}