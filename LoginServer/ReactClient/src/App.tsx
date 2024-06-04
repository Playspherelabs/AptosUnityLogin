import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [keylessAccountAddress, setKeylessAccountAddress] = useState<string | null>(null);
  const [recipientAddress, setRecipientAddress] = useState<string>('');
  const [amount, setAmount] = useState<number>(0);
  const [fundAmount, setFundAmount] = useState<number>(1000000);

  // Helper function to parse JWT from URL
  const parseJWTFromURL = (): string | null => {
    const url = window.location.href;
    const params = new URLSearchParams(url.split('#')[1]);
    return params.get('id_token');
  };

  // Check URL on load and store JWT if present
  useEffect(() => {
    const jwt = parseJWTFromURL();
    if (jwt) {
      localStorage.setItem('id_token', jwt);
      window.history.replaceState({}, document.title, window.location.pathname);
      deriveKeylessAccountAddress(jwt);
    }
  }, []);

  const deriveKeylessAccountAddress = async (jwt: string) => {
    try {
      const storedEphemeralKeyPair = localStorage.getItem('ephemeralKeyPair');
      if (!storedEphemeralKeyPair) {
        throw new Error('No ephemeral key pair found in local storage');
      }

      const ephemeralKeyPair = JSON.parse(storedEphemeralKeyPair);

      const response = await fetch('http://localhost:3000/derive-keyless-account', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ nonce: ephemeralKeyPair.nonce, jwtToken: jwt }),
      });

      const { accountAddress } = await response.json();
      setKeylessAccountAddress(accountAddress);
    } catch (error) {
      console.error('Error deriving keyless account address:', error);
    }
  };

  const handleAuthClick = async () => {
    try {
      const response = await fetch('http://localhost:3000/generate-keys', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      const { ephemeralKeyPair } = await response.json();

      localStorage.setItem('ephemeralKeyPair', JSON.stringify(ephemeralKeyPair));
      const nonce = ephemeralKeyPair.nonce;

      const clientId = '<CLIENT_ID>'; // Replace with your Google client ID
      const redirectUri = 'http://localhost:5173';
      const authURL = `https://accounts.google.com/o/oauth2/v2/auth?response_type=id_token&scope=openid+email+profile&nonce=${nonce}&redirect_uri=${redirectUri}&client_id=${clientId}`;
      window.location.href = authURL;
    } catch (error) {
      console.error('Error generating keys:', error);
    }
  };

  const handleFundAccountClick = async () => {
    if (keylessAccountAddress) {
      try {
        const response = await fetch('http://localhost:3000/fund-account', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            accountAddress: keylessAccountAddress,
            amount: fundAmount,
          }),
        });
        const result = await response.json();
        console.log('Fund account result:', result);
      } catch (error) {
        console.error('Error funding account:', error);
      }
    } else {
      console.error('Keyless account address is not available.');
    }
  };

  const handleTransferClick = async () => {
    try {
      const jwt = localStorage.getItem('id_token');
      const storedEphemeralKeyPair = localStorage.getItem('ephemeralKeyPair');
      if (!jwt || !keylessAccountAddress || !storedEphemeralKeyPair) {
        throw new Error('Missing JWT or Keyless Account Address or EphemeralKeyPair');
      }

      const ephemeralKeyPair = JSON.parse(storedEphemeralKeyPair);

      const response = await fetch('http://localhost:3000/submit-transaction', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          nonce: ephemeralKeyPair.nonce,
          jwtToken: jwt,
          txn: {
            function: "0x1::coin::transfer",
            typeArguments: ["0x1::aptos_coin::AptosCoin"],
            functionArguments: [recipientAddress, amount],
          },
        }),
      });

      const result = await response.json();
      console.log('Transaction result:', result);
    } catch (error) {
      console.error('Error submitting transaction:', error);
    }
  };

  return (
    <>
      <div>
        <h1>Aptos Keyless Account Demo</h1>
      </div>
      <div className="card">
        <button onClick={handleAuthClick}>Authenticate with Google</button>
        {keylessAccountAddress && (
          <>
            <p>Keyless Account Address: {keylessAccountAddress}</p>
            <div>
              <h3>Fund Account</h3>
              <input 
                type="number" 
                placeholder="Fund Amount" 
                value={fundAmount} 
                onChange={(e) => setFundAmount(parseInt(e.target.value, 10))} 
              />
              <button onClick={handleFundAccountClick}>Fund Account</button>
            </div>
            <div>
              <h3>Transfer Coins</h3>
              <input 
                type="text" 
                placeholder="Recipient Address" 
                value={recipientAddress} 
                onChange={(e) => setRecipientAddress(e.target.value)} 
              />
              <input 
                type="number" 
                placeholder="Amount" 
                value={amount} 
                onChange={(e) => setAmount(parseInt(e.target.value, 10))} 
              />
              <button onClick={handleTransferClick}>Transfer Coins</button>
            </div>
          </>
        )}
      </div>
    </>
  );
}

export default App;
