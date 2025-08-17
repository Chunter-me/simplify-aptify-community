import fs from 'node:fs';
import path from 'node:path';

export default defineEventHandler(async (event) => {
  const query = getQuery(event);

  const appContent = query.AppContent;
  const recordId = query.RecordID;
  const lookupKey = query.LookupKey;

  const serverDataPath = path.join(process.cwd(), 'server', 'data');
  let serverDataFilePath = '';

  if (appContent == 'EntityList') {
    serverDataFilePath = path.join(serverDataPath, 'entities-list.json');
  }
  else if (appContent == 'Blocks') {
    serverDataFilePath = path.join(serverDataPath, 'blocks.json');
  }
  else if (appContent == 'Article') {
    serverDataFilePath = path.join(serverDataPath, 'Articles', `${lookupKey}.json`);
  }
  else if (appContent == 'Definition') {
    serverDataFilePath = path.join(serverDataPath, 'Definitions', `${recordId}.json`);
  }
  else if (appContent == 'ViewDetails') {
    serverDataFilePath = path.join(serverDataPath, 'ViewDetails', `${recordId}.json`);
  }

  if (serverDataFilePath != '') {
    const data = await fs.promises.readFile(serverDataFilePath, 'utf-8');
    setResponseStatus(event, 200);
    return JSON.parse(data);
  }

  setResponseStatus(event, 204);
  return null;
});