<script setup lang="ts">
import { ref } from 'vue'
import { Stock, matchStock } from '@/api/stockApi'

const keyword = ref('')
const result = ref<Stock | null>(null)

const emit = defineEmits<{
    (e: 'select', stock: Stock | null): void;
}>()


async function onKeywordChange() {
    const trimmed = keyword.value.trim()

    if (!trimmed) {
        result.value = null
        emit('select', null)
        return
    }

    try {
        const res = await matchStock(trimmed)
        const stock = res.data.stock

        if (!stock) {
            result.value = null
            emit('select', null)
            return
        }

        result.value = stock
        // 把選到的股票字串顯示在輸入框
        keyword.value = `${stock.symbol} ${stock.name}`
        emit('select', stock)
    } catch (e) {
        console.error(e)
        result.value = null
        emit('select', null)
    }
}
</script>

<template>
    <div class="autocomplete">
        <input v-model="keyword" type="text" placeholder="輸入股票代號或名稱" class="input" @change="onKeywordChange" />
    </div>
</template>

<style scoped>
.autocomplete {
    position: relative;
}

.input {
    width: 100%;
    padding: 8px;
    border-radius: 6px;
    border: 1px solid #ccc;
}
</style>
