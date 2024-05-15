using System;
using System.Linq;
using System.Reflection;

namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{
    /// <summary>
    /// Template for element from CardiovascularDisease DataSet.
    /// </summary>
    public class HeartFailurePredictionDataSet : DataSetBase
    {
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public long? Age { get; set; }
        /// <summary>
        /// Gets or sets the sex.
        /// </summary>
        /// <value>
        /// The sex. If male <c>true</c>; otherwise <c>false</c>.
        /// </value>
        public int? Sex { get; set; }
        /// <summary>
        /// Gets or sets the chest pain type.
        /// </summary>
        /// <value>
        /// The chest pain type.
        /// </value>
        public long? ChestPainType { get; set; }
        /// <summary>
        /// Gets or sets the resting blood pressure.
        /// </summary>
        /// <value>
        /// The resting blood pressure.
        /// </value>
        public long? RestingBloodPressure { get; set; }
        /// <summary>
        /// Gets or sets the serum cholestoral.
        /// </summary>
        /// <value>
        /// The serum cholestoral.
        /// </value>
        public long? SerumCholestoral { get; set; }
        /// <summary>
        /// Gets or sets the fasting blood sugar.
        /// </summary>
        /// <value>
        /// The fasting blood sugar.
        /// </value>
        public bool? FastingBloodSugar { get; set; }
        /// <summary>
        /// Gets or sets the resting electrocardiographic results.
        /// </summary>
        /// <value>
        /// The resting electrocardiographic results.
        /// </value>
        public long? RestingElectrocardiographicResults { get; set; }
        /// <summary>
        /// Gets or sets the maximum heart rate achieved.
        /// </summary>
        /// <value>
        /// The maximum heart rate achieved.
        /// </value>
        public long? MaximumHeartRateAchieved { get; set; }
        /// <summary>
        /// Gets or sets the exercise induced angina.
        /// </summary>
        /// <value>
        /// The exercise induced angina.
        /// </value>
        public bool? ExerciseInducedAngina { get; set; }
        /// <summary>
        /// Gets or sets the ST depression.
        /// </summary>
        /// <value>
        /// The ST depression.
        /// </value>
        public double? STDepression { get; set; }
        /// <summary>
        /// Gets or sets the ST slope.
        /// </summary>
        public STSlopeType STSlope { get; set; }

        /// <summary>
        /// The Cleveland data set class constructor.
        /// </summary>
        public HeartFailurePredictionDataSet()
        {
            DataSetType = DataSetTypes.HeartFailurePredictionDataSet;
        }
        /// <summary>
        /// Конструктор для создание экземпляра класса из Csv файла.
        /// </summary>
        /// <param name="str">The strings.</param>
        /// <exception cref="ArgumentException"/>
        public HeartFailurePredictionDataSet(params string[] str)
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
            DataSetType = DataSetTypes.HeartFailurePredictionDataSet;
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
    /// <summary>
    /// TODO: посмотреть на реальных данных.
    /// </summary>
    public enum STSlopeType
    {
        Up,
        Flat,
        Down
    }
}
