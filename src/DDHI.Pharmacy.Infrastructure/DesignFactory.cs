using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace DDHI.Pharmacy.Infrastructure;
public sealed class DesignFactory:IDesignTimeDbContextFactory<PharmacyDb>{public PharmacyDb CreateDbContext(string[] args)=>new(new DbContextOptionsBuilder<PharmacyDb>().UseNpgsql(Environment.GetEnvironmentVariable("DDHI_TEST_CONNECTION")??"Host=localhost;Database=ddhi_design;Username=ddhi").Options);}
