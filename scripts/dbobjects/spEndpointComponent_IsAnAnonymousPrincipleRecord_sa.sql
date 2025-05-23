/*
NAME: spEndpointComponent_IsAnAnonymousPrincipleRecord_sa
DESC: Returns True/False if the given Principle Record Id the Anonymous record for the given Service Application or a Anonymous record for any if no Service Application provided.
TYPE: Stored Procedure
GRANT: GRANT EXECUTE ON spEndpointComponent_IsAnAnonymousPrincipleRecord_sa TO EndUsers
*/

CREATE PROCEDURE spEndpointComponent_IsAnAnonymousPrincipleRecord_sa (
  @AuthenticatedPrincipalRecordId INT,
  @ServiceApplicationName NVARCHAR(50) = NULL
) AS BEGIN

  SELECT CAST(ISNULL((
    SELECT TOP 1 1
    FROM ServiceApplication sa
    WHERE sa.AnonymousPerson = @AuthenticatedPrincipalRecordId
    AND (RTRIM(ISNULL(@ServiceApplicationName, '')) = '' OR sa.Name = @ServiceApplicationName)
  ), 0) AS BIT)

END