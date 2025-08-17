import {
  EntityDefinitionPartApplications,
  EntityDefinitionPartArticles,
  EntityDefinitionPartAttributes,
  EntityDefinitionPartBaseViews,
  EntityDefinitionPartChangeLogs,
  EntityDefinitionPartFields,
  EntityDefinitionPartFilterRelations,
  EntityDefinitionPartLinkedEntities,
  EntityDefinitionPartOverview,
  EntityDefinitionPartPipelines,
  EntityDefinitionPartPlugins,
  EntityDefinitionPartReports,
  EntityDefinitionPartSubTypes,
  EntityDefinitionPartSupportLinks,
  EntityDefinitionPartViews
} from '#components';

const tableName = {
  Entity: 'Entity',
  Articles: 'Articles',
  Fields: 'Fields',
  FieldValues: 'FieldValues',
  SubTypes: 'SubTypes',
  Views: 'Views',
  BaseViews: 'BaseViews',
  BaseViewFields: 'BaseViewFields',
  Pipeline: 'Pipeline',
  PipelineSteps: 'PipelineSteps',
  PipelineComponents: 'PipelineComponents',
  PipelineInputs: 'PipelineInputs',
  PipelineFlowOutputs: 'PipelineFlowOutputs',
  PipelineComponentOutputs: 'PipelineComponentOutputs',
  PipelineFlowResults: 'PipelineFlowResults',
  PipelineComponentResults: 'PipelineComponentResults',
  PipelineFlowStepActions: 'PipelineFlowStepActions',
  PipelineFlowStepOutputMaps: 'PipelineFlowStepOutputMaps',
  LinkedEntities: 'LinkedEntities',
  Applications: 'Applications',
  ApplicationPermissions: 'ApplicationPermissions',
  ApplicationEntities: 'ApplicationEntities',
  HelpLinks: 'HelpLinks',
  RecordHistory: 'RecordHistory',
  FilterRelations: 'FilterRelations',
  Reports: 'Reports',
  Attributes: 'Attributes',
  Plugins: 'Plugins'
};

const useEntityDefinitionState = ref<{
  [key: string]: any;
  raw: Array<Array<any>> | null;
  entityId: number;
  parts: Array<any>;
}>({
  raw: null,
  entityId: -1,
  parts: [
    {
      id: tableName.Entity,
      partId: 'overview',
      label: 'Overview',
      icon: 'fa6-regular:file-lines',
      component: markRaw(EntityDefinitionPartOverview)
    },

    {
      id: tableName.Fields,
      partId: 'fields',
      label: 'Fields',
      icon: 'fa6-solid:leaf',
      component: markRaw(EntityDefinitionPartFields)
    },

    {
      id: tableName.SubTypes,
      partId: 'sub-types',
      label: 'Sub-Types',
      icon: 'fa6-solid:folder-tree',
      component: markRaw(EntityDefinitionPartSubTypes)
    },

    {
      id: tableName.Views,
      partId: 'views',
      label: 'Views',
      icon: 'fa6-solid:suitcase',
      component: markRaw(EntityDefinitionPartViews)
    },

    {
      id: tableName.BaseViews,
      partId: 'base-views',
      label: 'Base Views',
      icon: 'fa6-solid:cart-flatbed-suitcase',
      component: markRaw(EntityDefinitionPartBaseViews)
    },

    {
      id: tableName.Pipeline,
      partId: 'process-pipeline',
      label: 'Process Pipeline',
      icon: 'fa6-solid:chart-diagram',
      component: markRaw(EntityDefinitionPartPipelines)
    },

    {
      id: tableName.LinkedEntities,
      partId: 'linked-entities',
      label: 'Linked Entities',
      icon: 'fa6-solid:network-wired',
      component: markRaw(EntityDefinitionPartLinkedEntities)
    },

    {
      id: tableName.Applications,
      partId: 'applications',
      label: 'Applications',
      icon: 'fa6-solid:cubes',
      component: markRaw(EntityDefinitionPartApplications)
    },

    {
      id: tableName.Articles,
      partId: 'articles',
      label: 'Articles',
      icon: 'fa6-regular:newspaper',
      component: markRaw(EntityDefinitionPartArticles)
    },

    {
      id: tableName.HelpLinks,
      partId: 'support-links',
      label: 'Support Links',
      icon: 'fa6-solid:handshake-angle',
      component: markRaw(EntityDefinitionPartSupportLinks)
    },

    {
      id: tableName.RecordHistory,
      partId: 'change-logs',
      label: 'Change Log',
      icon: 'fa6-solid:clock-rotate-left',
      component: markRaw(EntityDefinitionPartChangeLogs)
    },

    {
      id: tableName.FilterRelations,
      partId: 'filter-relations',
      label: 'Filter Relations',
      icon: 'fa6-solid:circle-nodes',
      component: markRaw(EntityDefinitionPartFilterRelations)
    },

    {
      id: tableName.Reports,
      partId: 'reports',
      label: 'Reports',
      icon: 'fa6-regular:file-image',
      component: markRaw(EntityDefinitionPartReports)
    },

    {
      id: tableName.Attributes,
      partId: 'attributes',
      label: 'Attributes',
      icon: 'fa6-solid:tag',
      component: markRaw(EntityDefinitionPartAttributes)
    },

    {
      id: tableName.Plugins,
      partId: 'plugins',
      label: 'Plugins',
      icon: 'fa6-solid:puzzle-piece',
      component: markRaw(EntityDefinitionPartPlugins)
    }
  ]
});

