CREATE PROCEDURE spEntityDefinitionApp_sa (
	@AppContent NVARCHAR(50),
	@RecordID INT = NULL,
	@LookupKey NVARCHAR(150) = ''
) AS BEGIN

    SET NOCOUNT ON;

	IF @AppContent = 'Blocks'
	BEGIN

		SELECT *
		FROM EntityDefinitionAppContent_sa

	END

	ELSE IF @AppContent = 'EntityList'
	BEGIN

		SELECT
			e.ID,
			RTRIM(e.Name) [Name],
			RTRIM(e.DisplayName) [DisplayName],
			LOWER(REPLACE(RTRIM(e.Name), ' ', '-')) [LookupName],
			ISNULL(e.ParentID, -1) [ParentID],
			RTRIM(ISNULL(pe.Name, '')) [ParentName],
			RTRIM(ISNULL(pe.DisplayName, '')) [ParentDisplayName],
			e.IsFrameworkEntity,
			e.IsInstalled,
			e.IsVirtualEntity,

			ISNULL(ed.ID, -1) [DefinitionID],

			CASE
				WHEN RTRIM(ISNULL(ed.Icon, '')) = '' THEN 'fa6-solid:cubes'
				ELSE ed.Icon
			END [Icon],

			ed.IsFeatured,
			ed.IsFeaturedSummary,

			(SELECT COUNT(*) FROM Views WHERE EntityID = e.ID) [ViewCount],
			(SELECT COUNT(*) FROM EntityFields WHERE EntityID = e.ID) [FieldCount]

		FROM
			vwEntities e

			LEFT OUTER JOIN Entity pe ON
				pe.ID = e.ParentID

			LEFT OUTER JOIN EntityDefinition_sa ed ON
				ed.EntityID = e.ID

		ORDER BY
			e.DisplayName,
			e.Name

	END

    ELSE IF @AppContent = 'Article'
    BEGIN

		/** ARTICLES **/
		SELECT a.*, LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(a.Title, '''', ''), '  ', ' '), ' ', '-')))) [LookupKey]
		FROM EntityDefinitionArticle_sa a
		WHERE (
			LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(a.Title, '''', ''), '  ', ' '), ' ', '-')))) = @LookupKey
			OR @LookupKey = '_xAllArticles'
		)

		/** ARTICLE DEFINITION LINKS **/
		SELECT
			la.*,
			e.Name [EntityName]

		FROM
			EntityDefinitionLinkedArticle_sa la

			INNER JOIN EntityDefinitionArticle_sa a ON
				a.ID = la.ArticleID
				AND LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(a.Title, '''', ''), '  ', ' '), ' ', '-')))) = @LookupKey

			INNER JOIN EntityDefinition_sa ed ON
				ed.ID = la.EntityDefinitionID

			INNER JOIN Entity e ON
				e.ID = ed.EntityID

	END

    ELSE IF @AppContent = 'ViewDetails'
    BEGIN

		/** FILTERS **/
		SELECT
			v.ID [ViewID],
			v.EntityID,
			vf.Sequence,
			vf.FieldName [Field],
			RTRIM(op.Name) [Operator],
			CASE
				WHEN RTRIM(op.Name) = '=' THEN 'Is Equal to'
				WHEN RTRIM(op.Name) = '<>' THEN 'Is Not Equal to'
				WHEN RTRIM(op.Name) = '>' THEN 'Is Greater Than'
				WHEN RTRIM(op.Name) = '>=' THEN 'Is Greater Than or Equal to'
				WHEN RTRIM(op.Name) = '<' THEN 'Is Less Than'
				WHEN RTRIM(op.Name) = '<=' THEN 'Is Less Than or Equal to'
				ELSE ''
			END [OperatorDescription],
			vf.Value

		FROM
			ViewFilters vf

			INNER JOIN Views v ON
				v.ID = vf.ViewID
				AND (
					ViewID = ISNULL(@RecordID, -1)
					OR (@LookupKey = 'EntityID' AND v.EntityID = @RecordID)
				)

			OUTER APPLY (
				SELECT TOP 1 *
				FROM ViewFilterOperator o
				WHERE o.Value = vf.Operator
				AND RTRIM(o.FieldType) = CASE
					WHEN ISNUMERIC(vf.Value) = 1 THEN 'Numeric'
					WHEN ISDATE(vf.Value) = 1 THEN 'DateTime'
					ELSE 'Text'
				END
			) AS op

		/** FIELDS **/
		SELECT
			v.ID [ViewID],
			v.EntityID,
			vf.[Sequence],
			vf.FieldName [Field],
			RTRIM(vf.Description) [DisplayName]

		FROM
			ViewFields vf

			INNER JOIN Views v ON
				v.ID = vf.ViewID
				AND (
					ViewID = ISNULL(@RecordID, -1)
					OR (@LookupKey = 'EntityID' AND v.EntityID = @RecordID)
				)

		/** SQL **/
		SELECT
			v.ID [ViewID],
			v.EntityID,
			s.SQL

		FROM
			ViewSQL s

			INNER JOIN Views v ON
				v.ID = s.ViewID
				AND (
					ViewID = ISNULL(@RecordID, -1)
					OR (@LookupKey = 'EntityID' AND v.EntityID = @RecordID)
				)

	END

    ELSE IF @AppContent = 'Definition'
    BEGIN

		/** 0 - ENTITY **/
		SELECT
			'Entity' [EntityDefinitionAppTable],
			e.*,
			ed.ID [DefinitionID],
			ed.Definition,
			(SELECT TOP 1 Name FROM Entities WHERE ID = e.ParentID) [ParentEntity]

		FROM
			vwEntities e

			OUTER APPLY (
				SELECT TOP 1 *
				FROM EntityDefinition_sa ed
				WHERE ed.EntityID = e.ID
			) AS ed

		WHERE
			e.ID = @RecordID

		/** 1 - ENTITY FIELDS **/
		SELECT
			'Fields' [EntityDefinitionAppTable],
			f.*,
			CASE
				WHEN ISNULL(edf.EntityDefinitionFieldID, -1) > 0 THEN edf.EntityDefinitionFieldDesc
				ELSE f.Description
			END [FieldDefinition],
			CASE
				WHEN ISNULL(edf.EntityDefinitionFieldID, -1) > 0 THEN edf.EntityDefinitionFieldType
				ELSE CASE
					WHEN SQLDataType LIKE '%var%' OR SQLDataType LIKE '%char%' THEN 'String'
					WHEN SQLDataType LIKE '%int%' OR SQLDataType = 'numeric' OR SQLDataType = 'real' THEN 'Number'
					WHEN SQLDataType = 'decimal' OR SQLDataType = 'money' OR SQLDataType = 'Float' THEN 'Decimal'
					WHEN SQLDataType = 'date' THEN 'Date'
					WHEN SQLDataType = 'datetime' THEN 'DateTime'
					WHEN SQLDataType = 'bit' THEN 'True/False'
					WHEN SQLDataType = 'uniqueidentifier' THEN 'Guid'
					WHEN SQLDataType = 'image' THEN 'Blob'
					ELSE SQLDataType
				END
			END [SQLTypeLabel],

			CAST((
				CASE
					WHEN f.VirtualFieldType = 'Joined' OR f.VirtualFieldType = 'Calculated' THEN 1
					ELSE 0
				END
			) AS BIT) [IsVirtual]

		FROM
			vwEntityFields f

			OUTER APPLY (
				SELECT edf2.ID [EntityDefinitionFieldID], edf2.Description [EntityDefinitionFieldDesc], edf2.Type [EntityDefinitionFieldType]
				FROM vwEntityDefinitionFields_sa edf2
				INNER JOIN EntityDefinition_sa ed ON ed.ID = edf2.EntityDefinitionID AND ed.EntityID = @RecordID
				WHERE edf2.Field = f.Name
			) AS edf

		WHERE
			f.EntityID = @RecordID

		/** 2 - ENTITY FIELD VALUES **/
		SELECT
			'FieldValues' [EntityDefinitionAppTable],
			efv.EntityFieldsID [FieldID],
			efv.Value,
			efv.Description,
			efv.DisplayFields,
			efv.WhereClause,
			efv.OrderBy

		FROM
			EntityFieldValues efv

		WHERE
			efv.EntityID = @RecordID

		/** 3 - ENTITY SUB-TYPES **/
		SELECT
			'SubTypes' [EntityDefinitionAppTable],
			st.*

		FROM
			vwEntities st

		WHERE
			st.ParentID = @RecordID

		/** 4 - LINKED ENTITIES **/
		SELECT
			'LinkedEntities' [EntityDefinitionAppTable],
			lef.ID,
			lef.Name,
			lef.DisplayName,
			lef.Description,
			lef.LinkedEntityField,
			lef.SQLAllowNull,
			lefE.ID [EntityID],
			lefE.Name [Entity],
			lefE.DisplayName [EntityDisplayName],
			lefE.Description [EntityDescription],
			lefE.IconHtmlClass [EntityIconHtmlClass]

		FROM
			EntityFields lef

			INNER JOIN Entity lefE ON
				lefE.ID = lef.EntityID

		WHERE
			lef.LinkedEntity = (
				SELECT TOP 1 e.Name
				FROM Entity e
				WHERE e.ID = @RecordID
			)

		/** 5 - ENTITY BASE VIEWS **/
		SELECT
			'BaseViews' [EntityDefinitionAppTable],
			ebv.*,
			(SELECT TOP 1 o.SQLText FROM DBObject o WHERE o.Name = ebv.BaseView) [SQL]

		FROM
			vwEntityBaseViews ebv

		WHERE
			ebv.EntityID = @RecordID

		/** 6 - ENTITY BASE VIEW FIELDS **/
		SELECT
			'BaseViewFields' [EntityDefinitionAppTable],
			bvf.*

		FROM
			vwEntityBaseViewFields bvf

			INNER JOIN EntityBaseView ebv ON
				ebv.ID = bvf.EntityBaseViewID
				AND ebv.EntityID = @RecordID

		/** 7 - ATTRIBUTES **/
		SELECT
			'Attributes' [EntityDefinitionAppTable],
			ea.*
		FROM
			vwEntityAttributes ea

		WHERE
			ea.EntityID = @RecordID

		/** 8 - FILTER RELATIONS **/
		SELECT
			'FilterRelations' [EntityDefinitionAppTable],
			er.*,
			o.Description [DboDescription],
			o.SQLText [DboSql],
			o.GrantSQLText [BdoGrant]

		FROM
			vwEntityRelations er

			LEFT OUTER JOIN DBObject o ON
				o.Name = er.JoinTable

		WHERE
			er.EntityID = @RecordID

		/** 9 - PLUGINS **/
		SELECT
			'Plugins' [EntityDefinitionAppTable],
			ep.*

		FROM
			vwEntityPlugIns ep

		WHERE
			ep.EntityID = @RecordID

		/** 10 - REPORTS **/
		SELECT
			'Reports' [EntityDefinitionAppTable],
			r.EntityID,
			r.Name,
			r.Description,
			ISNULL(r.Category, '') [Category],
			r.Object,
			TRIM(REPLACE(r.ReportFormat, 'Report', '')) [ReportFormat],
			CAST(CASE
				WHEN r.Status = 'Active' THEN 1
				ELSE 0
			END AS BIT) [IsActive],
			r.Sequence

		FROM
			vwReports r

		WHERE
			r.EntityID = @RecordID

		/** 11 - APPLICATIONS **/
		SELECT
			'Applications' [EntityDefinitionAppTable],
			a.*

		FROM
			vwApplications a

			INNER JOIN vwApplicationEntities ae ON
				ae.ApplicationID = a.ID
				AND ae.EntityID = @RecordID

		/** 12 - APPLICATION PERMISSIONS **/
		SELECT DISTINCT
			'ApplicationPermissions' [EntityDefinitionAppTable],
			agp.ApplicationID,
			'Group' [Type],
			agp.GroupID [TypeID],
			agp.GroupID_Name [TypeName]

		FROM
			vwApplicationGroupPermissions agp

			INNER JOIN ApplicationEntity ae ON
				ae.ApplicationID = agp.ApplicationID
				AND ae.EntityID = @RecordID

		UNION ALL SELECT DISTINCT
			'ApplicationPermissions' [EntityDefinitionAppTable],
			aup.ApplicationID,
			'User' [Type],
			aup.UserID [TypeID],
			aup.UserID_Name [TypeName]

		FROM
			vwApplicationUserPermissions aup

			INNER JOIN ApplicationEntity ae ON
				ae.ApplicationID = aup.ApplicationID
				AND ae.EntityID = @RecordID

		/** 13 - APPLICATION ENTITIES **/
		SELECT
			'ApplicationEntities' [EntityDefinitionAppTable],
			ae.ApplicationID,
			(SELECT Name FROM Application WHERE ID = ae.ApplicationID) [Application],
			ae.EntityID,
			e.Name [Entity],
			e.DisplayName [EntityDisplayName],
			CAST(CASE WHEN aue.ID IS NULL THEN 0 ELSE 1 END AS BIT) [InMyFolderList]

		FROM
			ApplicationEntity ae

			LEFT OUTER JOIN ApplicationUserEntity aue ON
				aue.ApplicationID = ae.ApplicationID
				AND aue.EntityID = ae.EntityID
				AND aue.UserID = (
					SELECT ID
					FROM Users u
					WHERE u.UserID = SUSER_NAME()
				)

			LEFT OUTER JOIN Entity e ON
				e.ID = ae.EntityID

		WHERE
			ae.ApplicationID IN (
				SELECT app.ID
				FROM Application app
				INNER JOIN ApplicationEntity appE ON appE.ApplicationID = app.ID
				AND appE.EntityID = @RecordID
			)

		/** 14 - VIEWS **/
		SELECT
			'Views' [EntityDefinitionAppTable],
			v.*,
			CASE WHEN v.OwnerID = SUSER_NAME() THEN 1 ELSE 0 END [IsMine],
			vc.Name [ViewCategory],
			vc.EnableSharing [IsShared],
			vt.Name [ViewType],
			CASE
				WHEN vt.Name = 'List View' THEN 'fa6-solid:list'
				WHEN vt.Name = 'Chart' THEN 'fa6-solid:chart-pie'
				WHEN vt.Name = 'Calendar' THEN 'fa6-solid:calendar-days'
				WHEN vt.Name = 'Pivot Table' THEN 'fa6-solid:rectangle-list'
				WHEN vt.Name = 'Pivot Grid' THEN 'fa6-regular:rectangle-list'
				WHEN vt.Name = 'Map' THEN 'fa6-solid:map-location-dot'
				WHEN vt.Name = 'Gantt' THEN 'fa6-solid:chart-gantt'
				WHEN vt.Name = 'Card View' THEN 'fa6-rectangle:id-card'
				ELSE vt.IconHtmlClass
			END [ViewTypeIconHTMLClass]

		FROM
			vwViews v

			INNER JOIN vwViewCategories vc ON
				vc.ID = v.ViewCategoryID

			INNER JOIN vwViewTypes vt ON
				vt.ID = v.ViewTypeID

		WHERE
			v.EntityID = @RecordID

		/** 15 - RECORD HISTORY **/
		SELECT
			'RecordHistory' [EntityDefinitionAppTable],
			erv.ID,
			erv.Changes,
			erv.WhoUpdated,
			erv.DateUpdated

		FROM
			EntityRecordVersions erv

		WHERE
			erv.EntityID = (SELECT TOP 1 ID FROM Entity WHERE Name = 'Entities')
			AND RecordID = @RecordID

		ORDER BY
			DateUpdated DESC

		/** 16 - ARTICLES **/
		SELECT
			'Articles' [EntityDefinitionAppTable],
			a.*,
			LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(a.Title, '''', ''), '  ', ' '), ' ', '-')))) [LookupKey]

		FROM
			EntityDefinitionArticle_sa a

			INNER JOIN EntityDefinitionLinkedArticle_sa la ON
				la.ArticleID = a.ID

			INNER JOIN EntityDefinition_sa ed ON
				ed.ID = la.EntityDefinitionID
				AND ed.EntityID = @RecordID

		/** 17 - HELP LINKS **/
		SELECT
			'HelpLinks' [EntityDefinitionAppTable],
			ehl.*

		FROM
			EntityDefinitionSupportLink_sa ehl

			INNER JOIN EntityDefinition_sa ed ON
				ed.ID = ehl.EntityDefinitionID
				AND ed.EntityID = @RecordID

		/** PROCESS PIPELINE **/
		DROP TABLE IF EXISTS #EntityDefinitions_ProcessPipeline;
		DROP TABLE IF EXISTS #EntityDefinitions_ProcessPipeline_Steps;
		DROP TABLE IF EXISTS #EntityDefinitions_ProcessPipeline_Components;

		/** EVENTS AND PROCESS FLOWS **/
		SELECT
			eh.ID [EventHandlerID],
			eh.IsActive [EventIsActive],
			eh.Sequence [EventSequence],

			ed.ID [DefinitionID],
			ed.Name [DefinitionName],
			RTRIM(ed.Description) [DefinitionDescription],
			RTRIM(ed.EventScope) [DefinitionEventScope],
			ed.BaseEventID [DefinitionBaseEventID],
			(SELECT TOP 1 Name FROM EventDefinition WHERE ID = ed.BaseEventID) [DefinitionBaseEvent],
			ed.FiringSequence [DefinitionFiringSequence],
			CASE WHEN ed.EntityID > 0 THEN ed.EntityID ELSE NULL END [DefinitionEntityID],
			(SELECT TOP 1 Name FROM Entity WHERE ID = ed.EntityID) [DefinitionEntity],
			CASE WHEN ed.ApplicationID > 0 THEN ed.ApplicationID ELSE NULL END [DefinitionApplicationID],
			(SELECT TOP 1 Name FROM Application WHERE ID = ed.ApplicationID) [DefinitionApplication],
			(SELECT TOP 1 ScriptText FROM Script WHERE ID = ed.ScriptID) [DefinitionScriptText],

			pf.ID [ProcessFlowID],
			pf.Name [ProcessFlowName],
			pf.Description [ProcessFlowDescription],
			pf.CategoryID [ProcessFlowCategoryID],
			(SELECT TOP 1 Name FROM ProcessFlowCategory WHERE ID = pf.CategoryID) [ProcessFlowCategory],
			pf.FirstStepID [ProcessFlowFirstStepID],
			(SELECT TOP 1 Name FROM ProcessFlowStep WHERE ID = pf.FirstStepID) [ProcessFlowFirstStep],
			RTRIM(pf.ExecutionMode) [ProcessFlowExecutionMode],
			RTRIM(pf.ExecutionLocation) [ProcessFlowExecutionLocation]

		INTO #EntityDefinitions_ProcessPipeline

		FROM
			EventHandler eh

			INNER JOIN EventDefinition ed ON
				ed.ID = eh.EventID

			INNER JOIN ProcessFlow pf ON
				pf.ID = eh.ProcessFlowID

		WHERE
			eh.EntityID = @RecordID

		/** PROCESS FLOW STEPS **/
		SELECT
			pfs.ID,
			pfs.ProcessFlowID,
			(SELECT TOP 1 Name FROM ProcessFlow WHERE ID = pfs.ProcessFlowID) [ProcessFlowName],
			pfs.Name,
			REPLACE(RTRIM(pfs.Description), '[Step Description]', '') [Description],
			RTRIM(pfs.Type) [Type],
			pfs.ProcessComponentID,
			(SELECT TOP 1 Name FROM ProcessComponent WHERE ID = pfs.ProcessComponentID) [ProcessComponentName],
			pfs.SubProcessFlowID,
			(SELECT TOP 1 Name FROM ProcessFlow WHERE ID = pfs.SubProcessFlowID) [SubProcessFlowName],
			RTRIM(pfs.RuleType) [RuleType],
			pfs.RuleTextScriptID,
			(SELECT TOP 1 ScriptText FROM Script WHERE ID = pfs.RuleTextScriptID) [RuleTextScript],
			pfs.ZOrder

		INTO #EntityDefinitions_ProcessPipeline_Steps

		FROM
			ProcessFlowStep pfs

		WHERE
			pfs.ProcessFlowID IN (
				SELECT ProcessFlowID
				FROM #EntityDefinitions_ProcessPipeline
			)

		/** PROCESS COMPONENTS **/
		SELECT
			pc.ID,
			pc.Name,
			pc.Description,
			pc.ProcessComponentCategoryID,
			(SELECT TOP 1 Name FROM ProcessComponentCategory WHERE ID = pc.ProcessComponentCategoryID) [ProcessComponentCategory],
			RTRIM(pc.Object) [Object],
			RTRIM(pc.AssemblyName) [AssemblyName],
			RTRIM(pc.Class) [Class]

		INTO #EntityDefinitions_ProcessPipeline_Components

		FROM
			ProcessComponent pc

		WHERE
			pc.ID IN (
				SELECT ProcessComponentID
				FROM #EntityDefinitions_ProcessPipeline_Steps
			)

		/** 18 - PIPELINE **/
		SELECT 'Pipeline' [EntityDefinitionAppTable], *
		FROM #EntityDefinitions_ProcessPipeline

		/** 19 - PIPELINE STEPS **/
		SELECT 'PipelineSteps' [EntityDefinitionAppTable], *
		FROM #EntityDefinitions_ProcessPipeline_Steps

		/** 20 - PIPELINE COMPONENTS **/
		SELECT 'PipelineComponents' [EntityDefinitionAppTable], *
		FROM #EntityDefinitions_ProcessPipeline_Components

		/** 21 - PIPELINE INPUTS **/
		SELECT
			'PipelineInputs' [EntityDefinitionAppTable],
			'ProcessFlow' [For],
			pfip.ProcessFlowID [ForID],
			(SELECT TOP 1 Name FROM ProcessFlow WHERE ID = pfip.ProcessFlowID) [ForName],
			pfip.Name,
			pfip.Description,
			RTRIM(pfip.Type) [Type],
			pfip.DefaultValue,
			pfip.IsRequired

		FROM
			ProcessFlowInputProperty pfip

		WHERE
			pfip.ProcessFlowID IN (
				SELECT ProcessFlowID
				FROM #EntityDefinitions_ProcessPipeline
			)

		UNION ALL SELECT
			'PipelineInputs' [EntityDefinitionAppTable],
			'ProcessComponent' [For],
			pcip.ProcessComponentID [ForID],
			(SELECT TOP 1 Name FROM ProcessComponent WHERE ID = pcip.ProcessComponentID) [ForName],
			pcip.Name,
			pcip.Description,
			RTRIM(pcip.Type) [Type],
			pcip.DefaultValue,
			pcip.IsRequired

		FROM
			ProcessFlowCompInput pcip

		WHERE
			pcip.ProcessComponentID IN (
				SELECT ID
				FROM #EntityDefinitions_ProcessPipeline_Components
			)

		/** 22 - PROCESS FLOW OUTPUTS **/
		SELECT
			'PipelineFlowOutputs' [EntityDefinitionAppTable],
			pfop.ID,
			pfop.ProcessFlowID,
			(SELECT TOP 1 Name FROM ProcessFlow WHERE ID = pfop.ProcessFlowID) [ProcessFlowName],
			pfop.Sequence,
			pfop.Name,
			pfop.Description,
			RTRIM(pfop.Type) [Type]

		FROM
			ProcessFlowOutputProperty pfop

		WHERE
			pfop.ProcessFlowID IN (
				SELECT ProcessFlowID
				FROM #EntityDefinitions_ProcessPipeline
			)

		/** 23 - PROCESS COMPONENT OUTPUTS **/
		SELECT
			'PipelineComponentOutputs' [EntityDefinitionAppTable],
			pcip.ID,
			pcip.ProcessComponentID,
			(SELECT TOP 1 Name FROM ProcessComponent WHERE ID = pcip.ProcessComponentID) [ProcessComponentName],
			pcip.Sequence,
			pcip.Name,
			pcip.Description,
			RTRIM(pcip.Type) [Type]

		FROM
			ProcessFlowCompOutput pcip

		WHERE
			pcip.ProcessComponentID IN (
				SELECT ID
				FROM #EntityDefinitions_ProcessPipeline_Components
			)

		/** 24 - PROCESS FLOW RESULT CODES **/
		SELECT
			'PipelineFlowResults' [EntityDefinitionAppTable],
			pfrc.ID,
			pfrc.ProcessFlowID,
			(SELECT TOP 1 Name FROM ProcessFlow WHERE ID = pfrc.ProcessFlowID) [ProcessFlowName],
			pfrc.Sequence,
			RTRIM(pfrc.Code) [Code],
			pfrc.[Description],
			pfrc.IsSuccess

		FROM
			ProcessFlowResultCode pfrc

		WHERE
			pfrc.ProcessFlowID IN (
				SELECT ProcessFlowID
				FROM #EntityDefinitions_ProcessPipeline
			)

		/** 25 - PROCESS COMPONENT RESULT CODES **/
		SELECT
			'PipelineComponentResults' [EntityDefinitionAppTable],
			pcip.ID,
			pcip.ProcessComponentID,
			(SELECT TOP 1 Name FROM ProcessComponent WHERE ID = pcip.ProcessComponentID) [ProcessComponentName],
			pcip.Sequence,
			RTRIM(pcip.Code) [Code],
			pcip.Description,
			pcip.IsSuccess

		FROM
			ProcessFlowCompResultCode pcip

		WHERE
			pcip.ProcessComponentID IN (
				SELECT ID
				FROM #EntityDefinitions_ProcessPipeline_Components
			)


		/** 26 - PROCESS FLOW STEP ACTIONS **/
		SELECT
			'PipelineFlowStepActions' [EntityDefinitionAppTable],
			pfsac.ProcessFlowStepID,
			(SELECT TOP 1 Name FROM ProcessFlowStep WHERE ID = pfsac.ProcessFlowStepID) [ProcessFlowStepName],
			pfsac.Sequence,
			RTRIM(pfsac.ResultCode) [ResultCode],
			RTRIM(pfsac.Action) [Action],
			CASE WHEN pfsac.GoToStepID > 0 THEN pfsac.GoToStepID ELSE NULL END [GoToStepID],
			(SELECT TOP 1 Name FROM ProcessFlowStep WHERE ID = pfsac.GoToStepID) [GoToStepName],
			RTRIM(pfsac.ProcessFlowResultCode) [ProcessFlowResultCode],
        	pfsac.IsSuccess

		FROM
			ProcessFlowStepAction pfsac

		WHERE
			pfsac.ProcessFlowStepID IN (
				SELECT ID
				FROM #EntityDefinitions_ProcessPipeline_Steps
			)

		/** 27 - PROCESS FLOW STEP MAP **/
		SELECT
			'PipelineFlowStepOutputMaps' [EntityDefinitionAppTable],
			pfsom.ID,
			pfsom.ProcessFlowStepID,
			(SELECT TOP 1 Name FROM ProcessFlowStep WHERE ID = pfsom.ProcessFlowStepID) [ProcessFlowStepName],
			pfsom.Sequence,
			RTRIM(pfsom.OutputProperty) [OutputProperty],
			RTRIM(pfsom.Destination) [Destination],
			RTRIM(pfsom.DestinationType) [DestinationType]

		FROM
			ProcessFlowStepOutputMap pfsom

		WHERE
			pfsom.ProcessFlowStepID IN (
				SELECT ID
				FROM #EntityDefinitions_ProcessPipeline_Steps
			)

	END

	ELSE IF @AppContent = 'Generator'
	BEGIN

		/* ENTITIES */
		SELECT
			e.ID,
			TRIM(ISNULL(e.Description, '')) [Description],
			ISNULL((SELECT TOP 1 ID FROM EntityDefinition_sa WHERE EntityID = e.ID), -1) [DefinitionID]

		FROM
			Entities e

		WHERE
			e.ID = @RecordID
			OR @RecordID IS NULL


		/* ENTITY FIELDS */
		SELECT
			ef.EntityID,
			TRIM(ef.Name) [Name],
			TRIM(ISNULL(ef.Description, '')) [Description],
			ef.Sequence

		FROM
			EntityFields ef

		WHERE
			ef.EntityID = @RecordID
			OR @RecordID IS NULL


		/* ENTITY HELP LINKS*/
		SELECT
			hel.EntityID,
  		RTRIM(hel.Url) [Url]

		FROM
			EntityHelpLink hel

		WHERE
			hel.EntityID IS NOT NULL
			AND (
				hel.EntityID = @RecordID
				OR @RecordID IS NULL
		)

	END

END