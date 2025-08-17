<template>
  <div ref="container" class="size-full flex justify-center p-4" />
</template>


<script setup lang="ts">
import mermaid from 'mermaid';
mermaid.initialize({ startOnLoad: false, securityLevel: 'antiscript', flowchart:{ useMaxWidth:false }});

const { diagram } = defineProps<{ diagram?: string }>();
const emit = defineEmits([ 'node-click' ]);

const container = ref<HTMLElement | null>(null);

function addEdgeLabelPadding() {
  container.value?.querySelectorAll('.edgeLabel foreignObject')?.forEach((obj: any) => {
    const bbox = obj.getBBox();

    const rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    rect.setAttribute('x', (bbox.x - 6).toString());
    rect.setAttribute('width', bbox.width + 6 * 2);
    rect.setAttribute('height', bbox.height);
    rect.setAttribute('rx', '2');
    rect.setAttribute('ry', '2');

    obj.parentNode.insertBefore(rect, obj)
  })
}

const renderMermaid = async () => {
  if (container.value && diagram) {
    container.value.innerHTML = '';

    const id = 'mermaid-' + Math.random().toString(36).substr(2, 9);
    const { svg } = await mermaid.render(id, diagram);

    container.value.innerHTML = svg;
    addEdgeLabelPadding();

    container.value.querySelectorAll('.clickable').forEach((node: any) => {
      node.addEventListener('click', () => {
        const id = node.getAttribute('id')?.split('-')[1];
        emit('node-click', { id, node });
      });
    });
  }
}

onMounted(() => {
  renderMermaid();
});

watch(() => diagram, () => {
  renderMermaid();
});
</script>