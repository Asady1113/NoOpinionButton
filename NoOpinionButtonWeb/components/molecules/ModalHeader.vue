<template>
  <header class="text-center mb-6">
    <Title
      :text="title"
      :level="level"
    />
  </header>
</template>

<script setup lang="ts">
import Title from '~/components/atoms/Title.vue'

interface Props {
  title: string
  level?: 'h1' | 'h2' | 'h3'
}

withDefaults(defineProps<Props>(), {
  level: 'h2'
})
</script>