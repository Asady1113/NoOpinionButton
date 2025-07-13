<template>
    <div>司会者ページ</div>
</template>

<script setup>
import { useSignInStore } from '~/composables/signIn/useSignIn'

const signInStore = useSignInStore()

console.log('受け取った情報:', signInStore.value)
</script>