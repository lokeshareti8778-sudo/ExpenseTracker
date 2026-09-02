using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Data;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(expense => expense.Title).HasMaxLength(200).IsRequired();
            entity.Property(expense => expense.Amount).HasColumnType("decimal(18,2)");
            entity.Property(expense => expense.Category).HasMaxLength(100).IsRequired();
            entity.Property(expense => expense.Notes).HasMaxLength(1000);
        });
    }
}
