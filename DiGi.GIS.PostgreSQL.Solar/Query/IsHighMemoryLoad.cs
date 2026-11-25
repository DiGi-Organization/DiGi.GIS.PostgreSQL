namespace DiGi.GIS.PostgreSQL.Solar
{
    public static partial class Query
    {
        /// <summary>
        /// Determines whether the machine's memory load has reached the threshold at which the garbage collector itself treats memory as scarce - the point past which the batch solar radiation solve warns (DiGi.GIS.PostgreSQL#167).
        /// <para>Both figures come from <see cref="System.GCMemoryInfo"/> (<see cref="System.GCMemoryInfo.MemoryLoadBytes"/> and <see cref="System.GCMemoryInfo.HighMemoryLoadThresholdBytes"/>): the load is the physical memory in use on the whole machine, not by the process alone, so a starved pagefile is seen whatever holds the memory. The threshold is the runtime's own (90 % of physical memory by default, <c>GCHighMemoryPercent</c>), so the solve sets no number of its own.</para>
        /// </summary>
        /// <param name="memoryLoadBytes">The memory load of the machine, in bytes.</param>
        /// <param name="highMemoryLoadThresholdBytes">The high memory load threshold, in bytes. 0 or less - no collection has run yet, so the figures are unknown - is never a high load.</param>
        /// <returns>True when <paramref name="highMemoryLoadThresholdBytes"/> is positive and <paramref name="memoryLoadBytes"/> is at or above it; otherwise, false.</returns>
        public static bool IsHighMemoryLoad(long memoryLoadBytes, long highMemoryLoadThresholdBytes)
        {
            return highMemoryLoadThresholdBytes > 0 && memoryLoadBytes >= highMemoryLoadThresholdBytes;
        }
    }
}
