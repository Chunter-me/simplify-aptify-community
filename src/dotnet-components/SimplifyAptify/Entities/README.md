# Entities

Entities contains the process components and plug-ins that are specific to an Entity.

## Naming Specifications

**Process Components** should be named using the following convention: *[EntityName][Event][AdditionalContext]ProcessComponent.cs*

Examples: 

- PersonsAfterValidateProcessComponent.cs
- PersonsBeforeSaveProcessComponent.cs
- PersonsAfterSaveUpdateSomethingElseProcessComponent.cs
- PersonsCleanupProcessComponent.cs

**Plug-ins** should be named using the following convention: *[EntityName][PluginType]Plugin.cs

Examples: PersonsEntityPlugin.cs