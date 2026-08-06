using MySql.Data.MySqlClient;
using System.Configuration;

namespace C969_Project.Database
{
    public class DatabaseManager
    {
        public static MySqlConnection? Conn { get; set; }

        public static void StartConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["localDb"].ConnectionString;


            try
            {
                Conn = new MySqlConnection(connectionString);

                Conn.Open();

                MessageBox.Show("Connected to Database");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void EndConnection()
        {
            try
            {
                if (Conn != null)
                {
                    Conn.Close();
                }

                Conn = null;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static List<CustomerDisplay> GetCustomers()
        {
            var customers = new List<CustomerDisplay>();

            string sql = @"
                            SELECT c.customerId, c.CustomerName, c.active, a.address, a.address2, a.postalCode, a.phone, ci.city, co.country
                            FROM customer c
                            JOIN address a ON c.addressId = a.addressId
                            JOIN city ci ON a.cityId = ci.cityId
                            JOIN country co ON co.countryId = ci.countryId
                            ORDER BY c.customerName";

            using (var cmd = new MySqlCommand(sql, Conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    customers.Add(new CustomerDisplay
                    {
                        CustomerId = reader.GetInt32("customerId"),
                        CustomerName = reader.GetString("customerName"),
                        Active = reader.GetBoolean("active"),
                        Address = reader.GetString("address"),
                        Address2 = reader.GetString("address2"),
                        PostalCode = reader.GetString("postalCode"),
                        City = reader.GetString("city"),
                        Country = reader.GetString("country"),
                        Phone = reader.GetString("phone")
                    });
                }
            }

            return customers;
        }

        //User input is bound as parameters, never concatenated, so a username like
        //"' OR 1=1 --" is matched as a literal string instead of becoming SQL.
        public static bool ValidateUser(string userName, string password)
        {
            //Plaintext comparison per ADR 0001 - the provided database ships
            //plaintext passwords and a seeded test/test account. The COLLATE clause
            //forces a case-sensitive match on the password; without it MySQL's
            //default case-insensitive collation would accept "TEST" for "test".
            //The username is deliberately left case-insensitive, as usernames
            //conventionally are. COLLATE rather than the BINARY operator, which
            //MySQL deprecated in 8.0.27 and expects to remove.
            string sql = @"
                            SELECT u.userId
                            FROM user u
                            WHERE u.userName = @userName
                              AND u.password = @password COLLATE utf8mb4_bin
                            LIMIT 1";

            using (var cmd = new MySqlCommand(sql, Conn))
            {
                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.Parameters.AddWithValue("@password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    //No matching row means either the username or the password was
                    //wrong. Both collapse to false so the caller cannot tell them
                    //apart, and neither case throws.
                    return reader.Read();
                }
            }
        }
    }
}