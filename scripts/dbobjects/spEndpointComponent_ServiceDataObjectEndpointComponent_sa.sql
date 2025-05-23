/*
NAME: spEndpointComponent_ServiceDataObjectEndpointComponent_sa
DESC: Gets the details needed for the setup and validation of the Service Data Object Endpoint Component.
TYPE: Stored Procedure
GRANT: GRANT EXECUTE ON spEndpointComponent_ServiceDataObjectEndpointComponent_sa TO EndUsers
*/

CREATE PROCEDURE spEndpointComponent_ServiceDataObjectEndpointComponent_sa (
  @SDO NVARCHAR(250),
  @ServiceApplicationName NVARCHAR(250) = 'e-Business'
) AS BEGIN

  -- GET SDO ID
  DECLARE @SDOID INT
  SELECT TOP 1 @SDOID = sdo.ID

  FROM
    ServiceDataObject sdo

    INNER JOIN ServiceDataObjectApplication a ON
      a.ServiceDataObjectID = sdo.ID

    INNER JOIN ServiceApplication sa ON
      sa.ID = a.ServiceApplicationID
      AND sa.Name = @ServiceApplicationName

    INNER JOIN DBObject o ON
      o.ID = sdo.DatabaseObjectID
      AND o.Type = 'Stored Procedure'

  WHERE
    sdo.Name = @SDO


  -- SDO Details
  SELECT
    sdo.SQL [DataObjectName],
    sdo.EnableSecurity [RequiresPrinciple]

  FROM
    vwServiceDataObjects sdo

  WHERE
    sdo.ID = @SDOID


  -- SDO Properties
  SELECT
    p.Name,
    p.IsRequired

  FROM
    ServiceDataObjectParameter p

  WHERE
    p.ServiceDataObjectID = @SDOID

END