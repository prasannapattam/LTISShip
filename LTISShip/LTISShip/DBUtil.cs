using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTISShip
{
    public static class DBUtil
    {
        public static DataTable ExecuteDataTable(string connectionString, string cmdText)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmdText, connectionString);
            da.Fill(dt);
            da.Dispose();

            return dt;
        }

        public static SqlDataReader ExecuteReader(string connectionString, string cmdText)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(cmdText, conn);

            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            return dr;
        }

        public static void Execute(string connectionString, string cmdText)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(cmdText, conn);
            try
            {
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                cmd.Dispose();
            }
        }

        public static object ExecuteScalar(string connectionString, string cmdText)
        {

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlCommand cmd = new SqlCommand(cmdText, conn);
            try
            {
                object value = cmd.ExecuteScalar();
                return value;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                cmd.Dispose();
            }
        }

        public static string EscapeSingleQuote(object str)
        {
            return str.ToString().Replace("'", "''").Trim();
        }

        public static string DBString(object str)
        {
            if (str == null || str == DBNull.Value)
                return "";
            else
                return str.ToString();
        }
    }
}
