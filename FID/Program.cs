using System.Diagnostics;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using FID.Models;

namespace FID
{
    class Program
    {
        public static readonly string BaseDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));



        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting data processing...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var companyData = ReadCompanyData("register.csv");
            stopwatch.Stop();
            Console.WriteLine($"reading companies took: {stopwatch.Elapsed.TotalSeconds}");
            stopwatch.Reset();
            stopwatch.Start();
            var (combinedData, unmatchedCompanies) = await DataMerger.MergeCompaniesWithMunicipalities(companyData);
            stopwatch.Stop();
            Console.WriteLine($"data merge took: {stopwatch.Elapsed.TotalSeconds}");
            stopwatch.Reset();
            
            List<UnmatchedCompany> convertedUnmatchedCompanies = unmatchedCompanies.Select(ConvertToUnmatchedCompany).ToList();
            stopwatch.Start();
            DatabaseHelper.InsertDataIntoDatabase(combinedData);
            stopwatch.Stop();
            Console.WriteLine($"databae merged data insert took: {stopwatch.Elapsed.TotalSeconds}");
            stopwatch.Reset();
            stopwatch.Start();
            DatabaseHelper.InsertUnmatchedCompaniesIntoDatabase(convertedUnmatchedCompanies);
            stopwatch.Stop();
            Console.WriteLine($"databae unmatched data insert took: {stopwatch.Elapsed.TotalSeconds}");
            stopwatch.Reset();
            stopwatch.Start();
            SaveCombinedData(combinedData);
            SaveUnmatchedCompanies(unmatchedCompanies);
            stopwatch.Stop();
            Console.WriteLine($"data saved to files took: {stopwatch.Elapsed.TotalSeconds}");
            
            Console.WriteLine("Data processing completed.");
            Console.ReadLine();
        }

        private static List<Company> ReadCompanyData(string fileName)
        {
            var filePath = Path.Combine(BaseDirectory, "FID", fileName);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null
            });
            csv.Context.RegisterClassMap<CompanyMap>();
            
            return csv.GetRecords<Company>().ToList();
        }

        private static UnmatchedCompany ConvertToUnmatchedCompany(Company company)
        {
            return new UnmatchedCompany
            {
                RegCode = company.RegCode,
                Name = company.Name,
                Address = company.Address
            };
        }

        private static void SaveCombinedData(List<MergedData> combinedData)
        {
            var filePath = Path.Combine(BaseDirectory, "FID", "combinedData.csv");


            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(combinedData);
            }
            var recordCount = combinedData.Count;
            Console.WriteLine($"Combined data saved to {filePath}");
            Console.WriteLine($"Total records in combinedData.csv: {recordCount}");
        }

        private static void SaveUnmatchedCompanies(List<Company> unmatchedCompanies)
        {
            var filePath = Path.Combine(BaseDirectory, "FID", "unmatched_companies.csv");


            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(unmatchedCompanies);
            }
            Console.WriteLine($"Unmatched companies data saved to {filePath}");
            Console.WriteLine($"Total unmatched company records: {unmatchedCompanies.Count}");
        }

        public sealed class CompanyMap : ClassMap<Company>
        {
            public CompanyMap()
            {
                Map(m => m.RegCode).Name("regcode");
                Map(m => m.Name).Name("name");
                Map(m => m.Address).Name("address");

            }
        }
    }
} 