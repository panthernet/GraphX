namespace GraphX.PCL.Common.Interfaces
{
	public interface ILayoutParameters : IAlgorithmParameters
	{
        /// <summary>
        /// Seed to be used to initialize any random number generators in order to construct
        /// more deterministic output.
        /// </summary>
        int Seed { get; set; }
	}
}