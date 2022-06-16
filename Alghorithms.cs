using System.Collections.Generic;

namespace MagSem2_MIPZ_Lab1
{
    static class Alghorithms
    {
        const int INITIAL_COUNTRY_COIN_COUNT = 1000000;
        const int REPRESENTATIVE_PORTION_DIVIDER = 1000;

        const int MIN_AREA_COORD = 1;
        const int MAX_AREA_COORD = 10;
        const int MAX_NAME_LENGTH = 25;
        const int MAX_COUNTRY_COUNT = 20;

        static readonly (int dX, int dY)[] CELL_SIDES = new[] { (1,0), (-1,0), (0,1), (0,-1) };

        public struct CountryResult
        {
            public int Index;
            public int IterationCount;
        }

        struct CityState
        {
            public int[] Coins;
        } 

        public static List<CountryResult> SimulateEurodiffusion(List<CountrySettings> countrySettings)
        {
            if (!CheckCSInput(countrySettings))
            {
                return null;
            }

            Rect globalBoundingRect = GetGlobalBoundingRect(countrySettings);

            int globalHeight = globalBoundingRect.Height + 1;
            int globalWidth = globalBoundingRect.Width + 1;

            // init world grid
            CityState[,] currWorld = new CityState[globalHeight, globalWidth],
                nextWorld = new CityState[globalHeight, globalWidth];
            InitWorld(countrySettings, globalBoundingRect, ref currWorld, ref nextWorld);

            List<int> uncompletedCountries = new List<int>(countrySettings.Count);
            InitUncompletedCountriesList(countrySettings, ref uncompletedCountries);

            List<CountryResult> results = new List<CountryResult>(countrySettings.Count);

            int worldIteration = 0;
            while(uncompletedCountries.Count > 0)
            {
                // check distribution
                for (int i = uncompletedCountries.Count - 1; i >= 0; i--)
                {
                    var currCountryIndex = uncompletedCountries[i];
                    var currCountryArea = countrySettings[currCountryIndex].OccupiedArea;

                    if (IsCountryAreaCompleted(currCountryArea, globalBoundingRect, ref currWorld))
                    {
                        uncompletedCountries.RemoveAt(i);
                        results.Add(new CountryResult() { Index = currCountryIndex, IterationCount = worldIteration });
                    }
                }

                // copy coin values
                for (int yI = 0; yI < globalHeight; yI++)
                {
                    for (int xI = 0; xI < globalWidth; xI++)
                    {
                        CityState currCity = currWorld[yI, xI];

                        if (currCity.Coins != null)
                        {
                            for (int cI = 0; cI < currCity.Coins.Length; cI++)
                            {
                                nextWorld[yI, xI].Coins[cI] = currCity.Coins[cI];
                            }
                        }
                    }
                }

                // spread coins
                for (int yI = 0; yI < globalHeight; yI++)
                {
                    for (int xI = 0; xI < globalWidth; xI++)
                    {
                        int[] currCityCoins = currWorld[yI, xI].Coins;
                        int[] NWcurrCityCoins = nextWorld[yI, xI].Coins;

                        if (currCityCoins != null)
                        {
                            for (int cI = 0; cI < currCityCoins.Length; cI++)
                            {
                                int reprPortion = currCityCoins[cI] / REPRESENTATIVE_PORTION_DIVIDER;

                                if (reprPortion == 0)
                                {
                                    continue;
                                }

                                for (int sI = 0; sI < CELL_SIDES.Length; sI++)
                                {
                                    var newXI = CELL_SIDES[sI].dX + xI;
                                    var newYI = CELL_SIDES[sI].dY + yI;

                                    if (newXI >= 0 && newXI < globalWidth &&
                                        newYI >= 0 && newYI < globalHeight &&
                                        currWorld[newYI, newXI].Coins != null)
                                    {
                                        nextWorld[newYI, newXI].Coins[cI] += reprPortion;
                                        NWcurrCityCoins[cI] -= reprPortion;
                                    }
                                }
                            }
                        }
                    }
                }

                var temp = currWorld;
                currWorld = nextWorld;
                nextWorld = temp;
                worldIteration++;
            }

            return results;
        }

        static Rect GetGlobalBoundingRect(List<CountrySettings> countrySettings)
        {
            Rect globalBoundingRect = countrySettings[0].OccupiedArea;
            for (int i = 1; i < countrySettings.Count; i++)
            {
                globalBoundingRect = Rect.GetBoundingRect(globalBoundingRect, countrySettings[i].OccupiedArea);
            }
            return globalBoundingRect;
        }

