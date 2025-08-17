
export const useKeywordTrigger = (keyword: Ref<string>) => {
  const filterKeyword = ref<string>();

  watchDebounced(
    keyword,
    () => {
      const key = ((keyword.value ?? '').trim() || '');
      filterKeyword.value = key.length >= 3 ? key.toLowerCase() : '';
    },
    { debounce: 400, maxWait: 500 },
  );

  return {
    filterKeyword
  }
}