using DiGi.GIS.PostgreSQL.Enums;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Gets the leading slice of an administrative code that names its ancestor at the given administrative areal type.
        /// <para>A code carries the whole chain above it: the first 2 characters name the voivodeship, the first 4 the county, and the first 6 the municipality. The 7th character of a municipality code is the gmina <i>type</i> digit - a town and the rural area of one urban-rural gmina carry <c>4</c> and <c>5</c> against the gmina's own <c>3</c> - so the municipality slice is 6 characters, not the whole code. Verified against the stored table: every one of the 100 354 subdivision rows carries a 7-character code whose 4-character prefix is an existing county code, and 99.72% of the rows that already resolved agree with their municipality at 6 characters.</para>
        /// <para>Returns <see langword="null"/> for <see cref="AdministrativeArealType.Country"/>, which has no such relation - every country row's code is <c>10</c>, which is also the voivodeship code of łódzkie, so slicing 2 characters there would match one voivodeship's chain rather than the country. A caller that gets <see langword="null"/> has no code constraint to apply and must fall back to searching the whole level.</para>
        /// <para><b>A code is not a key.</b> 18 county codes and 64 municipality codes name several rows, one per polygon part, so a slice identifies a <i>set</i> of candidate rows and geometry still has to choose between them. See https://github.com/DiGi-Organization/DiGi.GIS.PostgreSQL/issues/1.</para>
        /// </summary>
        /// <param name="code">The administrative code to slice.</param>
        /// <param name="administrativeArealType">The administrative areal type of the ancestor being named.</param>
        /// <returns>The leading slice of <paramref name="code"/> naming the ancestor, or <see langword="null"/> when the type has no code relation or the code is too short to reach it.</returns>
        public static string? AdministrativeCodeKey(string? code, AdministrativeArealType administrativeArealType)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            int length = administrativeArealType switch
            {
                Enums.AdministrativeArealType.Voivodeship => 2,
                Enums.AdministrativeArealType.County => 4,
                Enums.AdministrativeArealType.Municipality => 6,
                _ => -1
            };

            if (length == -1 || code.Length < length)
            {
                return null;
            }

            return code[..length];
        }
    }
}
