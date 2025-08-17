const usePageLoaderState = ref<{
  show: boolean,
  message: string
}>({
  show: false,
  message: 'Loading....'
});

export const usePageLoader = () => {
  const showHidePageLoader = (show: boolean = true, message: string = 'Loading....') => {
    usePageLoaderState.value.message = message;
    usePageLoaderState.value.show = show;
  }

  return {
    showHidePageLoader,
    show: computed(() => usePageLoaderState.value.show),
    message: computed(() => usePageLoaderState.value.message)
  }
}