        static void InitWorld(List<CountrySettings> countrySettings, Rect globalBoundingRect, ref CityState[,] currentWorld, ref CityState[,] nextWorld)
        {
            for (int i = 0; i < countrySettings.Count; i++)
            {
                int startX = countrySettings[i].OccupiedArea.MinX - globalBoundingRect.MinX;
                int startY = countrySettings[i].OccupiedArea.MinY - globalBoundingRect.MinY;
                int endX = countrySettings[i].OccupiedArea.MaxX - globalBoundingRect.MinX;
                int endY = countrySettings[i].OccupiedArea.MaxY - globalBoundingRect.MinY;

                for (int yI = startY; yI <= endY; yI++)
                {
                    for (int xI = startX; xI <= endX; xI++)
                    {
                        currentWorld[yI, xI].Coins = new int[countrySettings.Count];
                        nextWorld[yI, xI].Coins = new int[countrySettings.Count];

                        currentWorld[yI, xI].Coins[i] = INITIAL_COUNTRY_COIN_COUNT;
                    }
                }
            }
        }

        static void InitUncompletedCountriesList(List<CountrySettings> countrySettings, ref List<int> uncompletedCountryList)
        {
            for (int i = 0; i < countrySettings.Count; i++)
            {
                uncompletedCountryList.Add(i);
            }
        }

        static bool IsCountryAreaCompleted(Rect currCountryArea, Rect globalBoundingRect, ref CityState[,] currentWorld)
        {
            var startX = currCountryArea.MinX - globalBoundingRect.MinX;
            var startY = currCountryArea.MinY - globalBoundingRect.MinY;
            var endX = currCountryArea.MaxX - globalBoundingRect.MinX;
            var endY = currCountryArea.MaxY - globalBoundingRect.MinY;


            for (int yI = startY; yI <= endY; yI++)
            {
                for (int xI = startX; xI <= endX; xI++)
                {
                    var currCity = currentWorld[yI, xI];

                    for (int cI = 0; cI < currCity.Coins.Length; cI++)
                    {
                        if(currCity.Coins[cI] == 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        static bool IsInputValid(List<CountrySettings> countrySettings)
        {
            if (countrySettings.Count > MAX_COUNTRY_COUNT)
                return false;

            for (int i = 0; i < countrySettings.Count; i++)
            {
                if (CheckIndividualCountrySetting(countrySettings[i]))
                    return false;
            }

            return true;
        }

        static bool CheckIndividualCountrySetting(CountrySettings countrySetting) => countrySetting.Name.Length > MAX_NAME_LENGTH ||
                    countrySetting.OccupiedArea.MinY < MIN_AREA_COORD || countrySetting.OccupiedArea.MinY > MAX_AREA_COORD ||
                    countrySetting.OccupiedArea.MinX < MIN_AREA_COORD || countrySetting.OccupiedArea.MinX > MAX_AREA_COORD ||
                    countrySetting.OccupiedArea.MaxY < MIN_AREA_COORD || countrySetting.OccupiedArea.MaxY > MAX_AREA_COORD ||
                    countrySetting.OccupiedArea.MaxX < MIN_AREA_COORD || countrySetting.OccupiedArea.MaxX > MAX_AREA_COORD;

        static bool IsCountrySettingsValid(List<CountrySettings> countrySettings)
        {
            var graph = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < countrySettings.Count; i++)
            {
                var neighbours = new HashSet<int>();
                var currContRect = countrySettings[i].OccupiedArea;

                for (int j = 0; j < countrySettings.Count; j++)
                {
                    if (i != j && Rect.IsTouching(currContRect, countrySettings[j].OccupiedArea))
                    {
                        neighbours.Add(j);
                    }
                }

                graph.Add(i, neighbours);
            }

            HashSet<int> visitedCountries = new HashSet<int>(countrySettings.Count);
            Queue<int> countriesToVisit = new Queue<int>();
            countriesToVisit.Enqueue(0);

            while (countriesToVisit.TryDequeue(out var currIndex))
            {
                visitedCountries.Add(currIndex);

                foreach (var nextIndex in graph[currIndex])
                {
                    if (!visitedCountries.Contains(nextIndex))
                    {
                        countriesToVisit.Enqueue(nextIndex);
                    }
                }
            }

            return visitedCountries.Count == countrySettings.Count;
        }

        static bool CheckCSInput(List<CountrySettings> countrySettings) =>
            IsInputValid(countrySettings) && IsCountrySettingsValid(countrySettings);


    }
}
