<template>
  <i :class="iconClass" :style="iconStyles" />
</template>


<script setup lang="ts">
  const props = defineProps<{ name: string, size?: string | number, class?: string | Array<string> | null }>();

  const iconClass = computed(() => {
    const css: Array<string> = [ 'inline-block' ];

    const split: Array<string> = props.name.split(':');
    if (split.length == 2) {
      css.push(split[0]?.trim().replace('fa6', 'fa') ?? '');
      css.push(`fa-${split[1]?.trim() ?? ''}`);
    }

    if (props.class != undefined && props.class != null && props.class.length > 0) {
      css.push(...(Array.isArray(props.class) ? props.class : [props.class]));
    }

    return css.length > 0 ? css.join(' ') : null;
  });

  const iconStyles = computed(() => {
    return {
      fontSize: props.size ? `${props.size}px` : '',
    };
  });
</script>
