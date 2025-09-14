<template>
  <component
    :is="level"
    class="font-bold text-center"
    :class="titleClasses"
  >
    {{ text }}
  </component>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  text: string
  level?: 'h1' | 'h2' | 'h3'
}

const props = withDefaults(defineProps<Props>(), {
  level: 'h2'
})

const titleClasses = computed(() => {
  switch (props.level) {
    case 'h1':
      return 'text-2xl md:text-3xl mb-6'
    case 'h2':
      return 'text-xl md:text-2xl mb-4'
    case 'h3':
      return 'text-lg md:text-xl mb-3'
    default:
      return 'text-xl md:text-2xl mb-4'
  }
})
</script>