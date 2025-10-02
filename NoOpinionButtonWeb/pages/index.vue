<template>
  <div>
    <!-- This page will automatically redirect to signin -->
  </div>
</template>

<script setup>
// Redirect to signin page as the main entry point
await navigateTo('/signin')
</script>