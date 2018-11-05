namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessedEventArgs
    {
        public ScGimpProcessType ProcessType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ProcessedEventArgs(ScGimpProcessType processType)
        {
            ProcessType = processType;
        }
    }
}
