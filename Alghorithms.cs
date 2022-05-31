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
            if (IsInputValid(countrySettings) || !IsCountrySettingsValid(countrySettings))
            {
                return null;
            }

            Rect globalBoundingRect = countrySettings[0].OccupiedArea;
            for (int i = 1; i < countrySettings.Count; i++)
            {
                globalBoundingRect = Rect.GetBoundingRect(globalBoundingRect, countrySettings[i].OccupiedArea);
            }
            int globalHeight = globalBoundingRect.Height + 1;
            int globalWidth = globalBoundingRect.Width + 1;

            // init world grid
            CityState[,] currWorld = new CityState[globalHeight, globalWidth],
                nextWorld = new CityState[globalHeight, globalWidth];
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
                        currWorld[yI, xI].Coins = new int[countrySettings.Count];
                        nextWorld[yI, xI].Coins = new int[countrySettings.Count];

                        currWorld[yI, xI].Coins[i] = INITIAL_COUNTRY_COIN_COUNT;
                    }
                }
            }

            List<int> uncompletedCountries = new List<int>(countrySettings.Count);
            for (int i = 0; i < countrySettings.Count; i++)
            {
                uncompletedCountries.Add(i);
            }

            List<CountryResult> results = new List<CountryResult>(countrySettings.Count);

            int worldIteration = 0;
            while(uncompletedCountries.Count > 0)
            {
                // check distribution
                for (int i = uncompletedCountries.Count - 1; i >= 0; i--)
                {
                    var currCountryIndex = uncompletedCountries[i];
                    var currCountryArea = countrySettings[currCountryIndex].OccupiedArea;

                    var startX = currCountryArea.MinX - globalBoundingRect.MinX;
                    var startY = currCountryArea.MinY - globalBoundingRect.MinY;
                    var endX = currCountryArea.MaxX - globalBoundingRect.MinX;
                    var endY = currCountryArea.MaxY - globalBoundingRect.MinY;

                    bool isCompleted = true;

                    for (int yI = startY; yI <= endY && isCompleted; yI++)
                    {
                        for (int xI = startX; xI <= endX && isCompleted; xI++)
                        {
                            var currCity = currWorld[yI, xI];

                            for (int cI = 0; cI < currCity.Coins.Length && isCompleted; cI++)
                            {
                                isCompleted &= currCity.Coins[cI] > 0;
                            }
                        }
                    }

                    if (isCompleted)
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

        public static bool IsInputValid(List<CountrySettings> countrySettings)
        {
            if (countrySettings.Count > MAX_COUNTRY_COUNT)
                return false;

            for (int i = 0; i < countrySettings.Count; i++)
            {
                var currSetting = countrySettings[i];

                if (currSetting.Name.Length > MAX_NAME_LENGTH ||
                    currSetting.OccupiedArea.MinY < MIN_AREA_COORD || currSetting.OccupiedArea.MinY > MAX_AREA_COORD ||
                    currSetting.OccupiedArea.MinX < MIN_AREA_COORD || currSetting.OccupiedArea.MinX > MAX_AREA_COORD ||
                    currSetting.OccupiedArea.MaxY < MIN_AREA_COORD || currSetting.OccupiedArea.MaxY > MAX_AREA_COORD ||
                    currSetting.OccupiedArea.MaxX < MIN_AREA_COORD || currSetting.OccupiedArea.MaxX > MAX_AREA_COORD)
                    return false;
            }

            return true;
        }

        public static bool IsCountrySettingsValid(List<CountrySettings> countrySettings)
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
    }
}
