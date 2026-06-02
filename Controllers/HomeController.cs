using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using azure_app_service.Models;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace azure_app_service.Controllers;

public class HomeController : Controller
{
        private readonly IConfiguration _config;

    public HomeController(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IActionResult> Index()
    {


        var keyVaultUrl = "https://app-service-demo-kv.vault.azure.net/";

        var client = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()
        );

        KeyVaultSecret secret = await client.GetSecretAsync("DbConnectionString");

        string connectionString = secret.Value;
        Console.WriteLine(connectionString);

        // var connString = _config.GetConnectionString("DefaultConnection");
        // var conn = new SqlConnection(connString);

        // conn.Open();
        int hits = 0;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // 1. Increment hits
            string updateSql = @"
                UPDATE PageHits
                SET Hits = Hits + 1
                WHERE Id = 1;

                SELECT Hits FROM PageHits WHERE Id = 1;
            ";

            using (SqlCommand cmd = new SqlCommand(updateSql, conn))
            {
                hits = (int)cmd.ExecuteScalar();
            }
        }

        var model = new HomeViewModel
        {
            Hits = hits
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
