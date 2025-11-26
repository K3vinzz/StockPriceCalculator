<script setup lang="ts">
import { ref, watch } from 'vue'
import { searchStocks, Stock, type StockSearchResponse } from '@/api/stockApi'
import debounce from 'lodash.debounce'

const keyword = ref('')
const results = ref<Stock[]>([])
const isOpen = ref(false)

const emit = defineEmits(['select'])

const fetchStocks = debounce(async () => {
    if (!keyword.value) {
        results.value = []
        isOpen.value = false
        return
    }

    const res = await searchStocks(keyword.value)
    results.value = res.data.stocks
    isOpen.value = true
}, 250)

watch(keyword, fetchStocks)

function selectStock(item: Stock) {
    keyword.value = `${item.symbol} ${item.name}`
    isOpen.value = false
    emit('select', item)
}
</script>

<template>
    <div class="autocomplete">
        <input v-model="keyword" type="text" placeholder="輸入股票代號或名稱" class="input" />

        <ul v-if="isOpen && results.length" class="dropdown">
            <li v-for="item in results" :key="item.symbol" @click="selectStock(item)">
                {{ item.symbol }} - {{ item.name }} ({{ item.market }})
            </li>
        </ul>
    </div>
</template>

<style scoped>
.autocomplete {
    position: relative;
}

.dropdown {
    position: absolute;
    width: 100%;
    background: white;
    border: 1px solid #ddd;
    max-height: 200px;
    overflow: auto;
    z-index: 10;
    list-style: none;
    margin: 0;
    padding: 0;
}

.dropdown li {
    padding: 8px;
    cursor: pointer;
}

.dropdown li:hover {
    background: #f0f0f0;
}

.input {
    width: 100%;
    padding: 8px;
    border-radius: 6px;
    border: 1px solid #ccc;
}
</style>
