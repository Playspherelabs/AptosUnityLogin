import express from 'express';
import cors from 'cors';
import { EphemeralKeyPair, Aptos, AptosConfig, Network, AccountAddress, AnyRawTransaction } from '@aptos-labs/ts-sdk';

interface StoredEphemeralKeyPairs {
  [nonce: string]: EphemeralKeyPair;
}

// In-memory storage for key pairs
const storage: StoredEphemeralKeyPairs = {};

// Helper function to store EphemeralKeyPair
const storeEphemeralKeyPair = (ephemeralKeyPair: EphemeralKeyPair): void => {
  storage[ephemeralKeyPair.nonce] = ephemeralKeyPair;
};

const getStoredEphemeralKeyPairs = (): StoredEphemeralKeyPairs => storage;

const app = express();
const PORT = process.env.PORT || 3000;

const toHexString = (byteArray: Uint8Array): string => {
  return Array.prototype.map.call(byteArray, (byte: number) => {
    return ('0' + (byte & 0xFF).toString(16)).slice(-2);
  }).join('');
};

// Middleware to parse JSON request bodies
app.use(express.json());

// Middleware to enable CORS
app.use(cors({
  origin: '*', // Allow all origins. Update this to restrict access to specific origins.
}));

// Generate and return an ephemeral key pair
app.post('/generate-keys', (req, res) => {
  const ephemeralKeyPair = EphemeralKeyPair.generate();
  storeEphemeralKeyPair(ephemeralKeyPair);
  res.json({
    ephemeralKeyPair: {
      data: Array.from(ephemeralKeyPair.bcsToBytes()),
      nonce: ephemeralKeyPair.nonce,
    }
  });
});

// Derive keyless account address
app.post('/derive-keyless-account', async (req, res) => {
  try {
    const { nonce, jwtToken }: { nonce: string; jwtToken: string } = req.body;
    const ephemeralKeyPair = storage[nonce];

    if (!ephemeralKeyPair || !jwtToken) {
      return res.status(400).json({ error: 'Missing ephemeralKeyPair or JWT token' });
    }

    const aptos = new Aptos(new AptosConfig({ network: Network.DEVNET }));
    const keylessAccount = await aptos.deriveKeylessAccount({
      jwt: jwtToken,
      ephemeralKeyPair,
    });
    // await aptos.faucet.fundAccount({
    //   accountAddress: keylessAccount.accountAddress,
    //   amount: 1000000
    // });
    console.log('Keyless account address:', keylessAccount.accountAddress.data);
    
    res.json({ accountAddress: toHexString(keylessAccount.accountAddress.data) });
  } catch (error) {
    console.error('Error deriving keyless account:', error);
    res.status(500).json({ error: error! });
  }
});
app.post('/fund-account', async (req, res) => {
  const { accountAddress, amount } = req.body;
  try {
    const aptos = new Aptos(new AptosConfig({ network: Network.DEVNET }));
    await aptos.faucet.fundAccount({
      accountAddress,
      amount,
    });
    res.json({ status: 'Account funded successfully' });
  } catch (error) {
    console.error('Error funding account:', error);
    res.status(500).json({ error: error! });
  }
});
// Handle transactions
app.post('/submit-transaction', async (req, res) => {
  const { nonce, jwtToken, txn }: {
    nonce: string;
    jwtToken: string;
    txn: { function: `${string}::${string}::${string}`; typeArguments: string[]; functionArguments: any[] }
  } = req.body;

  if (!jwtToken || !txn) {
    return res.status(400).send('Missing jwt or transaction');
  }

  try {
    const aptos = new Aptos(new AptosConfig({ network: Network.DEVNET }));
    const ephemeralKeyPair = storage[nonce];

    if (!ephemeralKeyPair) {
      return res.status(400).send('EphemeralKeyPair not found');
    }

    const keylessAccount = await aptos.deriveKeylessAccount({
      jwt: jwtToken,
      ephemeralKeyPair,
    });

    // build a transaction
const transaction = await aptos.transaction.build.simple({
  sender: keylessAccount.accountAddress,
  data: {
    function: txn.function,
    typeArguments: txn.typeArguments,
    functionArguments: txn.functionArguments,
  },
});

// using sign and submit separately
const senderAuthenticator = aptos.transaction.sign({
  signer: keylessAccount,
  transaction,
});
const committedTransaction = await aptos.transaction.submit.simple({
  transaction,
  senderAuthenticator,
});


    // Create transaction payload
  
 
    const committedTxn = await aptos.waitForTransaction({ transactionHash: committedTransaction.hash });
    res.json(committedTxn);
  } catch (error) {
    res.status(500).json({ error: error! });
  }
});

app.listen(PORT, () => {
  console.log(`Server running at http://localhost:${PORT}`);
});
