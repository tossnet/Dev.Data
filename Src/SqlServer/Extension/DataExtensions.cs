﻿using System;
using System.Data.SqlClient;

namespace Apps72.Dev.Data
{
    /// <summary>
    /// Helper Extensions to simplify data management
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Adds a value to the end of the System.Data.SqlClient.SqlParameterCollection.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to be added. Null value will be replaced by System.DBNull.Value.</param>
        /// <returns>A System.Data.SqlClient.SqlParameter object.</returns>
        public static SqlParameter AddWithValueOrDBNull(this SqlParameterCollection parameters, string parameterName, object value)
        {
            if (parameters != null)
            {
                return parameters.AddWithValue(parameterName, value == null ? System.DBNull.Value : value);
            }
            return null;
        }

        /// <summary>
        /// Add all properties / values to the end of the System.Data.SqlClient.SqlParameterCollection.
        /// If a property is already exist in Parameters collection, the parameter is removed and new added with new value.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="values">Object or anonymous object to convert all properties to parameters</param>
        public static void AddValues<T>(this SqlParameterCollection parameters, T values)
        {
            Schema.DataParameter.AddValues<T, SqlParameter>(parameters, values);
        }

    }
}
