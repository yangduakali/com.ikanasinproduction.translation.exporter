using TranslationExporter;
using UnityEngine;

namespace cisini.TranslationExporter
{
    public abstract class CisiniExporterProcessor : ExporterProcessor
    {
        /// <summary>
        /// Creates a <see cref="LocalizeString"/> from an array of 2 localized strings.
        /// Order: [0] Indonesian, [1] English.
        /// </summary>
        /// <param name="array">An array of localized strings (must be exactly 8 elements).</param>
        /// <param name="asset">Optional, to display asset on Diff View</param>
        /// <returns>A <see cref="LocalizeString"/> containing the values from the array, or an empty one if invalid.</returns>
        protected LocalizeString GetLocalizeStringFromStringArray(string[] array, Object asset = null)
        {
            if (array.Length < 2)
            {
                return new LocalizeString
                {
                    Asset = asset
                };
            }

            return new LocalizeString
            {
                Asset = asset,
                Indonesian = array[0],
                English = array[1],
            };
        }

        protected int GetLocalizeIndexFromColumnKey(object columnKey)
        {
            return $"{columnKey}" switch
            {
                "Indonesian(id)" => 0,
                "English(en)" => 1,
                _ => 0
            };
        }

    }
}