<template>
  <div class="flex items-start space-x-2 mb-4">
    <!-- 参加者名アバター -->
    <div class="w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center text-sm font-medium flex-shrink-0">
      {{ displayName }}
    </div>
    
    <!-- メッセージ吹き出し -->
    <div class="bg-green-200 rounded-lg px-4 py-2 ml-3 max-w-xs lg:max-w-md">
      <div class="text-sm text-gray-900">
        {{ content }}
      </div>
      <div v-if="showTimestamp" class="text-xs text-gray-600 mt-1">
        {{ formattedTime }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  participantId: string
  participantName: string
  content: string
  createdAt: string
  showTimestamp?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showTimestamp: false
})

// Display name for avatar (fallback to participant ID if name is empty)
const displayName = computed(() => {
  const name = props.participantName?.trim()
  if (name && name.length > 0) {
    // Use first 2 characters of participant name
    return name.slice(0, 2).toUpperCase()
  }
  // Fallback to participant ID if name is not available
  return props.participantId.slice(0, 2).toUpperCase()
})

// Format timestamp for display
const formattedTime = computed(() => {
  try {
    const date = new Date(props.createdAt)
    // Check if the date is valid
    if (isNaN(date.getTime())) {
      return ''
    }
    return date.toLocaleTimeString('ja-JP', {
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    return ''
  }
})
</script>