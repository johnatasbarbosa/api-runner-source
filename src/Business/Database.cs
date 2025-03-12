using Newtonsoft.Json.Linq;
using Npgsql;

namespace APIRunner.Business;

public static class Database
{
    public static bool VerifyConnection(string connectionString)
    {
        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
