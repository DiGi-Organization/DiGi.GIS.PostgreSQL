using System;
using System.Collections.Generic;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Resolves the quadkey of every Microsoft source file of one run, so a county scope covered by several published chunks imports in a single run (DiGi-Organization/DiGi.GIS.PostgreSQL#132).
        /// <para>Each path's own <c>quadkey=&lt;digits&gt;</c> directory segment comes first (<see cref="DiGi.GIS.IO.Query.Quadkey(string?)"/>), and <paramref name="quadkey"/> stands in only for a path that has none. The reader synthesizes source identifiers as <c>quadkey:index</c> with the index restarting in every file, so the resolution is refused - before the run truncates anything - when a path resolves to no quadkey, when a path's segment disagrees with an explicit <paramref name="quadkey"/>, or when two files resolve to the same quadkey and their identifiers would collide.</para>
        /// <para>Both the core task and the tray's options check ask this one rule, so the dialog refuses what the run would refuse.</para>
        /// </summary>
        /// <param name="paths">The run's source paths, or null.</param>
        /// <param name="quadkey">The quadkey typed for the run, used only for a path without a segment, or null.</param>
        /// <param name="quadkeys">The quadkey of every path, keyed by the path, or null when the resolution was refused.</param>
        /// <param name="message">Why the resolution was refused, or null when it succeeded.</param>
        /// <returns>True when every path resolved to its own quadkey.</returns>
        public static bool TryMicrosoftQuadkeys(IEnumerable<string>? paths, string? quadkey, out Dictionary<string, string>? quadkeys, out string? message)
        {
            quadkeys = null;
            message = null;

            if (paths is null)
            {
                message = "No source file was named.";
                return false;
            }

            string? quadkey_Explicit = string.IsNullOrWhiteSpace(quadkey) ? null : quadkey!.Trim();

            Dictionary<string, string> quadkeys_Path = [];
            Dictionary<string, string> paths_Quadkey = new(StringComparer.Ordinal);

            foreach (string path in paths)
            {
                string? quadkey_Segment = DiGi.GIS.IO.Query.Quadkey(path);

                if (quadkey_Segment is not null && quadkey_Explicit is not null && !string.Equals(quadkey_Segment, quadkey_Explicit, StringComparison.Ordinal))
                {
                    message = $"The file sits under quadkey {quadkey_Segment}, but the run names quadkey {quadkey_Explicit}: {path}";
                    return false;
                }

                string? quadkey_Path = quadkey_Segment ?? quadkey_Explicit;
                if (quadkey_Path is null)
                {
                    message = $"The Microsoft file carries no quadkey=<digits> folder and the run names no quadkey: {path}";
                    return false;
                }

                if (paths_Quadkey.TryGetValue(quadkey_Path, out string? path_Other))
                {
                    message = $"Two files resolve to quadkey {quadkey_Path}, and their source identifiers would collide: {path_Other} and {path}";
                    return false;
                }

                paths_Quadkey[quadkey_Path] = path;
                quadkeys_Path[path] = quadkey_Path;
            }

            if (quadkeys_Path.Count == 0)
            {
                message = "No source file was named.";
                return false;
            }

            quadkeys = quadkeys_Path;
            return true;
        }
    }
}
