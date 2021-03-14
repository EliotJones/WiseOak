namespace UglyToad.WiseOak
{
    public enum FeatureType
    {
        /// <summary>
        /// The feature contains categories representing characteristics of the model.
        /// </summary>
        Categorical = 1,
        /// <summary>
        /// The feature contains numerical data which is continuous.
        /// </summary>
        Continuous = 2,
        /// <summary>
        /// The feature contains numerical data which is discrete.
        /// </summary>
        Discrete = 3,
        /// <summary>
        /// The feature contains text data.
        /// </summary>
        Text = 4
    }
}
