using System.Collections.Concurrent;
using FID.Models;

namespace FID
{
    public static class DataCombiner
    {
        public static (List<CombinedData>, List<Company>) CombineData(List<Company> companies)
        {
            var combinedDataList = new ConcurrentBag<CombinedData>();
            var unmatchedCompanies = new ConcurrentBag<Company>();

            var municipalityData = MunicipalityData.NovadiUnPilsetas;

            foreach (var company in companies)
            {
                bool matched = false;
                foreach (var kvp in municipalityData)
                {
                    string municipality = kvp.Key;
                    Tuple<string, List<string>> atvkAndCities = kvp.Value;
                    List<string> cities = atvkAndCities.Item2;

                    if (company.Address != null && (cities.Any(city => company.Address.Contains(city)) || company.Address.Contains(municipality)))
                    {
                        combinedDataList.Add(new CombinedData
                        {
                            CompanyRegCode = company.RegCode,
                            CompanyName = company.Name,
                            CompanyAddress = company.Address,
                            MunicipalityDistrictAtvk = atvkAndCities.Item1,
                            MunicipalityDistrictName = municipality
                        });
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    unmatchedCompanies.Add(company);
                }
            }

            return (combinedDataList.ToList(), unmatchedCompanies.ToList());
        }
    }
}