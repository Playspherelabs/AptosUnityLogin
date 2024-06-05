"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express_1 = __importDefault(require("express"));
const cors_1 = __importDefault(require("cors"));
const ts_sdk_1 = require("@aptos-labs/ts-sdk");
// In-memory storage for key pairs
const storage = {};
// Helper function to store EphemeralKeyPair
const storeEphemeralKeyPair = (ephemeralKeyPair) => {
    storage[ephemeralKeyPair.nonce] = ephemeralKeyPair;
};
const getStoredEphemeralKeyPairs = () => storage;
const app = (0, express_1.default)();
const PORT = process.env.PORT || 3000;
const toHexString = (byteArray) => {
    return Array.prototype.map.call(byteArray, (byte) => {
        return ('0' + (byte & 0xFF).toString(16)).slice(-2);
    }).join('');
};
// Middleware to parse JSON request bodies
app.use(express_1.default.json());
// Middleware to enable CORS
app.use((0, cors_1.default)({
    origin: '*', // Allow all origins. Update this to restrict access to specific origins.
}));
// Generate and return an ephemeral key pair
app.post('/generate-keys', (req, res) => {
    const ephemeralKeyPair = ts_sdk_1.EphemeralKeyPair.generate();
    storeEphemeralKeyPair(ephemeralKeyPair);
    res.json({
        ephemeralKeyPair: {
            data: Array.from(ephemeralKeyPair.bcsToBytes()),
            nonce: ephemeralKeyPair.nonce,
        }
    });
});
// Derive keyless account address
app.post('/derive-keyless-account', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    try {
        const { nonce, jwtToken } = req.body;
        const ephemeralKeyPair = storage[nonce];
        if (!ephemeralKeyPair || !jwtToken) {
            return res.status(400).json({ error: 'Missing ephemeralKeyPair or JWT token' });
        }
        const aptos = new ts_sdk_1.Aptos(new ts_sdk_1.AptosConfig({ network: ts_sdk_1.Network.DEVNET }));
        const keylessAccount = yield aptos.deriveKeylessAccount({
            jwt: jwtToken,
            ephemeralKeyPair,
        });
        // await aptos.faucet.fundAccount({
        //   accountAddress: keylessAccount.accountAddress,
        //   amount: 1000000
        // });
        console.log('Keyless account address:', keylessAccount.accountAddress.data);
        res.json({ accountAddress: toHexString(keylessAccount.accountAddress.data) });
    }
    catch (error) {
        console.error('Error deriving keyless account:', error);
        res.status(500).json({ error: error });
    }
}));
app.post('/fund-account', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { accountAddress, amount } = req.body;
    try {
        const aptos = new ts_sdk_1.Aptos(new ts_sdk_1.AptosConfig({ network: ts_sdk_1.Network.DEVNET }));
        yield aptos.faucet.fundAccount({
            accountAddress,
            amount,
        });
        res.json({ status: 'Account funded successfully' });
    }
    catch (error) {
        console.error('Error funding account:', error);
        res.status(500).json({ error: error });
    }
}));
// Handle transactions
app.post('/submit-transaction', (req, res) => __awaiter(void 0, void 0, void 0, function* () {
    const { nonce, jwtToken, txn } = req.body;
    if (!jwtToken || !txn) {
        return res.status(400).send('Missing jwt or transaction');
    }
    try {
        const aptos = new ts_sdk_1.Aptos(new ts_sdk_1.AptosConfig({ network: ts_sdk_1.Network.DEVNET }));
        const ephemeralKeyPair = storage[nonce];
        if (!ephemeralKeyPair) {
            return res.status(400).send('EphemeralKeyPair not found');
        }
        const keylessAccount = yield aptos.deriveKeylessAccount({
            jwt: jwtToken,
            ephemeralKeyPair,
        });
        // build a transaction
        const transaction = yield aptos.transaction.build.simple({
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
        const committedTransaction = yield aptos.transaction.submit.simple({
            transaction,
            senderAuthenticator,
        });
        // Create transaction payload
        const committedTxn = yield aptos.waitForTransaction({ transactionHash: committedTransaction.hash });
        res.json(committedTxn);
    }
    catch (error) {
        res.status(500).json({ error: error });
    }
}));
app.listen(PORT, () => {
    console.log(`Server running at http://localhost:${PORT}`);
});
