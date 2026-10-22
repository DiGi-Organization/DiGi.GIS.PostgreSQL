using DiGi.GIS.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiGi.GIS.PostgreSQL
{
    public static partial class Query
    {
        /// <summary>
        /// Normalises free text for the address search: the one form both the stored <c>search_text</c> column of <see cref="Constants.TableName.AddressPoint"/> and every search query are reduced to, so the two can never diverge.
        /// <para>Steps: lower-cased with the invariant culture; <c>ł</c> mapped to <c>l</c> (it has no canonical decomposition, so <see cref="NormalizationForm.FormD"/> leaves it alone); diacritics removed by decomposing and dropping the non-spacing marks; every character that is neither a letter nor a digit turned into a space; the street-type tokens <c>ul</c>, <c>al</c>, <c>pl</c>, <c>os</c> and <c>rondo</c> dropped; whitespace collapsed. Only whole tokens are dropped, so <c>ulica</c> or <c>plac</c> stay.</para>
        /// <para>This replaces <c>unaccent()</c> on the database side, which is not <c>IMMUTABLE</c> and so cannot back an index. The result does not depend on <see cref="CultureInfo.CurrentCulture"/>.</para>
        /// </summary>
        /// <param name="text">The text to normalise.</param>
        /// <returns>The normalised text, or null when <paramref name="text"/> is null or holds no letter or digit. When the text holds nothing but street-type tokens they are kept, so a street named <c>Rondo</c> is still searchable.</returns>
        public static string? SearchText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string text_Decomposed = text.ToLowerInvariant().Replace('ł', 'l').Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new(text_Decomposed.Length);
            foreach (char @char in text_Decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(@char) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                stringBuilder.Append(char.IsLetterOrDigit(@char) ? @char : ' ');
            }

            string[] tokens = stringBuilder.ToString().Normalize(NormalizationForm.FormC).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return null;
            }

            HashSet<string> tokens_StreetType = ["ul", "al", "pl", "os", "rondo"];

            List<string> tokens_Result = [];
            foreach (string token in tokens)
            {
                if (!tokens_StreetType.Contains(token))
                {
                    tokens_Result.Add(token);
                }
            }

            return tokens_Result.Count == 0 ? string.Join(' ', tokens) : string.Join(' ', tokens_Result);
        }

        /// <summary>
        /// Builds the normalised <c>search_text</c> of an address point: <c>"&lt;street&gt; &lt;locality&gt;"</c>, or the locality alone for a locality without streets, reduced by <see cref="SearchText(string?)"/>.
        /// </summary>
        /// <param name="addressPoint">The address point.</param>
        /// <returns>The normalised search text, or null when the address point is null or has neither a street nor a locality name.</returns>
        public static string? SearchText(this AddressPoint? addressPoint)
        {
            if (addressPoint is null)
            {
                return null;
            }

            string? streetName = addressPoint.StreetName;
            string? localityName = addressPoint.LocalityName;

            return SearchText(string.IsNullOrWhiteSpace(streetName) ? localityName : $"{streetName} {localityName}");
        }
    }
}
