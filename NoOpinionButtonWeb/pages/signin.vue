<template>
  <div>
    <h1>サインイン画面</h1>
    <input v-model="meetingId" placeholder="会議ID" />
    <input v-model="password" type="password" placeholder="パスワード" />
    <button @click="submit" :disabled="!canSubmit">会議に参加</button>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useSignInApi } from '~/composables/useSignInApi';

const meetingId = ref('')
const password = ref('')
 
// meetingId と passwordの値が変わると自動計算される。
// true: meetingId と password が両方空じゃないとき
const canSubmit = computed(() => meetingId.value !== '' && password.value !== '')

const { signIn } = useSignInApi();

async function submit() {
  try {
    const data = await signIn(meetingId.value, password.value);
    console.log('Success:', data);
    // 成功時の処理
  } catch (e) {
    console.error(`Error`)
  }
}
</script>