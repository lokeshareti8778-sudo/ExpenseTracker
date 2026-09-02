const apiUrl = '/api/expenses';
const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
let expenses = [];
let activeCategory = 'all';

const elements = {
  list: document.querySelector('#expense-list'), empty: document.querySelector('#empty-state'),
  search: document.querySelector('#search-input'), filters: document.querySelector('#category-filters'),
  breakdown: document.querySelector('#category-breakdown'), modal: document.querySelector('#modal-backdrop'),
  form: document.querySelector('#expense-form'), error: document.querySelector('#form-error'),
  modalTitle: document.querySelector('#modal-title'), submitLabel: document.querySelector('#submit-label'), toast: document.querySelector('#toast')
};

const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#039;', '"': '&quot;' }[character]));
const dateValue = value => new Date(value).toISOString().slice(0, 10);
const displayDate = value => new Date(value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

async function request(url = apiUrl, options) {
  const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
  if (!response.ok) throw new Error(`Request failed (${response.status})`);
  return response.status === 204 ? null : response.json();
}

function render() {
  const query = elements.search.value.trim().toLowerCase();
  const visible = expenses.filter(expense => activeCategory === 'all' || expense.category === activeCategory)
    .filter(expense => `${expense.title} ${expense.category} ${expense.notes ?? ''}`.toLowerCase().includes(query))
    .sort((first, second) => new Date(second.expenseDate) - new Date(first.expenseDate));
  elements.list.innerHTML = visible.map(expense => `<tr>
    <td><div class="expense-name"><span class="expense-dot"></span><div><strong>${escapeHtml(expense.title)}</strong><small>${escapeHtml(expense.notes || 'No note added')}</small></div></div></td>
    <td><span class="category-pill">${escapeHtml(expense.category)}</span></td>
    <td class="muted">${displayDate(expense.expenseDate)}</td>
    <td class="amount-column"><strong>${currency.format(expense.amount)}</strong></td>
    <td><div class="row-actions"><button class="icon-button" data-edit="${expense.id}" type="button" aria-label="Edit ${escapeHtml(expense.title)}">✎</button><button class="icon-button danger" data-delete="${expense.id}" type="button" aria-label="Delete ${escapeHtml(expense.title)}">⌫</button></div></td>
  </tr>`).join('');
  elements.empty.hidden = visible.length > 0;
  renderStats(); renderCategories();
}

function renderStats() {
  const total = expenses.reduce((sum, expense) => sum + Number(expense.amount), 0);
  const now = new Date();
  const monthTotal = expenses.filter(expense => { const date = new Date(expense.expenseDate); return date.getMonth() === now.getMonth() && date.getFullYear() === now.getFullYear(); }).reduce((sum, expense) => sum + Number(expense.amount), 0);
  const grouped = groupByCategory();
  const top = Object.entries(grouped).sort((first, second) => second[1] - first[1])[0];
  document.querySelector('#total-amount').textContent = currency.format(total);
  document.querySelector('#expense-count').textContent = `${expenses.length} expense${expenses.length === 1 ? '' : 's'}`;
  document.querySelector('#month-amount').textContent = currency.format(monthTotal);
  document.querySelector('#month-label').textContent = now.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  document.querySelector('#top-category').textContent = top?.[0] || '—';
  document.querySelector('#top-category-amount').textContent = top ? `${currency.format(top[1])} spent` : 'No expenses yet';
}

function groupByCategory() { return expenses.reduce((groups, expense) => { groups[expense.category] = (groups[expense.category] || 0) + Number(expense.amount); return groups; }, {}); }
function renderCategories() {
  const grouped = groupByCategory(); const total = Object.values(grouped).reduce((sum, amount) => sum + amount, 0) || 1;
  elements.filters.innerHTML = `<button class="filter ${activeCategory === 'all' ? 'active' : ''}" data-category="all" type="button">All</button>${Object.keys(grouped).sort().map(category => `<button class="filter ${activeCategory === category ? 'active' : ''}" data-category="${escapeHtml(category)}" type="button">${escapeHtml(category)}</button>`).join('')}`;
  const colors = ['#ef6f61', '#2f8f83', '#e9b949', '#6786b9', '#a4779b'];
  elements.breakdown.innerHTML = Object.entries(grouped).sort((first, second) => second[1] - first[1]).map(([category, amount], index) => `<div class="breakdown-row"><div class="breakdown-label"><span class="legend-dot" style="background:${colors[index % colors.length]}"></span><span>${escapeHtml(category)}</span><strong>${currency.format(amount)}</strong></div><div class="progress-track"><span style="width:${amount / total * 100}%;background:${colors[index % colors.length]}"></span></div></div>`).join('') || '<p class="muted">Your category breakdown will appear here.</p>';
}

function openModal(expense) {
  elements.form.reset(); elements.error.hidden = true;
  document.querySelector('#expense-id').value = expense?.id || '';
  document.querySelector('#title').value = expense?.title || '';
  document.querySelector('#amount').value = expense?.amount || '';
  document.querySelector('#category').value = expense?.category || '';
  document.querySelector('#expense-date').value = expense ? dateValue(expense.expenseDate) : dateValue(new Date());
  document.querySelector('#notes').value = expense?.notes || '';
  elements.modalTitle.textContent = expense ? 'Edit expense' : 'Add expense';
  elements.submitLabel.textContent = expense ? 'Update expense' : 'Save expense';
  elements.modal.hidden = false; document.querySelector('#title').focus();
}
function closeModal() { elements.modal.hidden = true; }
function showToast(message) { elements.toast.textContent = message; elements.toast.classList.add('show'); setTimeout(() => elements.toast.classList.remove('show'), 2600); }

async function loadExpenses() { try { expenses = await request(); render(); } catch (error) { elements.empty.hidden = false; elements.empty.querySelector('h3').textContent = 'Could not load expenses'; elements.empty.querySelector('p').textContent = 'Check the API connection and try again.'; } }

elements.form.addEventListener('submit', async event => {
  event.preventDefault(); const id = document.querySelector('#expense-id').value;
  const payload = { id: id ? Number(id) : 0, title: document.querySelector('#title').value.trim(), amount: Number(document.querySelector('#amount').value), category: document.querySelector('#category').value.trim(), expenseDate: document.querySelector('#expense-date').value, notes: document.querySelector('#notes').value.trim() || null };
  try { await request(id ? `${apiUrl}/${id}` : apiUrl, { method: id ? 'PUT' : 'POST', body: JSON.stringify(payload) }); closeModal(); await loadExpenses(); showToast(id ? 'Expense updated' : 'Expense added'); } catch { elements.error.textContent = 'Could not save this expense. Please try again.'; elements.error.hidden = false; }
});

document.addEventListener('click', async event => {
  const editId = event.target.closest('[data-edit]')?.dataset.edit; const deleteId = event.target.closest('[data-delete]')?.dataset.delete;
  if (editId) openModal(expenses.find(expense => expense.id === Number(editId)));
  if (deleteId && confirm('Delete this expense?')) { try { await request(`${apiUrl}/${deleteId}`, { method: 'DELETE' }); await loadExpenses(); showToast('Expense deleted'); } catch { showToast('Could not delete expense'); } }
  const category = event.target.closest('[data-category]')?.dataset.category; if (category) { activeCategory = category; render(); }
});
document.querySelector('#new-expense-button').addEventListener('click', () => openModal());
document.querySelector('#empty-add-button').addEventListener('click', () => openModal());
document.querySelector('#close-modal').addEventListener('click', closeModal);
elements.modal.addEventListener('click', event => { if (event.target === elements.modal) closeModal(); });
elements.search.addEventListener('input', render);
document.querySelector('#today-label').textContent = new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'short', day: 'numeric' });
loadExpenses();
