<template>
  <input
    :value="modelValue"
    @input="$emit('update:modelValue', $event.target.value)"
    :placeholder="placeholder"
    :maxlength="maxLength"
    :disabled="disabled"
    class="border px-4 py-1 rounded-lg w-full focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
  />
</template>

<script setup lang="ts">
interface Props {
  modelValue: string
  placeholder?: string
  maxLength?: number
  disabled?: boolean
}

/*
modelValueという値が更新されたらそれを親に通知
親はただ v-model="nameInput" と書いているだけ
*/
interface Emits {
  (e: 'update:modelValue', value: string): void
}

defineProps<Props>()
defineEmits<Emits>()
</script>