using ExpenseTracker.API.DTOs;
using ExpenseTracker.API.Interfaces;
using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Services;

public class ExpenseService(IExpenseRepository repository, ILogger<ExpenseService> logger) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all expenses");
        var expenses = await repository.GetAllAsync(cancellationToken);
        return expenses.Select(ToDto).ToList();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving expense {ExpenseId}", id);
        var expense = await repository.GetByIdAsync(id, cancellationToken);
        return expense is null ? null : ToDto(expense);
    }

    public async Task<ExpenseDto> CreateAsync(ExpenseDto expenseDto, CancellationToken cancellationToken = default)
    {
        var expense = await repository.AddAsync(ToModel(expenseDto), cancellationToken);
        logger.LogInformation("Created expense {ExpenseId}", expense.Id);
        return ToDto(expense);
    }

    public async Task<bool> UpdateAsync(int id, ExpenseDto expenseDto, CancellationToken cancellationToken = default)
    {
        var expense = await repository.GetByIdAsync(id, cancellationToken);
        if (expense is null) return false;

        expense.Title = expenseDto.Title;
        expense.Amount = expenseDto.Amount;
        expense.Category = expenseDto.Category;
        expense.ExpenseDate = expenseDto.ExpenseDate;
        expense.Notes = expenseDto.Notes;
        await repository.UpdateAsync(expense, cancellationToken);
        logger.LogInformation("Updated expense {ExpenseId}", id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (deleted) logger.LogInformation("Deleted expense {ExpenseId}", id);
        return deleted;
    }

    private static Expense ToModel(ExpenseDto dto) => new()
    {
        Title = dto.Title,
        Amount = dto.Amount,
        Category = dto.Category,
        ExpenseDate = dto.ExpenseDate,
        Notes = dto.Notes
    };

    private static ExpenseDto ToDto(Expense expense) => new()
    {
        Id = expense.Id,
        Title = expense.Title,
        Amount = expense.Amount,
        Category = expense.Category,
        ExpenseDate = expense.ExpenseDate,
        Notes = expense.Notes
    };
}
