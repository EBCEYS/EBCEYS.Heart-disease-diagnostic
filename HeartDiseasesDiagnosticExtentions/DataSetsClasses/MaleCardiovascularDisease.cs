using System;
using System.Linq;
using System.Reflection;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    /// <summary>
    /// Template for element from MaleCardiovascularDisease DataSet.
    /// </summary>
    public class MaleCardiovascularDiseaseDataSet : DataSetBase
    {
        /// <summary>
        /// Systolic blood pressure.
        /// </summary>
        public long? SystolicBloodPressure { get; set; }
        /// <summary>
        /// Cumulative tobacco (kg).
        /// </summary>
        public double? Tobacoo { get; set; }
        /// <summary>
        /// Low density lipoprotein cholesterol.
        /// </summary>
        public double? Cholesterol { get; set; }
        /// <summary>
        /// Adiposity.
        /// </summary>
        public double? Adiposity { get; set; }
        /// <summary>
        /// Family history of heart disease, a factor with levels "Absent" (false) and "Present" (true).
        /// </summary>
        public bool? FamilyHistory { get; set; }
        /// <summary>
        /// Type-A behavior.
        /// </summary>
        public int? TypeA { get; set; }
        /// <summary>
        /// Obesity.
        /// </summary>
        public double? Obesity { get; set; }
        /// <summary>
        /// Current alcohol consumption.
        /// </summary>
        public double? Alcohol { get; set; }
        /// <summary>
        /// Age at onset.
        /// </summary>
        public long? Age { get; set; }

        /// <summary>
        /// The MaleCardiovascularDisease data set class constructor.
        /// </summary>
        public MaleCardiovascularDiseaseDataSet()
        {
            DataSetType = DataSetTypes.MaleCardiovascularDiseaseDataSet;
        }
        /// <summary>
        /// Конструктор для создание экземпляра класса из Csv файла.
        /// </summary>
        /// <param name="str">The strings.</param>
        /// <exception cref="ArgumentException"/>
        public MaleCardiovascularDiseaseDataSet(params string[] str)
        {
            PropertyInfo[] props = GetType().GetProperties();
            if (str.Contains(string.Empty))
            {
                str = str.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            if (str.Length != props.Length)
            {
                throw new ArgumentException($"Can not create dataset object from this line! line length != properties number {str.Length} != {props.Length}");
            }
            DataSetType = DataSetTypes.MaleCardiovascularDiseaseDataSet;
            int i = 0;
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(this) != null)
                {
                    prop.SetValue(this, Convert.ChangeType(str[i], prop.PropertyType));
                    i++;
                }
            }
        }
    }
}