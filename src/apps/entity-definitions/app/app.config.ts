export default defineAppConfig({
  ui: {
    colors: {
      primary: 'red',
      neutral: 'zinc'
    },

    table: {
      slots: {
        root: 'size-full',
        tbody: '[&>tr]:data-[selectable=true]:hover:bg-red-600/75 [&>tr]:data-[selectable=true]:dark:hover:bg-red-600/25',
        tr: 'group data-[selectable=true]:cursor-pointer data-[selectable=true]:hover:[&>td]:text-white even:bg-zinc-100 dark:even:bg-zinc-800/50',
        th: 'dark:text-gray-400',
        td: 'text-gray-800 dark:text-gray-200'
      }
    },

    button: {
      slots: {
        base: '!gap-x-2'
      },
      compoundVariants: [
        {
          color: 'primary',
          variant: 'solid',
          class: '!text-gray-200 bg-primary-700 hover:bg-primary/75 disabled:bg-primary aria-disabled:bg-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary'
        },

        {
          color: 'primary',
          variant: 'link',
          class: 'text-gray-800 hover:text-gray-800 hover:bg-zinc-300 dark:text-gray-200 hover:dark:text-gray-800 hover:dark:bg-zinc-200 disabled:text-primary aria-disabled:text-primary focus:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-primary'
        }
      ]
    },

    popover: {
      slots: {
        content: 'p-3 max-w-[300px] dark:border-zinc-950 dark:bg-zinc-800'
      }
    },

    tabs: {
      slots: {
      }
    }
  }
});