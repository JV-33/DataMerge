using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FID.Models;

namespace FID
{
    public static class DataMerger
    {
        public static async Task<(List<MergedData>, List<Company>)> MergeCompaniesWithMunicipalities(List<Company> companies)
        {
            var combinedDataList = new ConcurrentBag<MergedData>();
            var unmatchedCompanies = new ConcurrentBag<Company>();

            await Parallel.ForEachAsync(companies, (company, cancellationToken) =>
            {
                var data = FindMunicipalityData(company);

                if (data != null)
                {
                    combinedDataList.Add(data);
                }
                else
                {
                    unmatchedCompanies.Add(company);
                }

                return ValueTask.CompletedTask;
            });

            return (combinedDataList.ToList(), unmatchedCompanies.ToList());
        }

        public static MergedData? FindMunicipalityData(Company? company)
        {
            if (company == null)
            {
                return null;
            }

            var addressFirstPart = company.Address?.Split(',')[0].Trim();

            if (string.IsNullOrEmpty(addressFirstPart))
            {
                return null;
            }

            var municipalityData = MunicipalityData.NovadiUnPilsetas;

            foreach (var kvp in municipalityData)
            {
                var municipality = kvp.Key;
                var municipalityAtvkAndCities = kvp.Value;
                var municipalityCities = municipalityAtvkAndCities.Value;

                Func<string, bool> isMatch = city =>
                {

                    var pattern = Regex.Escape(city);
                    var cityRegex = new Regex(pattern, RegexOptions.IgnoreCase); // Using IgnoreCase for case-insensitive matching
                    return cityRegex.IsMatch(addressFirstPart);
                };

                if (company.Address != null && (municipalityCities.Any(isMatch) || isMatch(municipality)))
                {
                    return new MergedData
                    {
                        CompanyRegCode = company.RegCode,
                        CompanyName = company.Name,
                        CompanyAddress = company.Address,
                        MunicipalityDistrictAtvk = municipalityAtvkAndCities.Key,
                        MunicipalityDistrictName = municipality
                    };
                    break;
                }
            }
            
            return null;
        }
    }
}