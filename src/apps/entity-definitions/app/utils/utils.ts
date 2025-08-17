import xmlFormat from 'xml-formatter';

export const aptifyWebBaseUrl: string = `${window.location.origin}/Aptify`;

export const sleep = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const logDev = (message: any, obj?: any) => {
  if (process.env.NODE_ENV == 'development') {
    if (obj) {
      console.log(message, obj);
    }
    else {
      console.log(message);
    }
  }
}

export const parseDateString = (dateString: string): Date => {
  const [month, day, year] = dateString.split('/').map(Number);
  return new Date(year, month - 1, day);
};

export const toTitleCase = (str: string): string => {
  return str.replace(/\w\S*/g, function(txt) {
    return txt.charAt(0).toUpperCase() + txt.substring(1).toLowerCase();
  });
}

export const isNumber = (value: string | number): boolean => {
  if (typeof value === 'number') {
    return true; // Already a number
  }

  if (typeof value !== 'string') {
    return false; // Not a string or number, so it's not a number
  }

  if (value.trim() === '') {
    return false; // Empty string is not a number
  }

  return !isNaN(Number(value));
}

export const sqlDataTypeDescription = (type: string) => {
  if (type != undefined && type != null && type.trim() != '') {
    type = type.toLowerCase().trim();

    if (type == 'bigint') { return '<strong>BigInt:</strong> A large integer data type for storing values ranging from -2^63 to 2^63-1.' }
    else if (type == 'bit') { return '<strong>Bit:</strong> A data type used to store Boolean values, represented by 0 (false) or 1 (true).' }
    else if (type == 'char') { return '<strong>Char:</strong> A fixed-length character data type used for storing non-Unicode characters.' }
    else if (type == 'datetime') { return '<strong>Datetime:</strong> A date and time data type with a range from January 1, 1753, to December 31, 9999.' }
    else if (type == 'decimal') { return '<strong>Decimal:</strong> A fixed precision and scale numeric data type for exact numeric values.' }
    else if (type == 'float') { return '<strong>Float:</strong> A floating-point numeric data type used to store approximate values with floating decimal precision.' }
    else if (type == 'int') { return '<strong>Int:</strong> A standard integer data type that can store values from -2^31 to 2^31-1.' }
    else if (type == 'money') { return '<strong>Money:</strong> A data type for storing monetary values with 4 decimal places.' }
    else if (type == 'nchar') { return '<strong>NChar:</strong> A fixed-length Unicode character data type for storing multi-language characters.' }
    else if (type == 'numeric') { return '<strong>Numeric:</strong> An exact numeric data type similar to Decimal, with user-defined precision and scale.' }
    else if (type == 'nvarchar' || type == 'nvarchar(max)') { return '<strong>NVarchar:</strong> A variable-length Unicode character data type for storing multi-language text.' }
    else if (type == 'real') { return '<strong>Real:</strong> A single-precision floating-point numeric data type used for approximate numeric values.' }
    else if (type == 'tinyint') { return '<strong>TinyInt:</strong> A small integer data type for storing values from 0 to 255.' }
    else if (type == 'uniqueidentifier') { return '<strong>UniqueIdentifier:</strong> A globally unique identifier (GUID) used for unique keys.' }
    else if (type == 'varbinary(max)') { return '<strong>VarBinary:</strong> A variable-length binary data type used for storing raw binary data.' }
    else if (type == 'varchar' || type == 'varchar(max)') { return '<strong>VarChar:</strong> A variable-length character data type used for storing non-Unicode text.' }
  }

  return '';
}

export const tableDataKeywordNameDisplayFilter = (data: Array<any>, keyword: string, keywordFields: Array<string> = [ 'Name', 'DisplayName' ]): Array<any> => {
  keyword = keyword?.toLowerCase().trim() ?? '';

  if (!keywordFields || keywordFields.length == 0) {
    keywordFields = [ 'Name', 'DisplayName' ];
  }

  if (data && data.length > 0 && keyword.length > 2) {
    return data.filter((d: any) => keywordFields.some(field => d[field] && d[field].toLowerCase().includes(keyword)));
  }

  return data;
}

export const tableColumnSortClick = (column: any, desc?: boolean) => {
  if (desc == undefined) {
    column.clearSorting();
  }
  else {
    column.toggleSorting(desc);
  }
}

export const markdownWrapSqlCode = (sql: string) => {
  return markdownWrapCode('sql', sql);
}

export const markdownWrapJsonCode = (json: string) => {
  return markdownWrapCode('json', json);
}

export const markdownWrapFormalXmlCode = (xml: string) => {
  const formatted = xmlFormat(xml);
  return markdownWrapCode('xml', formatted);
}

export const markdownWrapCode = (type: string, content: string) => {
  return `\`\`\`${type}\n${content ?? ''}\n\`\`\`` ;
}

export const objectToPropsKeyValue = (obj: any): Array<{key: string, value: string}> => {
  return Object.entries(obj).map((prop: Array<any>) => {
    return {
      key: String(prop[0]).trim(),
      value: String(prop[1]).trim()
    }
  })
  .sort((a: any, b: any) => a.key.toLowerCase().localeCompare(b.key.toLowerCase()));
}

export const uTableHeaderMetaCenter = {
  class: {
    th: 'whitespace-nowrap text-center',
    td: 'text-center'
  }
};