using System.Reflection;
using System.Text.Json;

namespace HeartDiseasesDiagnosticExtentions.DataBaseExtensions
{
    public class DBRequestParams : Dictionary<string, SQLParam>
    {
        /// <summary>
        /// SELECT users.AddRole
        /// </summary>
        public string Command { get; set; }

        public DBRequestParams(string command) 
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException($"\"{nameof(command)}\" не может быть неопределенным или пустым.", nameof(command));
            }

            Command = command;
        }

        private string GetParamsString()
        {

            string line = string.Empty;
            foreach (KeyValuePair<string, SQLParam> prop in this)
            {
                line += GetPropertySQLString(prop.Value) + ", ";
            }
            return line.Remove(line.Length - 2);
        }

        private static string GetPropertySQLString(SQLParam prop)
        {
            Type type = prop.ParamType;

            object value = prop.Value;

            string result;
            if (type == typeof(double) || type == typeof(float) || type == typeof(double?) || type == typeof(float?))
            {
                result = $"{value}::DECIMAL";
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(int?) || type == typeof(long?))
            {
                result = $"{value}::INTEGER";
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                result = $"{value}::BOOLEAN";
            }
            else if (type == typeof(JsonDocument))
            {
                result = $"'{(value as JsonDocument).RootElement}'::JSON";
            }
            else if (type == typeof(DateTime))
            {
                result = $"'{value}'::TIMESTAMP";
            }
            else
            {
                result = $"'{value}'::TEXT";
            }

            return result;
        }

        public string CreateCommand()
        {
            return $"{Command}({GetParamsString()});";
        }
    }

    public class SQLParam
    {
        public object Value { get; set; }
        public Type ParamType { get; set; }

        public static SQLParam Create<T>(T value) 
        {
            return new SQLParam()
            {
                Value = value,
                ParamType = typeof(T)
            };
        }
    }
}
