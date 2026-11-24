namespace DiGi.GIS.PostgreSQL.Classes
{
    /// <summary>
    /// The source file an open building import is reading right now: its path, its 1-based position among the run's source files and how many files the run holds (DiGi.GIS.PostgreSQL#169).
    /// <para>One instance travels both ways along the run - the task's footprint stream fills it in as each file starts, and the same instance is handed to <see cref="OpenBuildingPostgreSQLConverter"/>'s <c>ImportAsync</c>, which reads it when it composes a progress line. The loop therefore names the file it is inside without the stream telling it per footprint.</para>
    /// <para>Writer and reader run on the same enumeration: the position is set by the enumerator the import loop is pulling, so the fields are never touched concurrently and need no synchronisation. Before the first file starts the position is empty - null path, index 0, count 0 - and a progress line reads "(starting)" then.</para>
    /// </summary>
    public class OpenBuildingSourcePosition
    {
        /// <summary>
        /// Gets or sets the full path of the source file being enumerated, or null before the first file starts.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the 1-based position of that file among the run's source files, or 0 before the first file starts.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the number of source files the run reads, or 0 before the stream starts being enumerated.
        /// </summary>
        public int Count { get; set; }
    }
}
