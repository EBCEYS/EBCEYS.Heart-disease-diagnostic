namespace HeartDiseasesDiagnosticExtentions.DataSetsClasses
{

    /// <summary>
    /// The data set types.
    /// </summary>
    /// <seealso cref="https://www.kaggle.com/datasets/fedesoriano/heart-failure-prediction">HeartFailurePrediction</seealso>
    /// <seealso cref="https://www.kaggle.com/datasets/sulianova/cardiovascular-disease-dataset">CardiovascularDiseaseDataSet</seealso>
    /// <seealso cref="https://www.kaggle.com/datasets/yassinehamdaoui1/cardiovascular-disease">MaleCardiovascularDisease</seealso>

    public enum DataSetTypes
    {
        /// <summary>
        /// Uses to set unknown dataset type.
        /// </summary>
        Unknown,
        /// <summary>
        /// This dataset was created by combining different datasets already available independently but not combined before. In this dataset, 5 heart datasets are combined over 11 common features which makes it the largest heart disease dataset available so far for research purposes. The five datasets used for its curation are:
        /// <br/>Cleveland: 303 observations
        /// <br/>Hungarian: 294 observations
        /// <br/>Switzerland: 123 observations
        /// <br/>Long Beach VA: 200 observations
        /// <br/>Stalog (Heart) Data Set: 270 observations.
        /// </summary>
        /// <seealso cref="https://www.kaggle.com/datasets/fedesoriano/heart-failure-prediction"/>
        HeartFailurePredictionDataSet,
        /// <summary>
        /// Features:
        /// <br/>Age | Objective Feature | age | int (days)
        /// <br/>Height | Objective Feature | height | int (cm) |
        /// <br/>Weight | Objective Feature | weight | float (kg) |
        /// <br/>Gender | Objective Feature | gender | categorical code |
        /// <br/>Systolic blood pressure | Examination Feature | ap_hi | int |
        /// <br/>Diastolic blood pressure | Examination Feature | ap_lo | int |
        /// <br/>Cholesterol | Examination Feature | cholesterol | 1: normal, 2: above normal, 3: well above normal |
        /// <br/>Glucose | Examination Feature | gluc | 1: normal, 2: above normal, 3: well above normal |
        /// <br/>Smoking | Subjective Feature | smoke | binary |
        /// <br/>Alcohol intake | Subjective Feature | alco | binary |
        /// <br/>Physical activity | Subjective Feature | active | binary |
        /// <br/>Presence or absence of cardiovascular disease | Target Variable | cardio | binary |
        /// </summary>
        /// <seealso cref="https://www.kaggle.com/datasets/sulianova/cardiovascular-disease-dataset"/>
        CardiovascularDiseaseDataSet,
        /// <summary>
        /// Rousseauw, J., du Plessis, J., Benade, A., Jordaan, P., Kotze, J. and Ferreira, J. (1983).<br/> Coronary risk factor screening in three rural communities, South African Medical Journal 64: 430–436.<br/>ElemStatLearn, R-Package
        /// </summary>
        /// <seealso cref="https://www.kaggle.com/datasets/yassinehamdaoui1/cardiovascular-disease"/>
        MaleCardiovascularDiseaseDataSet
    }
}
