<script setup lang="ts">
import { reactive } from 'vue';
import { calculateSettlement, type Stock } from './api/stockApi';
import AutocompleteStock from './api/components/AutocompleteStock.vue';

type Row = {
  id: number;
  symbol: string;
  market: string;
  rawDate: string;
  date: string;
  quantity: number | null;
  result: any | null;
  isLoading: boolean;
  error: string;
  lastRequestKey: string | null;
};

function createEmptyRow(): Row {
  return {
    id: Date.now() + Math.random(),
    symbol: '',
    market: 'TWSE',
    rawDate: '',
    date: '',
    quantity: null,
    result: null,
    isLoading: false,
    error: '',
    lastRequestKey: null,
  };
}

function parseDateInput(input: string): string | null {
  const digits = input.replace(/\D/g, '');
  if (digits.length === 8) {
    // 視為西元：yyyyMMdd
    const year = Number(digits.slice(0, 4));
    const month = Number(digits.slice(4, 6));
    const day = Number(digits.slice(6, 8));
    if (!isValidYmd(year, month, day)) return null;
    return `${year.toString().padStart(4, '0')}-${digits.slice(4, 6)}-${digits.slice(6, 8)}`;
  }

  if (digits.length === 7) {
    // 視為民國：yyyMMdd，例如 1140102
    const rocYear = Number(digits.slice(0, 3));
    const year = rocYear + 1911;
    const month = Number(digits.slice(3, 5));
    const day = Number(digits.slice(5, 7));
    if (!isValidYmd(year, month, day)) return null;
    return `${year.toString().padStart(4, '0')}-${digits.slice(3, 5)}-${digits.slice(5, 7)}`;
  }

  return null;
}

function isValidYmd(year: number, month: number, day: number): boolean {
  if (month < 1 || month > 12) return false;
  if (day < 1 || day > 31) return false;
  const dt = new Date(year, month - 1, day);
  return (
    dt.getFullYear() === year &&
    dt.getMonth() === month - 1 &&
    dt.getDate() === day
  );
}

function onDateChanged(row: Row) {
  row.error = '';
  const normalized = parseDateInput(row.rawDate);
  if (!normalized) {
    row.date = '';
    row.result = null;
    row.error = '日期格式錯誤，請輸入 20250101 或 1140102';
    return;
  }

  row.date = normalized; // 給 API 用
  autoCalculate(row);
}

const rows = reactive<Row[]>([createEmptyRow()]);

function addRow() {
  rows.push(createEmptyRow());
}

function removeRow(index: number) {
  rows.splice(index, 1);
}

function buildRequestKey(row: Row): string {
  return `${row.symbol}|${row.market}|${row.date}|${row.quantity}`;
}

function onStockSelected(row: Row, stock: Stock) {
  row.symbol = stock.symbol;
  row.market = stock.market;
  row.error = '';
  autoCalculate(row);
}

async function autoCalculate(row: Row) {
  row.error = '';
  row.result = null;

  // 必要欄位檢查（未填完就不要打 API）
  if (!row.symbol || !row.date || !row.quantity || row.quantity <= 0) {
    return;
  }

  const key = buildRequestKey(row);

  // 相同條件已算過就不再打 API
  if (row.lastRequestKey === key) {
    return;
  }

  row.lastRequestKey = key;
  row.isLoading = true;

  try {
    const res = await calculateSettlement({
      symbol: row.symbol,
      quantity: row.quantity,
      date: row.date,
      market: row.market,
    });
    row.result = res.data;
  } catch (e: any) {
    console.error(e);
    row.error = e?.response?.data ?? 'API 呼叫失敗';
    row.result = null;
  } finally {
    row.isLoading = false;
  }
}
</script>