export const useEntityDefinition = () => {
  const { soaEntityList, soaDefinition } = useAptifySoa();

  const entityDefinitionGet = async (entity: string | number, force: boolean = false) => {
    try {
      if (!entity) {
        entityDefinitionReset();
        return null;
      }

      // Parse entity as it could be the ID or the Name of the entity
      let entityId = isNumber(entity) ? Number(entity) : -1;
      if (entityId <= 0) {
        const entityList = await soaEntityList();
        const eName = String(entity).toLowerCase().trim();
        entityId = entityList?.find((entity: any) => entity.LookupName == eName)?.ID;
      }

      if (entityId <= 0) {
        entityDefinitionReset();
        return null;
      }

      if (useEntityDefinitionState.value.entityId == entityId && !force) {
        return useEntityDefinitionState.value.parts;
      }

      // New Entity, reset the state
      entityDefinitionReset();

      const response = await soaDefinition(entityId);
      if (response && response.length > 0) {
        let entityDisplayName = '';

        for (const def of useEntityDefinitionState.value.parts) {
          let defData = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == def.id);

          if (def.id == tableName.Entity) {
            defData = defData[0];
            useEntityDefinitionState.value.entityId = defData.ID;
            entityDisplayName = defData.DisplayName;
            logDev('Display Name: ', entityDisplayName);

            if (!defData.DefinitionID || !defData.Definition) {
              defData.Definition = `${defData.DisplayName} has no overview defined.`;
            }
          }

          else if (def.id == tableName.Fields) {
            const fieldValues = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.FieldValues) ?? [];
            defData = defData?.map((d: any) => {
              d.Values = fieldValues.filter((v: any) => v.FieldID == d.ID);
              return d;
            });
          }

          else if (def.id == tableName.BaseViews) {
            const baseViewFields = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.BaseViewFields) ?? [];
            defData = defData?.map((d: any) => {
              d.Fields = baseViewFields.filter((v: any) => v.EntityBaseViewID == d.ID);
              return d;
            });
          }

          else if (def.id == tableName.Applications) {
            const appPermissions = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.ApplicationPermissions) ?? [];
            const appEntities = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.ApplicationEntities) ?? [];

            defData = defData?.map((d: any) => {
              d.Permissions = appPermissions.filter((ap: any) => ap.ApplicationID == d.ID);
              d.PermissionsGroupDisplay = d.Permissions.filter((p: any) => p.Type == 'Group').map((p: any) => p.TypeName.trim()).sort().join(', ');
              d.PermissionsUserDisplay = d.Permissions.filter((p: any) => p.Type == 'User').map((p: any) => p.TypeName.trim()).sort().join(', ');

              d.Entities = appEntities.filter((e: any) => e.ApplicationID == d.ID)
                .map((e: any) => {
                  return {
                    ID: e.EntityID,
                    Name: e.Entity,
                    DisplayName: e.EntityDisplayName,
                    InMyFolderList: e.InMyFolderList
                  };
                });

              d.InMyFolderList = d.Entities.find((ae: any) => ae.InMyFolderList) != null;

              return d;
            });
          }

          else if (def.id == tableName.Pipeline) {
            const PipelineInputs = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineInputs) ?? [];
            const PipelineFlowOutputs = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineFlowOutputs) ?? [];
            const PipelineFlowResults = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineFlowResults) ?? [];
            const PipelineSteps = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineSteps) ?? [];
            const PipelineComponents = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineComponents) ?? [];
            const PipelineComponentOutputs = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineComponentOutputs) ?? [];
            const PipelineComponentResults = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineComponentResults) ?? [];
            const PipelineFlowStepActions = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineFlowStepActions) ?? [];
            const PipelineFlowStepOutputMaps = response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.PipelineFlowStepOutputMaps) ?? [];

            defData = defData?.map((d: any) => {
              d.ProcessFlow = {
                Inputs: PipelineInputs.filter((pi: any) => pi.For == 'ProcessFlow' && pi.ForID == d.ProcessFlowID),
                Outputs: PipelineFlowOutputs.filter((pfo: any) => pfo.ProcessFlowID == d.ProcessFlowID),
                Results: PipelineFlowResults.filter((pfr: any) => pfr.ProcessFlowID == d.ProcessFlowID),
                Steps: PipelineSteps.filter((ps: any) => ps.ProcessFlowID == d.ProcessFlowID).map((ps: any) => {
                  ps.IsFirstStep = defData.some((first: any) => first.ProcessFlowFirstStepID == ps.ID);
                  ps.Actions = PipelineFlowStepActions.filter((pfsa: any) => pfsa.ProcessFlowStepID == ps.ID);
                  ps.OutputMaps = PipelineFlowStepOutputMaps.filter((pfso: any) => pfso.ProcessFlowStepID == ps.ID);

                  if (ps.Type == 'Component') {
                    ps.Component = PipelineComponents.find((pc: any) => pc.ID == ps.ProcessComponentID);
                    if (ps.Component) {
                      ps.Inputs = PipelineInputs.filter((pi: any) => pi.For == 'ProcessComponent' && pi.ForID == ps.ProcessComponentID);
                      ps.Component.Outputs = PipelineComponentOutputs.filter((pco: any) => pco.ProcessComponentID == ps.ProcessComponentID);
                      ps.Component.Results = PipelineComponentResults.filter((pcr: any) => pcr.ProcessComponentID == ps.ProcessComponentID);
                    }
                  }

                  return ps;
                }).sort((a: any, b: any) => {
                  if (a.IsFirstStep == b.IsFirstStep) {
                    return a.Order - b.Order;
                  }

                  return (b.IsFirstStep ? 1 : 0) - (a.IsFirstStep ? 1 : 0)
                })
              };

              const results: Array<any> = PipelineFlowResults.filter((pfr: any) => pfr.ProcessFlowID == d.ProcessFlowID)
                .map((pfr: any) => {
                  return {
                    id: `z${(pfr.Code ?? 'NoResult').replaceAll(' ', '').toUpperCase()}`,
                    label: pfr.Code,
                    success: pfr.IsSuccess
                  };
                });

              let links: Array<any> = [];
              d.ProcessFlow.Steps.forEach((step: any) => {
                if (step.Actions?.length > 0) {
                  links = [...links, ...step.Actions.map((action: any) => {
                    // PF Result Code
                    const pfResultCode = (action.ProcessFlowResultCode.trim() == ''
                      ? 'NoResult'
                      : action.ProcessFlowResultCode.replaceAll(' ', '')
                    ).toUpperCase();

                    const pfCode = `z${pfResultCode}`;
                    if (!results.some((r: any) => r.id == pfCode)) {
                      results.push({
                        id: pfCode,
                        label: pfResultCode,
                        success: action.IsSuccess
                      });
                    }

                    // Link
                    return {
                      id: `${step.ID}-${action.ResultCode.replaceAll(' ', '')}`,
                      source: `${step.ID}`,
                      target: {
                        action: action.Action,
                        goToStepID: action.GoToStepID,
                        processFlowResultCode: pfResultCode,
                      },
                      label: action.ResultCode.toLowerCase(),
                      success: action.IsSuccess
                    };
                  })];
                }
                else {
                  links.push({
                    id: `${step.ID}-noresult`,
                    source: `${step.ID}`,
                    target: {
                      action: 'End Process',
                      goToStepID: null,
                      processFlowResultCode: 'NoResult',
                    },
                    label: 'No Result',
                    success: true
                  });
                }
              });

              if (results.length <= 0) {
                results.push({ id: 'zNORESULT', label: 'NoResult', success: true });
              }

              // Add Steps (nodes)
              const mermaidDiagram = ['flowchart TD'];

              d.ProcessFlow.Steps.forEach((step: any) => {
                if (step.Type == 'Component') {
                  mermaidDiagram.push(`\t${step.ID}[${step.Name}]`);
                }
                else if (step.Type == 'Sub-Process') {
                  mermaidDiagram.push(`\t${step.ID}[[${step.Name}]]`);
                }
                else {
                  mermaidDiagram.push(`\t${step.ID}([${step.Name}])`);
                }
              });

              results.forEach((result: any) => {
                if (links.some((l: any) => l.target.processFlowResultCode == result.label)) {
                  mermaidDiagram.push(`\t${result.id}(Result: ${result.label})`);
                }
              });

              mermaidDiagram.push('');

              // Add Links (edges)
              const linkSuccess: Array<Number> = [];
              const linkFailure: Array<Number> = [];

              links.forEach((link: any, idx: number) => {
                const target = link.target.action == 'Go To Step' ? link.target.goToStepID : `z${link.target.processFlowResultCode.toUpperCase()}`;
                mermaidDiagram.push(`\t${link.source} -- ${link.label} --> ${target}`);

                if (link.success) {
                  linkSuccess.push(idx);
                } else {
                  linkFailure.push(idx);
                }
              });

              mermaidDiagram.push('');

              // Click Actions
              d.ProcessFlow.Steps.forEach((step: any) => {
                mermaidDiagram.push(`click ${step.ID} mermaidCallback "${step.Name}"`);
              });

              mermaidDiagram.push('');

              // Class Definitions
              mermaidDiagram.push('classDef mermaidNodeStart, mermaidNodeRuleScript, mermaidNodeComponent, mermaidNodeComponent, mermaidResultCode;');
              d.ProcessFlow.Steps.filter((step: any) => step.IsFirstStep).forEach((step: any) => {
                mermaidDiagram.push(`class ${step.ID} mermaidNodeStart;`);
              });

              const ruleSteps = d.ProcessFlow.Steps.filter((step: any) => step.Type == 'Rule Script').map((step: any) => step.ID);
              if (ruleSteps.length > 0) {
                mermaidDiagram.push(`class ${ruleSteps.join(',')} mermaidNodeRuleScript;`);
              }

              const componentSteps = d.ProcessFlow.Steps.filter((step: any) => step.Type == 'Component').map((step: any) => step.ID);
              if (componentSteps.length > 0) {
                mermaidDiagram.push(`class ${componentSteps.join(',')} mermaidNodeComponent;`);
              }

              const subProcessSteps = d.ProcessFlow.Steps.filter((step: any) => step.Type == 'Sub-Process').map((step: any) => step.ID);
              if (subProcessSteps.length > 0) {
                mermaidDiagram.push(`class ${subProcessSteps.join(',')} mermaidNodeSubProcess;`);
              }

              const resultSteps = results.map((r: any) => `z${r.label.toUpperCase()}`);
              if (resultSteps.length > 0) {
                mermaidDiagram.push(`class ${resultSteps.join(',')} mermaidResultCode;`);
              }

              mermaidDiagram.push('');

              // Links Styles
              if (linkSuccess.length > 0) {
                mermaidDiagram.push(`linkStyle ${linkSuccess.join(',')} stroke:green`);
              }

              if (linkFailure.length > 0) {
                mermaidDiagram.push(`linkStyle ${linkFailure.join(',')} stroke:red`);
              }

              d.ProcessFlow.Diagram = mermaidDiagram.join('\n');
              //logDev('Mermaid Diagram: ', d.ProcessFlow.Diagram);

              return d;
            });
          }

          else if (def.id == tableName.FilterRelations) {
            defData = defData?.map((d: any) => {
              const vals = [`${response.find((d: Array<any>) => d[0]?.EntityDefinitionAppTable == tableName.Entity)[0]?.Name}.ID = `]

              if (d.JoinTable != '' && d.RelatedField.startsWith(d.JoinTable)) {
                vals.push(d.RelatedField);
              }
              else {
                vals.push(`${(d.JoinTable != '' ? d.JoinTable : d.RelatedEntity)}.${d.RelatedField}`);
              }

              d.RelationTitle = vals.join(' ');

              return d;
            });
          }

          // else if (def.id == tableName.LinkedEntities) {
          //   const entities: Array<any> = [];
          //   defData.forEach((e: any) => {
          //     if (!entities.some((ent: any) => ent.ID == e.EntityID)) {
          //       entities.push({
          //         ID: e.EntityID,
          //         Name: e.Entity,
          //         DisplayName: e.EntityDisplayName,
          //       });
          //     }
          //   });

          //   const mermaidDiagram = ['flowchart TD', `\t${entityId}["${entityDisplayName}"]`];
          //   entities.forEach((e: any) => {
          //     if (e.EntityID != entityId) {
          //       mermaidDiagram.push(`\t${e.ID}["${e.DisplayName}"]`);
          //     }
          //   });

          //   mermaidDiagram.push('');

          //   defData.forEach((e: any) => {
          //     if (!e.SQLAllowNull) {
          //       mermaidDiagram.push(`\t${e.EntityID} -- ${e.DisplayName} --> ${entityId}`);
          //     }
          //     else {
          //       mermaidDiagram.push(`\t${e.EntityID} -. ${e.DisplayName} .-> ${entityId}`);
          //     }
          //   });

          //   const diagram = mermaidDiagram.join('\n');
          //   logDev('Linked Entities Diagram: ', diagram);
          // }

          def.data = defData;
        }
      }
      else {
        console.error(`Entity Definition for Entity ID ${entityId} not found.`);
      }

      return useEntityDefinitionState.value.parts;
    }
    catch (error) {
      console.error('Error in entityDefinitionGet:', error);
      return null;
    }
  }

  const entityDefinitionReset = () => {
    useEntityDefinitionState.value.raw = null;
    useEntityDefinitionState.value.entityId = -1;
    useEntityDefinitionState.value.parts.forEach((def: any) => {
      def.data = null;
    });
  }

  return {
    entityDefinitionGet,
    entityDef: readonly(computed(() => useEntityDefinitionState.value.parts)),
    entityDefEntity: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Entity)?.data)),
    entityDefArticles: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Articles)?.data)),
    entityDefFields: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Fields)?.data)),
    entityDefSubTypes: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.SubTypes)?.data)),
    entityDefViews: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Views)?.data)),
    entityDefBaseViews: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.BaseViews)?.data)),
    entityDefPipeline: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Pipeline)?.data)),
    entityDefLinkedEntities: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.LinkedEntities)?.data)),
    entityDefApplications: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Applications)?.data)),
    entityDefHelpLinks: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.HelpLinks)?.data)),
    entityDefRecordHistory: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.RecordHistory)?.data)),
    entityDefAttributes: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Attributes)?.data)),
    entityDefReports: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Reports)?.data)),
    entityDefPlugins: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.Plugins)?.data)),
    entityDefFilterRelations: readonly(computed(() => useEntityDefinitionState.value.parts.find((t: any) => t.id == tableName.FilterRelations)?.data))
  }
}