export interface EntityDefinitionResultsIndexes {
  Name: string;
  Key: string;
  Index: number;
}

export interface EntityDefinition {
  [key: string]: any
  Entity: any
  Fields: Array<any>
  SubTypes: Array<any>
  LinkedEntities: Array<any>
  BaseViews: Array<any>
  Attributes: Array<any>
  FilterRelations: Array<any>
  Plugins: Array<any>
  Reports: Array<any>
  Applications: Array<any>
  Views: Array<any>
  RecordHistory: Array<any>
  Articles: Array<any>
  Links: Array<any>
  Pipeline: any
}

export interface ViewData {
  [key: string]: any
  Filters: Array<any>
  Fields: Array<any>
  Sql: string
  SqlParsed: any
}