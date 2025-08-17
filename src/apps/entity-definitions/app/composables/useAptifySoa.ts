const useAptifySoaState = ref<{
  contentBlocks: Array<any>
}>({
  contentBlocks: []
})

export const useAptifySoa = () => {
  const { parseMarkdown } = useMarkdownParser();

  const configs = useRuntimeConfig().public;

  let resolvedSoaUrl: string;
  if (configs.AptifySoaUrl == '/') {
    resolvedSoaUrl = window.location.origin;
  }
  else if (configs.AptifySoaUrl.startsWith('http')) {
    resolvedSoaUrl = configs.AptifySoaUrl;
  }
  else {
    resolvedSoaUrl = `${window.location.origin}${configs.AptifySoaUrl}`;
  }

  const baseSoaUrl: string = resolvedSoaUrl;
  const aptifyWebUrl: string = baseSoaUrl.replace(configs.AptifySoaUrl, '/Aptify');

  const baseRequest: any = {
    method: 'GET',
    credentials: 'include',
    mode: 'cors',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  };

  // For Development Only
  if (process.env.NODE_ENV != 'production' && configs.AptifyToken != undefined && configs.AptifyToken != null) {
    baseRequest.headers['AptifyAuthorization'] = configs.AptifyToken.toString() ?? '';
  }

  const soaIsLoggedIn = async (): Promise<boolean> => {
    const { data } = await useFetch<{result?: string}>(`${baseSoaUrl}/UserInformation`, { ...baseRequest, cache: 'no-store' });
    return (data.value?.result ?? 'fail') == 'ok';
  }

  const fetchAppContent = async (
    contentType: 'Blocks' | 'EntityList' | 'Article' | 'ViewDetails' | 'Definition',
    recordId?: number,
    lookupKey?: string | number,
  ): Promise<Array<any> | null> => {
    const params = new URLSearchParams();
    params.append('AppContent', contentType);
    if (recordId) params.append('RecordID', String(recordId));
    if (lookupKey?.toString().trim() !== '') params.append('LookupKey', String(lookupKey).trim());

    const cacheKey = `EntityDefinitionApp?${params.toString()}`;

    const endpoint = `${baseSoaUrl}/DataObjects/${cacheKey}`;
    return await useState(cacheKey, async () => {
      try {
        const result: any = await $fetch(endpoint, { ...baseRequest, key: cacheKey });
        return result?.results ?? [];
      } catch (error) {
        console.error('Get App Content Error: ', error);
        return null;
      }
    })?.value ?? [];
  };

  const soaAppContentBlocks = async (name?: string): Promise<Array<any>> => {
    if (useAptifySoaState.value.contentBlocks?.length <= 0) {
      const response = await fetchAppContent('Blocks');
      if (response && response.length > 0) {
        useAptifySoaState.value.contentBlocks = response;
      }
    }

    if (name) {
      return useAptifySoaState.value.contentBlocks?.filter((block: any) => block.Name.toLowerCase().startsWith(name.toLowerCase())) ?? [];
    }

    return useAptifySoaState.value.contentBlocks ?? [];
  }

  const soaEntityList = async (search?: string): Promise<Array<any>> => {
    const entities = await fetchAppContent('EntityList');

    if (search && search.length >= 1) {
      const keyword = search.toLowerCase().trim();

      return entities?.filter((entity: any) => {
        if (keyword == '/subtype') {
          return entity.ParentID > 0;
        }
        else if (keyword == '/virtual') {
          return entity.IsVirtualEntity;
        }
        else if (keyword == '/framework') {
          return entity.IsFrameworkEntity;
        }

        return entity.Name.toLowerCase().replaceAll(' ', '').trim().includes(keyword)
          || entity.DisplayName.toLowerCase().replaceAll(' ', '').trim().includes(keyword)
          || entity.ID.toString().includes(keyword);
      })
      .sort((a: any, b: any) => {
        const aLower = a.DisplayName.toLowerCase()
        const bLower = b.DisplayName.toLowerCase()

        if (keyword.startsWith('/')) {
          return aLower?.localeCompare(bLower);
        }

        // 1. Exact match wins
        if (aLower === keyword && bLower !== keyword) {
          return -1 // a comes first
        }
        if (bLower === keyword && aLower !== keyword) {
          return 1 // b comes first
        }

        // 2. Starts with wins if exact match wasn't found
        if (aLower.startsWith(keyword) && !bLower.startsWith(keyword)) {
          return -1
        }
        if (!aLower.startsWith(keyword) && bLower.startsWith(keyword)) {
          return 1
        }

        // 3. Shorter length wins (closer match)
        if (aLower.length < bLower.length) {
          return -1
        }
        if (bLower.length < aLower.length) {
          return 1
        }

        // 4. If all else is equal, maintain original order (optional)
        return 0
      }) ?? [];
    }

    return entities ?? [];
  }

  const soaDefinition = async (entityId: number): Promise<Array<any> | null> => {
    return await fetchAppContent('Definition', entityId);
  }

  const soaViewDetails = async (viewId: number): Promise<ViewData | null> => {
    const response = await fetchAppContent('ViewDetails', viewId);
    if (response && response.length > 0) {
      const data: ViewData = {
        Filters: response[0],
        Fields: response[1],
        Sql: response[2][0].SQL,
        SqlParsed: {}
      };

      data.SqlParsed = await parseMarkdown(markdownWrapSqlCode(data.Sql));

      return data;
    }

    return null;
  }

  const soaArticle = async (lookupKey: string): Promise<any> => {
    if (lookupKey) {
      const response = await fetchAppContent('Article', undefined, lookupKey);
      if (response && response.length > 0) {
        const data: any = {
          Article: response[0][0],
          Links: response[1]
        };

        data.Article.SummaryParsed = await parseMarkdown(data.Article.Summary);
        data.Article.ArticleParsed = await parseMarkdown(data.Article.Article);

        return data;
      }
    }

    return null;
  }

  return {
    aptifyWebUrl,
    soaIsLoggedIn,
    soaAppContentBlocks,
    soaEntityList,
    soaDefinition,
    soaViewDetails,
    soaArticle
  }
}