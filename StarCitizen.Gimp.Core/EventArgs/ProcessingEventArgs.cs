namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessingEventArgs
    {
        public ScGimpProcessType ProcessType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ProcessingEventArgs(ScGimpProcessType processType)
        {
            ProcessType = processType;
        }
    }
}
