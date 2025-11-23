<script setup lang="ts">
import { reactive } from 'vue';
import { calculateSettlement, Stock } from './api/stockApi';
import AutocompleteStock from './api/components/AutocompleteStock.vue';

type Row = {
  id: number;
  symbol: string;
  market: string;
  date: string;
  quantity: number | null;
  result: any | null;
  isLoading: boolean;
  error: string;
};

const rows = reactive<Row[]>([
  createEmptyRow(),
]);

function createEmptyRow(): Row {
  return {
    id: Date.now() + Math.random(),
    symbol: '',
    market: 'TWSE',
    date: '',
    quantity: null,
    result: null,
    isLoading: false,
    error: '',
  };
}

function addRow() {
  rows.push(createEmptyRow());
}

function removeRow(index: number) {
  rows.splice(index, 1);
}

function onStockSelected(row: Row, stock: Stock) {
  row.symbol = stock.symbol;
  row.market = stock.market; // 自動帶入市場
  row.error = '';
}

async function onCalculate(row: Row) {
  row.error = '';
  row.result = null;

  if (!row.symbol || !row.date || !row.quantity || row.quantity <= 0) {
    row.error = '請輸入完整資料';
    return;
  }

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
        輸入股票、日期與股數，系統會即時計算交割金額。
      </p>
    </header>

    <main class="content">
      <div v-for="(row, index) in rows" :key="row.id" class="trade-card">
        <div class="trade-card-header">
          <div class="trade-card-title">第 {{ index + 1 }} 筆交易</div>
          <button v-if="rows.length > 1" type="button" class="btn btn-ghost" @click="removeRow(index)">
            刪除
          </button>
        </div>

        <div class="trade-card-body">
          <!-- 左側：輸入欄位 -->
          <div class="trade-form">
            <div class="form-row form-row-full">
              <label class="form-label">股票（代號 / 名稱）</label>
              <!-- Autocomplete：選股票後會觸發 select 事件 -->
              <AutocompleteStock @select="stock => onStockSelected(row, stock)" />
            </div>

            <div class="form-row">
              <label class="form-label">交易日期</label>
              <input v-model="row.date" type="date" class="input" />
            </div>

            <div class="form-row">
              <label class="form-label">股數</label>
              <input v-model.number="row.quantity" type="number" min="1" step="1" class="input" placeholder="例如：1000" />
            </div>

            <div class="form-row form-row-full">
              <small class="hint">
                已選擇：
                <template v-if="row.symbol">
                  {{ row.symbol }}（市場：{{ row.market }}）
                </template>
                <template v-else>
                  尚未選擇股票
                </template>
              </small>
            </div>

            <div class="form-actions form-row-full">
              <button type="button" class="btn btn-primary" @click="onCalculate(row)" :disabled="row.isLoading">
                {{ row.isLoading ? '計算中…' : '計算' }}
              </button>
            </div>
          </div>

          <!-- 右側：結果區 -->
          <div class="trade-result">
            <div v-if="row.error" class="result-box error">
              {{ row.error }}
            </div>

            <div v-else-if="row.result" class="result-box success">
              <template v-if="row.result.hasPriceData">
                <div class="result-title">計算結果</div>

                <div class="result-item">
                  <span>股票代號</span>
                  <strong>{{ row.result.symbol }}</strong>
                </div>
                <div class="result-item">
                  <span>交易日期</span>
                  <strong>{{ row.result.tradeDate }}</strong>
                </div>
                <div class="result-item">
                  <span>收盤價</span>
                  <strong>{{ row.result.closePrice }}</strong>
                </div>
                <div class="result-item">
                  <span>股數</span>
                  <strong>{{ row.result.shares }}</strong>
                </div>
                <div class="result-item total">
                  <span>總交割金額</span>
                  <strong>{{ row.result.totalAmount }}</strong>
                </div>
              </template>
              <template v-else>
                <div class="result-title">無法取得收盤價</div>
                <p class="result-text">
                  該日期沒有收盤價資料，無法計算交割金額。
                </p>
              </template>
            </div>

            <div v-else class="result-box placeholder">
              請輸入資料並按下「計算」，結果會顯示在這裡。
            </div>
          </div>
        </div>
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
  max-width: 960px;
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
  max-width: 960px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.trade-card {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(15, 23, 42, 0.06);
  padding: 1rem 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.trade-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.trade-card-title {
  font-weight: 600;
  font-size: 0.95rem;
  color: #111827;
}

.trade-card-body {
  display: grid;
  grid-template-columns: 2fr 1.5fr;
  gap: 1.5rem;
}

.trade-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 1rem;
  row-gap: 0.75rem;
}

.form-row {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.form-row-full {
  grid-column: 1 / -1;
}

.form-label {
  font-size: 0.8rem;
  color: #6b7280;
}

.input {
  border-radius: 8px;
  border: 1px solid #d1d5db;
  padding: 0.4rem 0.55rem;
  font-size: 0.9rem;
  outline: none;
  transition: border-color 0.15s, box-shadow 0.15s;
  background-color: #f9fafb;
}

.input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 1px rgba(37, 99, 235, 0.4);
  background-color: #fff;
}

.hint {
  font-size: 0.8rem;
  color: #6b7280;
}

.form-actions {
  margin-top: 0.25rem;
}

.trade-result {
  display: flex;
  align-items: stretch;
}

.result-box {
  border-radius: 10px;
  padding: 0.75rem 0.9rem;
  font-size: 0.9rem;
  width: 100%;
}

.result-box.placeholder {
  border: 1px dashed #d1d5db;
  color: #6b7280;
  background: #f9fafb;
}

.result-box.error {
  border: 1px solid #fecaca;
  background: #fef2f2;
  color: #b91c1c;
}

.result-box.success {
  border: 1px solid #bfdbfe;
  background: #eff6ff;
  color: #1f2937;
}

.result-title {
  font-weight: 600;
  margin-bottom: 0.4rem;
}

.result-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.15rem;
}

.result-item span {
  color: #4b5563;
  font-size: 0.85rem;
}

.result-item strong {
  font-weight: 600;
}

.result-item.total {
  margin-top: 0.4rem;
  padding-top: 0.4rem;
  border-top: 1px dashed #93c5fd;
}

.result-item.total strong {
  font-size: 1rem;
}

.result-text {
  font-size: 0.85rem;
  color: #4b5563;
}

/* Buttons */
.btn {
  border-radius: 9999px;
  border: none;
  padding: 0.35rem 0.9rem;
  font-size: 0.85rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  transition: background-color 0.15s, color 0.15s, box-shadow 0.15s, border-color 0.15s;
}

.btn-primary {
  background: #2563eb;
  color: #fff;
  box-shadow: 0 1px 4px rgba(37, 99, 235, 0.4);
}

.btn-primary:disabled {
  opacity: 0.7;
  cursor: default;
  box-shadow: none;
}

.btn-primary:not(:disabled):hover {
  background: #1d4ed8;
}

.btn-outline {
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
}

.btn-ghost:hover {
  background: #f3f4f6;
  color: #111827;
}

.add-row {
  display: flex;
  justify-content: center;
  margin-top: 0.5rem;
}

/* RWD */
@media (max-width: 768px) {
  .trade-card-body {
    grid-template-columns: 1fr;
  }

  .trade-form {
    grid-template-columns: 1fr;
  }
}
</style>
