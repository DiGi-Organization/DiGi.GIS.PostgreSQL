using System.ComponentModel;

namespace DiGi.GIS.PostgreSQL.Enums
{
    /// <summary>
    /// Names how a footprint of an open building dataset relates to the BDOT10k footprints it overlaps: the class of the connected component of the overlap graph it belongs to (DiGi.GIS.PostgreSQL#127).
    /// <para>The values are stored as integers in <c>building_dataset_match.match_class</c>, so they are explicit and never renumbered.</para>
    /// </summary>
    [Description("OpenBuildingMatchClass")]
    public enum OpenBuildingMatchClass
    {
        /// <summary>
        /// Not classified.
        /// </summary>
        [Description("Undefined")] Undefined = 0,

        /// <summary>
        /// One dataset footprint and one BDOT10k footprint whose intersection over union reaches the threshold: the pair the geometry measures (IoU, area ratio, offset) are computed over.
        /// </summary>
        [Description("One To One")] OneToOne = 1,

        /// <summary>
        /// One dataset footprint and one BDOT10k footprint joined by an edge but with an intersection over union below the threshold. Counted as matched for completeness and left out of the geometry measures.
        /// </summary>
        [Description("One To One Weak")] OneToOneWeak = 2,

        /// <summary>
        /// One dataset footprint and several BDOT10k footprints: the dataset merges buildings the register keeps apart.
        /// </summary>
        [Description("One To Many")] OneToMany = 3,

        /// <summary>
        /// Several dataset footprints and one BDOT10k footprint: the dataset splits a building the register keeps whole.
        /// </summary>
        [Description("Many To One")] ManyToOne = 4,

        /// <summary>
        /// Several dataset footprints and several BDOT10k footprints connected through their overlaps.
        /// </summary>
        [Description("Many To Many")] ManyToMany = 5,

        /// <summary>
        /// A BDOT10k footprint no dataset footprint overlaps enough: missing from the dataset.
        /// </summary>
        [Description("Omission")] Omission = 6,

        /// <summary>
        /// A dataset footprint no BDOT10k footprint overlaps enough: present in the dataset only.
        /// </summary>
        [Description("Commission")] Commission = 7,

        /// <summary>
        /// A footprint smaller than the run's minimum area, on either side: left out of the matching and recorded so the snapshot still accounts for it.
        /// </summary>
        [Description("Excluded")] Excluded = 8,
    }
}
