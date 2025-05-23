/*
NAME: spEndpointComponent_DataQuery_Examples_sa
DESC: Examples of all the Data Query uses.
TYPE: Stored Procedure
GRANT: GRANT EXECUTE ON spEndpointComponent_DataQuery_Examples_sa TO EndUsers
*/

CREATE PROCEDURE spEndpointComponent_DataQuery_Examples_sa (
  @AuthenticatedPrincipalRecordId INT,
  @Example NVARCHAR(50) = NULL,
  @LimitValue NVARCHAR(50) = NULL
) AS BEGIN

  SELECT @Example = CASE
    WHEN @Example IS NULL OR RTRIM(@Example) = '' THEN 'SingleRecord'
    ELSE @Example
  END

  IF @Example = 'SingleRecord'
  BEGIN
    SELECT ID, FirstName, LastName
    FROM Person
    WHERE ID = @AuthenticatedPrincipalRecordId;
  END

  IF @Example = 'MultipleRecords'
  BEGIN
    SELECT TOP 10 ID, FirstName, LastName
    FROM Person
    ORDER BY NEWID()
  END

  IF @Example = 'SingleRecordTableConfig'
  BEGIN
    SELECT
      tables.TableName,
      tables.IsCollection

    FROM (
      SELECT 'Profile' [TableName], CAST(0 AS BIT) [IsCollection], 0 [Sequence]
    ) tables

    ;WITH OtherPersons AS (
      SELECT TOP 10 ID, FirstName, LastName, CAST(CASE WHEN ID = @AuthenticatedPrincipalRecordId THEN 1 ELSE 0 END AS BIT) [IsAuthenticatedPrincipalRecord]
      FROM Person
      WHERE ID != @AuthenticatedPrincipalRecordId
      ORDER BY NEWID()
    )
    SELECT TOP 10 ID, FirstName, LastName, CAST(CASE WHEN ID = @AuthenticatedPrincipalRecordId THEN 1 ELSE 0 END AS BIT) [IsAuthenticatedPrincipalRecord]
    FROM Person
    WHERE ID = @AuthenticatedPrincipalRecordId
    UNION ALL SELECT * FROM OtherPersons
  END

  IF @Example = 'MultipleRecordsTableConfig'
  BEGIN
    SELECT
      tables.TableName,
      tables.IsCollection

    FROM (
      SELECT 'Persons' [TableName], CAST(1 AS BIT) [IsCollection], 0 [Sequence]
    ) tables

    SELECT TOP 10 ID, FirstName, LastName
    FROM Person
    ORDER BY NEWID()
  END

  IF @Example = 'MultipleDatasets'
  BEGIN
    SELECT TOP 10 ID, FirstName, LastName
    FROM Person
    ORDER BY NEWID()

    SELECT c.Country, c.ISOCode, c.ID
    FROM Country c
    WHERE @LimitValue IS NULL OR CAST(c.ID AS NVARCHAR(50)) = @LimitValue

    SELECT RTRIM(sp.Abbreviation) [Abbr], sp.FullName, sp.CountryID
    FROM StateProvince sp
    WHERE @LimitValue IS NULL OR CAST(sp.CountryID AS NVARCHAR(50)) = @LimitValue
  END

  IF @Example = 'MultipleDatasetsTableConfig'
  BEGIN
    SELECT
      tables.TableName,
      tables.IsCollection

    FROM (
      SELECT 'Persons' [TableName], CAST(1 AS BIT) [IsCollection], 0 [Sequence]
      UNION SELECT 'Countries' [TableName], CAST(1 AS BIT) [IsCollection], 1 [Sequence]
      UNION SELECT 'StateProvinces' [TableName], CAST(1 AS BIT) [IsCollection], 2 [Sequence]
    ) tables

    ORDER BY tables.Sequence

    SELECT TOP 10 ID, FirstName, LastName
    FROM Person
    ORDER BY NEWID()

    SELECT c.Country, c.ISOCode, c.ID
    FROM Country c
    WHERE @LimitValue IS NULL OR CAST(c.ID AS NVARCHAR(50)) = @LimitValue

    SELECT RTRIM(sp.Abbreviation) [Abbr], sp.FullName, sp.CountryID
    FROM StateProvince sp
    WHERE @LimitValue IS NULL OR CAST(sp.CountryID AS NVARCHAR(50)) = @LimitValue
  END

  IF @Example = 'MultipleDatasetsSingleMultipleTableConfig'
  BEGIN
    SELECT
      tables.TableName,
      tables.IsCollection

    FROM (
      SELECT 'Persons' [TableName], CAST(0 AS BIT) [IsCollection], 0 [Sequence]
      UNION SELECT 'Countries' [TableName], CAST(1 AS BIT) [IsCollection], 1 [Sequence]
      UNION SELECT 'StateProvinces' [TableName], CAST(1 AS BIT) [IsCollection], 2 [Sequence]
    ) tables

    ORDER BY tables.Sequence

    SELECT ID, FirstName, LastName
    FROM Person
    WHERE ID = @AuthenticatedPrincipalRecordId;

    SELECT c.Country, c.ISOCode, c.ID
    FROM Country c
    WHERE @LimitValue IS NULL OR CAST(c.ID AS NVARCHAR(50)) = @LimitValue

    SELECT RTRIM(sp.Abbreviation) [Abbr], sp.FullName, sp.CountryID
    FROM StateProvince sp
    WHERE @LimitValue IS NULL OR CAST(sp.CountryID AS NVARCHAR(50)) = @LimitValue
  END

  IF @Example = 'ForJsonPath01'
  BEGIN
    SELECT (
      SELECT
        ID,
        FirstName,
        LastName,

        Countries = (
          SELECT c.Country, c.ISOCode, c.ID
          FROM Country c
          WHERE @LimitValue IS NULL OR CAST(c.ID AS NVARCHAR(50)) = @LimitValue
          FOR JSON PATH
        ),

        StateProvinces = (
          SELECT RTRIM(sp.Abbreviation) [Abbr], sp.FullName, sp.CountryID
          FROM StateProvince sp
          WHERE @LimitValue IS NULL OR CAST(sp.CountryID AS NVARCHAR(50)) = @LimitValue
          FOR JSON PATH
        )

      FROM
        Person

      WHERE
        ID = @AuthenticatedPrincipalRecordId

      FOR JSON PATH,
      INCLUDE_NULL_VALUES,
      WITHOUT_ARRAY_WRAPPER
    ) [JsonOutput]
  END

  IF @Example = 'ForJsonPath02'
  BEGIN
    SELECT (
      SELECT
        Profile = JSON_QUERY((
          SELECT
            ID,
            FirstName,
            LastName
          FROM Person
          WHERE ID = @AuthenticatedPrincipalRecordId
          FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )),
        Countries = (
          SELECT c.Country, c.ISOCode, c.ID
          FROM Country c
          WHERE @LimitValue IS NULL OR CAST(c.ID AS NVARCHAR(50)) = @LimitValue
          FOR JSON PATH
        ),
        StateProvinces = (
          SELECT RTRIM(sp.Abbreviation) [Abbr], sp.FullName, sp.CountryID
          FROM StateProvince sp
          WHERE @LimitValue IS NULL OR CAST(sp.CountryID AS NVARCHAR(50)) = @LimitValue
          FOR JSON PATH
        )

      FOR JSON PATH,
      INCLUDE_NULL_VALUES,
      WITHOUT_ARRAY_WRAPPER
    ) [JsonOutput]
  END

END