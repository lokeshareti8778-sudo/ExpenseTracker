using ExpenseTracker.API.DTOs;
using ExpenseTracker.API.Interfaces;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Services;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Tests;

public class ExpenseServiceTests
{
    [Fact]
    public async Task CreateAsync_MapsAndReturnsCreatedExpense()
    {
        var repository = new FakeExpenseRepository();
        var service = new ExpenseService(repository, new TestLogger<ExpenseService>());
        var input = new ExpenseDto
        {
            Title = "Lunch",
            Amount = 12.50m,
            Category = "Food",
            ExpenseDate = new DateTime(2026, 9, 1),
            Notes = "Team lunch"
        };

        var result = await service.CreateAsync(input);

        Assert.Equal(1, result.Id);
        Assert.Equal(input.Title, result.Title);
        Assert.Equal(input.Amount, repository.Expenses.Single().Amount);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalseWhenExpenseDoesNotExist()
    {
        var service = new ExpenseService(new FakeExpenseRepository(), new TestLogger<ExpenseService>());

        var updated = await service.UpdateAsync(42, new ExpenseDto { Title = "Missing" });

        Assert.False(updated);
    }

    private sealed class FakeExpenseRepository : IExpenseRepository
    {
        public List<Expense> Expenses { get; } = [];

        public Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Expense>>(Expenses);

        public Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Expenses.SingleOrDefault(expense => expense.Id == id));

        public Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            expense.Id = Expenses.Count + 1;
            Expenses.Add(expense);
            return Task.FromResult(expense);
        }

        public Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var expense = Expenses.SingleOrDefault(item => item.Id == id);
            return Task.FromResult(expense is not null && Expenses.Remove(expense));
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
