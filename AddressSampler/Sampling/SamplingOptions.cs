namespace AddressSampler.Sampling
{
    public class SamplingOptions
    {
        public int BatchSize { get; set; }
        public int SampleSize { get; set; }
        public string OutputPath { get; set; }
        public string DataPath { get; set; }
    }
}