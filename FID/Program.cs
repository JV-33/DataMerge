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


                var companyData = ReadCompanyData("register.csv");

                var (combinedData, unmatchedCompanies) = DataCombiner.CombineData(companyData);

                List<UnmatchedCompany> convertedUnmatchedCompanies = unmatchedCompanies.Select(c => ConvertToUnmatchedCompany(c)).ToList();

            DatabaseHelper.InsertDataIntoDatabase(combinedData);

            DatabaseHelper.InsertUnmatchedCompaniesIntoDatabase(convertedUnmatchedCompanies);



            SaveCombinedData(combinedData);
                SaveUnmatchedCompanies(unmatchedCompanies);


                Console.WriteLine("Data processing completed.");
                Console.ReadLine();
        }

        private static List<Company> ReadCompanyData(string fileName)
        {
            var filePath = Path.Combine(BaseDirectory, "FID", fileName);


            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<CompanyMap>();
                return csv.GetRecords<Company>().ToList();
            }
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


        private static void SaveCombinedData(List<CombinedData> combinedData)
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

        public static class Logger
        {
            public static void LogError(string message, Exception ex)
            {
                Console.WriteLine($"Error: {message}, Exception: {ex.Message}");
            }

            public static void LogInfo(string message)
            {
                Console.WriteLine($"Info: {message}");
            }
        }

    }
} 