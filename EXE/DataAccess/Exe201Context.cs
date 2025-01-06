using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Intrinsics.Arm;
using Assimp;
using Microsoft.EntityFrameworkCore;
using static NuGet.Packaging.PackagingConstants;

namespace EXE.DataAccess;

public partial class Exe201Context : DbContext
{
    public Exe201Context()
    {
    }

    public Exe201Context(DbContextOptions<Exe201Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ProjectOrder> ProjectOrders { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Material> Materials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //        var builder = new ConfigurationBuilder()
        //.SetBasePath(Directory.GetCurrentDirectory())
        //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //        IConfigurationRoot configuration = builder.Build();
        //        optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyStockDB"));
        // Chuỗi kết nối được tích hợp trực tiếp
        //var connectionstring = "server=desktop-dqgh4nr;database=exe201;user id=sa;password=sa;trustservercertificate=true;";
        var connectionstring = "Server =camel-rds.maychudns.net,1443;Database=gimme_id_vn_dbmain; User Id =gimme_id_vn_dbmain; Password = EXE201Gimme; TrustServerCertificate = True;";

        // Cấu hình DbContext để sử dụng SQL Server với chuỗi kết nối đã cung cấp
        optionsBuilder.UseSqlServer(connectionstring);

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public static implicit operator Exe201Context(ExeContext v)
    {
        throw new NotImplementedException();
    }
}
