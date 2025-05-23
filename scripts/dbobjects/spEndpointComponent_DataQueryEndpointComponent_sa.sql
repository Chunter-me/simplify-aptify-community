/*
NAME: spEndpointComponent_DataQueryEndpointComponent_sa
DESC: Gets the details needed for the setup and validation of the Data Query Endpoint Component.
TYPE: Stored Procedure
GRANT: GRANT EXECUTE ON spEndpointComponent_DataQueryEndpointComponent_sa TO EndUsers
*/

CREATE PROCEDURE spEndpointComponent_DataQueryEndpointComponent_sa (
  @DataObjectName NVARCHAR(250)
) AS BEGIN

  -- DB Object Type
  SELECT RTRIM(d.Type) [Type]
  FROM DBObject d
  WHERE d.Name = @DataObjectName


  -- Sproc Parameters
  SELECT
    pa.parameter_id [Sequence],
    pa.name [Name],
    UPPER(t.name) [Type],
    t.is_nullable [Nullable],
    t.max_length [Length]

  FROM
    sys.parameters pa

    INNER JOIN sys.procedures p on
      pa.object_id = p.object_id

    INNER JOIN sys.types t on
      pa.system_type_id = t.system_type_id
      AND pa.user_type_id = t.user_type_id

  WHERE
    p.name = @DataObjectName

  ORDER BY
    pa.parameter_id

END