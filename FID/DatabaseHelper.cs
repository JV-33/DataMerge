using Microsoft.EntityFrameworkCore;
using FID.Models;

namespace FID
{
    public static class DatabaseHelper
    {
        public static readonly string DatabasePath = Path.Combine(Program.BaseDirectory, "FID", "Data.db");



        public static void CreateDatabaseAndTable()
        {
            using (var context = new ApplicationDbContext())
            {
                context.Database.Migrate();
            }
        }

        public static void InsertDataIntoDatabase(List<MergedData> combinedData)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        
                        context.CombinedData.AddRange(combinedData);
                        context.SaveChanges();
                        transaction.Commit();
                        Console.WriteLine("Combined data successfully inserted into the database.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occurred while inserting combined data into the database: " + ex.Message);
                    }
                }
            }
        }

        public static void InsertUnmatchedCompaniesIntoDatabase(List<UnmatchedCompany> unmatchedCompanies)
        {
            using (var context = new ApplicationDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.UnmatchedCompanies.AddRange(unmatchedCompanies);
                        context.SaveChanges();
                        transaction.Commit();
                        Console.WriteLine("Unmatched companies successfully inserted into the database.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("An error occurred while inserting unmatched companies into the database: " + ex.Message);
                    }
                }
            }
        }

    }

    public class ApplicationDbContext : DbContext
    {
        public DbSet<MergedData> CombinedData { get; set; }
        public DbSet<UnmatchedCompany> UnmatchedCompanies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseHelper.DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}