<template>
  <div class="page">
    <header class="page-header">
      <h1>股票交割金額試算</h1>
      <p class="page-subtitle">
        選擇股票、日期與股數後，系統會自動取得收盤價並計算交割金額。
      </p>
    </header>

    <main class="content">
      <div class="table-wrapper">
        <table class="trade-table">
          <thead>
            <tr>
              <th>#</th>
              <th style="min-width: 220px;">股票（代號 / 名稱）</th>
              <th class="cell-center">市場</th>
              <th>交易日期</th>
              <th>股數</th>
              <th>收盤價</th>
              <th>交割金額</th>
              <th style="min-width: 160px;">狀態 / 刪除</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, index) in rows" :key="row.id">
              <!-- 序號 -->
              <td class="cell-center">
                {{ index + 1 }}
              </td>

              <!-- 股票 autocomplete -->
              <td>
                <AutocompleteStock class="stock-autocomplete" @select="stock => onStockSelected(row, stock)" />
                <div class="selected-info">
                  <span v-if="row.symbol">
                    已選擇：{{ row.symbol }}（{{ row.market }}）
                  </span>
                  <span v-else class="muted">尚未選擇股票</span>
                </div>
              </td>

              <!-- 市場（只展示） -->
              <td class="cell-center">
                <span class="badge">
                  {{ row.market || '—' }}
                </span>
              </td>

              <!-- 日期 -->
              <td>
                <input v-model="row.rawDate" type="text" class="input" @change="onDateChanged(row)" inputmode="numeric"
                  maxlength="8" />
              </td>

              <!-- 股數 -->
              <td>
                <input v-model.number="row.quantity" type="number" min="1" step="1" class="input" placeholder="例如：1000"
                  @change="autoCalculate(row)" />
              </td>

              <!-- 收盤價 -->
              <td class="cell-right">
                <span v-if="row.result && row.result.hasPriceData">
                  {{ row.result.closePrice }}
                </span>
                <span v-else class="muted">—</span>
              </td>

              <!-- 交割金額 -->
              <td class="cell-right">
                <span v-if="row.result && row.result.hasPriceData">
                  {{ row.result.totalAmount }}
                </span>
                <span v-else-if="row.result && !row.result.hasPriceData" class="muted">
                  無收盤價
                </span>
                <span v-else class="muted">—</span>
              </td>

              <!-- 狀態 + 刪除 -->
              <td>
                <div class="actions-cell">
                  <div class="status-text">
                    <span v-if="row.isLoading" class="muted">計算中…</span>
                    <span v-else-if="row.error" class="error-text">
                      {{ row.error }}
                    </span>
                    <span v-else-if="row.result && row.result.hasPriceData" class="ok-text">
                      已計算
                    </span>
                    <span v-else class="muted">
                      請輸入完整資料
                    </span>
                  </div>

                  <button v-if="rows.length > 1" type="button" class="btn btn-ghost" @click="removeRow(index)">
                    刪除
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="add-row">
        <button type="button" class="btn btn-outline" @click="addRow">
          ＋ 新增一筆交易
        </button>
      </div>
    </main>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  background: #f5f5f7;
  padding: 2rem 1rem;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.page-header {
  max-width: 1100px;
  margin: 0 auto 1.5rem;
}

.page-header h1 {
  font-size: 1.8rem;
  margin-bottom: 0.25rem;
}

.page-subtitle {
  color: #555;
  font-size: 0.95rem;
}

.content {
  max-width: 1100px;
  margin: 0 auto;
}

.table-wrapper {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(15, 23, 42, 0.06);
  padding: 1rem;
  /* overflow-x: auto;  // 已移除，避免裁掉下拉選單 */
}

.trade-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.trade-table thead {
  background: #f3f4f6;
}

.trade-table th,
.trade-table td {
  padding: 0.55rem 0.6rem;
  border-bottom: 1px solid #e5e7eb;
  vertical-align: top;
}

.trade-table th {
  text-align: left;
  font-weight: 600;
  font-size: 0.8rem;
  color: #6b7280;
}

th.cell-center,
td.cell-center {
  text-align: center;
}

.cell-right {
  text-align: right;
}

/* Autocomplete 下面的小提示 */
.selected-info {
  margin-top: 0.15rem;
  font-size: 0.75rem;
}

.muted {
  color: #9ca3af;
}

/* 市場 badge */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.5rem;
  border-radius: 999px;
  font-size: 0.75rem;
  background: #e5f0ff;
  color: #1d4ed8;
}

/* Inputs */
.input {
  width: 100%;
  border-radius: 8px;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
  outline: none;
  background-color: #f9fafb;
  transition: border-color 0.15s, box-shadow 0.15s, background-color 0.15s;
}

.input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 1px rgba(37, 99, 235, 0.3);
  background-color: #ffffff;
}

/* Buttons */
.btn {
  border-radius: 9999px;
  border: none;
  padding: 0.3rem 0.8rem;
  font-size: 0.8rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  transition: background-color 0.15s, color 0.15s, box-shadow 0.15s, border-color 0.15s;
}

.btn-outline {
  border-radius: 9999px;
  border: 1px solid #d1d5db;
  background: #fff;
  color: #374151;
}

.btn-outline:hover {
  border-color: #9ca3af;
  background: #f9fafb;
}

.btn-ghost {
  border: none;
  background: transparent;
  color: #6b7280;
  padding-inline: 0.3rem;
}

.btn-ghost:hover {
  background: #f3f4f6;
  color: #111827;
}

.actions-cell {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.25rem;
}

.status-text {
  font-size: 0.8rem;
}

.error-text {
  font-size: 0.8rem;
  color: #b91c1c;
}

.ok-text {
  font-size: 0.8rem;
  color: #16a34a;
}

/* 下方新增按鈕 */
.add-row {
  display: flex;
  justify-content: flex-end;
  margin-top: 0.75rem;
}

/* RWD: 窄螢幕時讓 table 可橫向捲動就好 */
@media (max-width: 768px) {
  .page {
    padding: 1rem 0.5rem;
  }

  .table-wrapper {
    padding: 0.5rem;
  }
}
</style>
