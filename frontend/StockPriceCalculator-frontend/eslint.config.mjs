// eslint.config.mjs
import pluginVue from 'eslint-plugin-vue'
import {
  defineConfigWithVueTs,
  vueTsConfigs
} from '@vue/eslint-config-typescript'
import { globalIgnores } from 'eslint/config'; // For global ignores
// 使用官方提供的幫手：Vue 3 + TypeScript 最小設定
export default defineConfigWithVueTs(
  pluginVue.configs['flat/essential'], // Vue 3 基本規則
  vueTsConfigs.recommended,              // TypeScript 推薦規則（含 .vue）
  globalIgnores(['**/dist/**'])
)
