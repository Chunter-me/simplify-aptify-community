export default defineNuxtRouteMiddleware(async (to) => {
  const { soaIsLoggedIn, aptifyWebUrl } = useAptifySoa();

  if (!(await soaIsLoggedIn())) {
    return navigateTo(aptifyWebUrl, { external: true });
  }

  if (to.path.startsWith('/definition')) {
    const path = to.path.endsWith('/') ? to.path.slice(0, -1) : to.path;
    if ((path.match(/\//g)?.length ?? 0) <= 2) {
      return navigateTo(`${path}/overview`);
    }
  }
});