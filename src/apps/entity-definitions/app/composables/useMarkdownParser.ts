import { createMarkdownParser, rehypeHighlight, createShikiHighlighter } from '@nuxtjs/mdc/runtime';

import DarkPlusTheme from '@shikijs/themes/dark-plus';
import JsLang from '@shikijs/langs/javascript';
import TsLang from '@shikijs/langs/typescript';
import VbLang from '@shikijs/langs/vb';
import CsharpLang from '@shikijs/langs/csharp';
import HtmlLang from '@shikijs/langs/html';
import CssLang from '@shikijs/langs/css';
import VueLang from '@shikijs/langs/vue';
import SqlLang from '@shikijs/langs/sql';
import YamlLang from '@shikijs/langs/yaml';
import XmlLang from '@shikijs/langs/xml';
import JsonLang from '@shikijs/langs/json';

export const useMarkdownParser = () => {
  let parser: Awaited<ReturnType<typeof createMarkdownParser>>;

  const parseMarkdown = async (markdown: string) => {
    if (!parser) {
      parser = await createMarkdownParser({
        rehype: {
          plugins: {
            highlight: {
              instance: rehypeHighlight,
              options: {
                theme: 'dark-plus',
                highlighter: createShikiHighlighter({
                  bundledThemes: {
                    'dark-plus': DarkPlusTheme,
                  },
                  bundledLangs: {
                    vb: VbLang,
                    html: HtmlLang,
                    csharp: CsharpLang,
                    vue: VueLang,
                    yml: YamlLang,
                    ts: TsLang,
                    js: JsLang,
                    typescript: TsLang,
                    css: CssLang,
                    sql: SqlLang,
                    xml: XmlLang,
                    json: JsonLang
                  }
                })
              }
            }
          }
        }
      });
    }

    const parsed = await parser(markdown);
    return parsed;
  };

  return {
    parseMarkdown
  };
}