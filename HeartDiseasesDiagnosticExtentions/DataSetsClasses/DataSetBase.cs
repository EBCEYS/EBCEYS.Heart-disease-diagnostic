using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    /// <summary>
    /// This class uses as parent for dataSets classes.
    /// </summary>
    public class DataSetBase
    {
        /// <summary>
        /// Gets the dataset type.
        /// </summary>
        /// <value>
        /// The dataset type.
        /// </value>
        public DataSetTypes? DataSetType { get; set; }

        /// <summary>
        /// Method checks for attributes were not null.
        /// </summary>
        /// <param name="nullProps">The out list of properties with null value.</param>
        /// <returns><c>true</c> if OK; otherwise <c>false</c></returns>
        public bool CheckAttributes(out List<string> nullProps)
        {
            PropertyInfo[] props = GetType().GetProperties();
            nullProps = new();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(this) == null && prop.PropertyType != typeof(DataSetTypes?))
                {
                    nullProps.Add(prop.Name);
                }
            }
            return nullProps.Count == 0;
        }
    }
}