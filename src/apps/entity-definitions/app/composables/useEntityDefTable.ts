export const useEntityDefTable = (entityDefData: Array<any>, keywordFields: Array<string> = [ 'Name', 'DisplayName' ]) => {
  const tableData = entityDefData ?? [];

  const entityDefTableFiltered = ref<Array<any>>(tableData);
  const entityDefTableFilterData = (keyword: string) => {
    entityDefTableFiltered.value = tableDataKeywordNameDisplayFilter(tableData, keyword, keywordFields);
  }

  return {
    entityDefTableFilterData,
    entityDefTableFiltered
  }
}