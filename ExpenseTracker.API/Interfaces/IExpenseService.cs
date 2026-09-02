using ExpenseTracker.API.DTOs;

namespace ExpenseTracker.API.Interfaces;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(ExpenseDto expenseDto, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, ExpenseDto expenseDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
