using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using CitizenFX.Core;
using MySql.Data.MySqlClient;

namespace BrainstockServer
{
    class MySQL : BaseScript
    {
        private static MySqlConnection Database;
        //private const string ConnectionString = "server=localhost;user=root;password=1122;database=brainstock;port=3306";
        private const string ConnectionString = "server=mysql-mariadb-dal01-9-101.zap-hosting.com;user=zap408853-1;password=edeLrSIuFsr0GtlW;database=zap408853-1";
        //
        private static int[] playerList;

        public MySQL()
        {
            Debug.WriteLine("[MySQL] Connection started for Brainstock");
            //Connect();
            CreateTable();
        }

        public static void Connect()
        {
            Debug.WriteLine("[MySQL] Establishing connection to database...");

            try
            {
                Database = new MySqlConnection(ConnectionString);
                Database.Open();

                if (Database.State == ConnectionState.Open)
                {
                    Debug.WriteLine("[MySQL] Connection established");
                    //CreateTable();
                }
            }
            catch
            {
                Debug.WriteLine("[MySQL] Connection failed, retrying...");

                if (Database.State == ConnectionState.Closed)
                {
                    Thread.Sleep(5000);
                    Connect();
                }
            }
        }

        public static void Disconnect()
        {
            if (Database.State == ConnectionState.Closed)
                return;

            Debug.WriteLine("[MySQL] Closing connection...");

            try
            {
                Database.Close();

                if (Database.State == ConnectionState.Closed)
                    Debug.WriteLine("[MySql] Connection stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[MySQL] Connection failed to disconnect " + ex.ToString());
            }
        }

        public static void ExecuteQuery(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.ExecuteNonQuery();

                    connection.Close();

                    //Debug.WriteLine("[MySQL] Query sent " + query);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("[MySQL] Connection " + (connection.State == ConnectionState.Open));
                    Debug.WriteLine("[MySQL] [ERROR]" + ex);
                }
            }
        }

        public static DataTable ExecuteQueryWithResults(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        DataTable results = new DataTable();
                        results.Load(reader);

                        reader.Close();
                        connection.ClearAllPoolsAsync();

                    connection.CloseAsync();

                    //Debug.WriteLine("[MySQL] Query sent " + query);
                    return results;
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("[MySQL] Connection " + (connection.State == ConnectionState.Open));
                    Debug.WriteLine("[MySQL] [ERROR]" + ex);

                    return null;
                }
            }
        }

        public static void CreateTable()
        {
            ExecuteQuery("CREATE TABLE IF NOT EXISTS users (id VARCHAR(48), name TINYTEXT, model TINYTEXT, PRIMARY KEY(id), UNIQUE(id))");
            ExecuteQuery("CREATE TABLE IF NOT EXISTS safezones (id VARCHAR(16), level TINYINT, fuel SMALLINT, supplies SMALLINT, weaponslist TEXT, PRIMARY KEY(id), UNIQUE(id))");
            //Test
        }
    }
}
