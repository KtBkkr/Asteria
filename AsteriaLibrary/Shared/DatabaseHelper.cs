using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace AsteriaLibrary.Shared
{
    public class DatabaseHelper
    {
        #region Methods
        public static string GetString(MySqlDataReader dr, string columnName)
        {
            int ordinal = dr.GetOrdinal(columnName);
            if (dr.IsDBNull(ordinal))
                return string.Empty;
            else
                return dr.GetString(ordinal);
        }

        public static Int32 GetInt32(MySqlDataReader dr, string columnName)
        {
            int ordinal = dr.GetOrdinal(columnName);
            if (dr.IsDBNull(ordinal))
                return -1;
            else
                return dr.GetInt32(ordinal);
        }

        public static DateTime GetDateTime(MySqlDataReader dr, string columnName)
        {
            int ordinal = dr.GetOrdinal(columnName);
            if (dr.IsDBNull(ordinal))
                return DateTime.MinValue;
            else
                return dr.GetDateTime(ordinal);
        }
        #endregion
    }
}
