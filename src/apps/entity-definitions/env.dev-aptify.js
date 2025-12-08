import fs from 'fs';
import fetch from 'node-fetch';
import https from 'https';
import readline from 'readline';

let soa = '';
const envFile = '.env.dev-aptify';
const httpsAgent = new https.Agent({ rejectUnauthorized: false });

function prompt(query) {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
  });

  return new Promise(resolve => rl.question(query, ans => {
    rl.close();
    resolve(ans);
  }));
}

function getValueFromEnv(value) {
  if (!fs.existsSync(envFile)) return null;

  const env = fs.readFileSync(envFile, 'utf8');
  const match = env.match(new RegExp(`^${value}=(.*)$`, 'm'));

  return match ? match[1] : null;
}

async function main() {
  soa = getValueFromEnv('BASE_APTIFY_SOA_URL');
  let token = getValueFromEnv('APTIFY_SOA_TOKEN');

  const baseRequest = {
    method: 'GET',
    credentials: 'include',
    mode: 'cors',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  };

  const userInfoResponse = await fetch(`${soa}/UserInformation`, {
    ...baseRequest,
    cache: 'no-store',
    headers: { AptifyAuthorization: token },
    agent: httpsAgent
  });

  // const x = userInfoResponse.headers.get('CSRFDefenseInDepthToken');
  // const y = userInfoResponse.headers.get('__RequestVerificationToken');

  const userInfo = await userInfoResponse.json();
  if (userInfo?.result == 'ok') {
    console.log('Existing token is valid.');
    return;
  }

  const username = await prompt('Username: ');
  const password = await prompt('Password: ');

  try {
    const response = await fetch(`${soa}/Authentication/Login/SQLNeedsInputWithContainer?username=${username}&password=${password}`, {
      ...baseRequest,
      cache: 'no-store',
      agent: httpsAgent
    });

    const data = await response.json();
    console.log('Response: ', data);
    token = data.TokenId;

    if (token) {
      let env = '';
      if (fs.existsSync(envFile)) {
        env = fs.readFileSync(envFile, 'utf8');
        env = env.replace(/^APTIFY_SOA_TOKEN=.*$/m, '');
      }

      env += `APTIFY_SOA_TOKEN=SQLNeedsInputWithContainer ${token}`;

      fs.writeFileSync(envFile, env.trim());
      console.log('Token updated in .env.dev');
    }
    else {
      throw 'No token returned.';
    }

  } catch (err) {
    console.error('Failed to fetch token:', err);
    process.exit(1);
  }
}

main();