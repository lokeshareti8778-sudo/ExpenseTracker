using ExpenseTracker.API.DTOs;
using ExpenseTracker.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController(IExpenseService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var expense = await service.GetByIdAsync(id, cancellationToken);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(ExpenseDto expenseDto, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(expenseDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ExpenseDto expenseDto, CancellationToken cancellationToken) =>
        await service.UpdateAsync(id, expenseDto, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
