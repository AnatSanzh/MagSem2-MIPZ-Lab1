using System;
using System.IO;
using System.Collections.Generic;

namespace MagSem2_MIPZ_Lab1
{
    static class InputOutputUtils
    {
        const int MAX_COUNTRYSET_COUNT = 1000000;

        public static List<List<CountrySettings>> ParseInput(TextReader reader)
        {
            var result = new List<List<CountrySettings>>();

            for (int i = 0; i < MAX_COUNTRYSET_COUNT; i++)
            {
                var currSetCountryCount = int.Parse(reader.ReadLine());
                if (currSetCountryCount == 0)
                {
                    break;
                }

                var countrySet = new List<CountrySettings>();
                ReadSetCountrySettings(reader, ref countrySet, currSetCountryCount);
                result.Add(countrySet);
            }

            return result;
        }

        static void ReadSetCountrySettings(TextReader reader, ref List<CountrySettings> countrySet, int countryCount)
        {
            for (int j = 0; j < countryCount; j++)
            {
                var segments = reader.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries
                    | StringSplitOptions.TrimEntries);

                countrySet.Add(new CountrySettings()
                {
                    Name = segments[0],
                    OccupiedArea = new Rect()
                    {
                        MinX = int.Parse(segments[1]),
                        MinY = int.Parse(segments[2]),
                        MaxX = int.Parse(segments[3]),
                        MaxY = int.Parse(segments[4])
                    }
                });
            }
        }

        public static void OutputResults(TextWriter writer, List<List<CountrySettings>> settings,
            List<List<Alghorithms.CountryResult>> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i] == null)
                {
                    writer.WriteLine($"Case Number {i + 1} contains invalid input!");
                }
                else
                {
                    OutputSetResults(i, writer, settings[i], results[i]);
                }
            }
        }

        static void OutputSetResults(int iteration, TextWriter writer, List<CountrySettings> setSettings, List<Alghorithms.CountryResult> setResults)
        {
            setResults.Sort(delegate (Alghorithms.CountryResult a, Alghorithms.CountryResult b)
            {
                if (a.IterationCount == b.IterationCount)
                    return setSettings[a.Index].Name.CompareTo(setSettings[b.Index].Name);
                return a.IterationCount.CompareTo(b.IterationCount);
            });

            writer.WriteLine($"Case Number {iteration + 1}");
            for (int j = 0; j < setResults.Count; j++)
            {
                writer.WriteLine($"{setSettings[setResults[j].Index].Name} {setResults[j].IterationCount}");
            }
        }
    }
}
