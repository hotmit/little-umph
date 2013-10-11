using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace LittleUmph
{
    /// <summary>
    /// Store and retreive Settings to and from the database.
    /// It's like the DB version of visual studio Setting dir.
    /// </summary>
    public class GlobalConfig
    {
        #region [ Private Variables ]
        private QuickDb _db;
        private SqlCommand _cmdSelect, _cmdUpdate, _cmdInsert, _cmdDelete;
        #endregion

        #region [ Indexer ]
        /// <summary>
        /// Gets or sets the <see cref="System.String"/> with the specified config name.
        /// </summary>
        /// <value></value>
        public string this[string configName]
        {
            get { return GetValue(configName); }
            set { SetValue(configName, value); }
        }
        #endregion

        #region [ Constructor ]
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConfig"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public GlobalConfig(string connectionString) : this (connectionString, "GlobalConfig")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConfig" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        public GlobalConfig(string connectionString, string tableName)
        {
            _db = new QuickDb(connectionString);

            _cmdSelect = new SqlCommand(string.Format("SELECT ConfigValue FROM {0} WHERE ConfigName = @ConfigName", tableName));
            _cmdSelect.Parameters.Add("@ConfigName", SqlDbType.VarChar);

            _cmdUpdate = new SqlCommand(string.Format("Update {0} SET ConfigValue = @ConfigValue WHERE ConfigName = @ConfigName", tableName));
            _cmdUpdate.Parameters.Add("@ConfigName", SqlDbType.VarChar);
            _cmdUpdate.Parameters.Add("@ConfigValue", SqlDbType.VarChar);

            _cmdInsert = new SqlCommand(string.Format("INSERT INTO {0} (ConfigName, ConfigValue) VALUES (@ConfigName, @ConfigValue)", tableName));
            _cmdInsert.Parameters.Add("@ConfigName", SqlDbType.VarChar);
            _cmdInsert.Parameters.Add("@ConfigValue", SqlDbType.VarChar);

            _cmdDelete = new SqlCommand(string.Format("DELETE FROM {0} WHERE ConfigName = @ConfigName", tableName));
            _cmdDelete.Parameters.Add("@ConfigName", SqlDbType.VarChar);
        }
        #endregion

        #region [ Traditional Get Methods ]
        /// <summary>
        /// Gets the value, return null on error.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string GetValue(string name)
        {
            return GetValue(name, null);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">Return this value on error.</param>
        /// <returns></returns>
        public string GetValue(string name, string defaultValue)
        {
            _cmdSelect.Parameters["@ConfigName"].Value = name;
            string value = _db.ScalarQuery(_cmdSelect, "ConfigValue", defaultValue);
            return value;
        }

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public int GetInt(string name, int defaultValue)
        {
            string v = GetValue(name);

            if (v == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(v);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the long.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public long GetLong(string name, long defaultValue)
        {
            string v = GetValue(name);

            if (v == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt64(v);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the double.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public double GetDouble(string name, double defaultValue)
        {
            string v = GetValue(name);

            if (v == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDouble(v);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the boolean.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public bool GetBoolean(string name, bool defaultValue)
        {
            string v = GetValue(name);

            if (v == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(v);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public DateTime GetDateTime(string name, DateTime defaultValue)
        {
            string v = GetValue(name);

            if (v == null)
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToDateTime(v);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        #endregion

        #region [ Set/Change/Remove Methods ]
        /// <summary>
        /// Add new row to the config table (CAN manage non existed row)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string name, string value)
        {
            _cmdUpdate.Parameters["@ConfigName"].Value = name;
            _cmdUpdate.Parameters["@ConfigValue"].Value = value;

            int affected = _db.NonQuery(_cmdUpdate);

            if (affected == 0)
            {
                _cmdInsert.Parameters["@ConfigName"].Value = name;
                _cmdInsert.Parameters["@ConfigValue"].Value = value;

                affected = _db.NonQuery(_cmdInsert);

                if (affected > 0)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Change the value of a config row 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <seealso cref="SetValue"/>
        public bool ChangeValue(string name, string value)
        {
            return SetValue(name, value);
        }

        /// <summary>
        /// Remove config row from the config table
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveConfig(string name)
        {
            _cmdDelete.Parameters["@ConfigName"].Value = name;
            int affected = _db.NonQuery(_cmdDelete);
            return affected > 0;
        }
        #endregion

        #region [ Generic GetValue Method ]
        /// <summary>
        /// Retrieve the value and convert accordingly to the type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValue<T>(string name, T defaultValue) where T : IConvertible
        {
            string value = GetValue(name, null);

            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        #endregion
    }
}
