using System;
using System.Linq;
using System.Reflection;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    /// <summary>
    /// Template for element from CardiovascularDisease DataSet.
    /// </summary>
    public class CardiovascularDiseaseDataSet : DataSetBase
    {
        /// <summary>
        /// Age | Objective Feature | age | int (days).
        /// </summary>
        public long? Age { get; set; }
        /// <summary>
        /// Gender | Objective Feature | gender | categorical code.
        /// </summary>
        public int? Gender { get; set; }
        /// <summary>
        /// Height | Objective Feature | height | int (cm).
        /// </summary>
        public double? Height { get; set; }
        /// <summary>
        /// Weight | Objective Feature | weight | float (kg).
        /// </summary>
        public double? Weight { get; set; }
        /// <summary>
        /// Systolic blood pressure | Examination Feature | ap_hi | int.
        /// </summary>
        public long? SystolicBloodPressure { get; set; }
        /// <summary>
        /// Diastolic blood pressure | Examination Feature | ap_lo | int.
        /// </summary>
        public long? DiastolicBloodPressure { get; set; }
        /// <summary>
        /// Cholesterol | Examination Feature | cholesterol | 1: normal, 2: above normal, 3: well above normal.
        /// </summary>
        public long? Cholesterol { get; set; }
        /// <summary>
        /// Glucose | Examination Feature | gluc | 1: normal, 2: above normal, 3: well above normal.
        /// </summary>
        public long? Glucose { get; set; }
        /// <summary>
        /// Smoking | Subjective Feature | smoke | binary.
        /// </summary>
        public bool? Smoking { get; set; }
        /// <summary>
        /// Alcohol intake | Subjective Feature | alco | binary.
        /// </summary>
        public bool? AlcoholIntake { get; set; }
        /// <summary>
        /// Physical activity | Subjective Feature | active | binary.
        /// </summary>
        public bool? PhysicalActivity { get; set; }

        /// <summary>
        /// The CardiovascularDisease data set class constructor.
        /// </summary>
        public CardiovascularDiseaseDataSet()
        {
            DataSetType = DataSetTypes.CardiovascularDiseaseDataSet;
        }
        /// <summary>
        /// Конструктор для создание экземпляра класса из Csv файла.
        /// </summary>
        /// <param name="str">The strings.</param>
        /// <exception cref="ArgumentException"/>
        public CardiovascularDiseaseDataSet(params string[] str)
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
            DataSetType = DataSetTypes.CardiovascularDiseaseDataSet;
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
