import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin(async () => {
  const { soaIsLoggedIn, aptifyWebUrl, soaAppContentBlocks, soaEntityList } = useAptifySoa();

  if (!(await soaIsLoggedIn())) {
    window.location.href = aptifyWebUrl;
  }
  else {
    await Promise.all([ soaEntityList(), soaAppContentBlocks() ]);
  }
});