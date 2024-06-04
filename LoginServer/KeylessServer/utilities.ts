import { EphemeralKeyPair } from '@aptos-labs/ts-sdk';

// Function to parse JWT from URL
export const parseJWTFromURL = (url: string): string | null => {
  const urlObject = new URL(url);
  const fragment = urlObject.hash.substring(1);
  const params = new URLSearchParams(fragment);
  return params.get('id_token');
};

// Function to decode ephemeral key pairs (example)
export const decodeEphemeralKeyPairs = (encodedEphemeralKeyPairs: string): any => {
  // Implement your decoding logic here based on how you've encoded the key pairs
  return JSON.parse(encodedEphemeralKeyPairs);
};
