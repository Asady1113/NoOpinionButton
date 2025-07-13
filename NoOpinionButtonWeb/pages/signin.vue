<template>
  <div class="flex flex-col items-center justify-center min-h-screen">
    <h1 class="text-center font-bold text-2xl mb-12">
      意見ありませんボタン
    </h1>
    <input v-model="meetingId" placeholder="会議ID" class="mb-6 border px-4 py-1 rounded-lg w-96" />
    <input v-model="password" type="password" placeholder="パスワード" class="mb-10 border px-4 py-1 rounded-lg w-96" />
    <button @click="submit" :disabled="!canSubmit" class="px-6 py-2 bg-indigo-500 text-white rounded-full disabled:opacity-50 w-60" >
      会議に参加
    </button>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useSignInApi } from '~/composables/useSignInApi';
import { useRouter } from 'vue-router'

const meetingId = ref('')
const password = ref('')
 
// meetingId と passwordの値が変わると自動計算される。
// true: meetingId と password が両方空じゃないとき
const canSubmit = computed(() => meetingId.value !== '' && password.value !== '')

const { signIn } = useSignInApi();
const router = useRouter()

async function submit() {
  try {
    const response = await signIn(meetingId.value, password.value);
    const data = response.Data;
    
    if (data.isFacilitator) {
      router.push(`/facilitator`) 
    } else {
      router.push('/participant') 
    }
  } catch (e) {
    console.error(`Error`)
  }
}
</script>