using Kuvik.ONDC.Api.Services.Transaction;
using Microsoft.EntityFrameworkCore;
namespace Kuvik.ONDC.Api.Data;
public sealed class OndcDbContext(DbContextOptions<OndcDbContext> options) : DbContext(options) { public DbSet<OndcTransaction> Transactions => Set<OndcTransaction>(); protected override void OnModelCreating(ModelBuilder b) { b.Entity<OndcTransaction>().HasKey(x => x.TransactionId); b.Entity<OndcTransaction>().Property(x => x.TransactionId).HasMaxLength(100); b.Entity<OndcTransaction>().Property(x => x.MessageId).HasMaxLength(100); b.Entity<OndcTransaction>().Property(x => x.Action).HasMaxLength(40); b.Entity<OndcTransaction>().Property(x => x.PayloadJson).HasColumnType("LONGTEXT"); b.Entity<OndcTransaction>().HasIndex(x => x.MessageId).IsUnique(); } }
