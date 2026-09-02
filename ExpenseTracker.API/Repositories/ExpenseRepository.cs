using ExpenseTracker.API.Data;
using ExpenseTracker.API.Interfaces;
using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Repositories;

public class ExpenseRepository(ExpenseDbContext context) : IExpenseRepository
{
    public async Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Expenses.AsNoTracking().OrderByDescending(expense => expense.ExpenseDate).ToListAsync(cancellationToken);

    public Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);

    public async Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        context.Expenses.Add(expense);
        await context.SaveChangesAsync(cancellationToken);
        return expense;
    }

    public async Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        context.Expenses.Update(expense);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await context.Expenses.FindAsync([id], cancellationToken);
        if (expense is null) return false;

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
