"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.decodeEphemeralKeyPairs = exports.parseJWTFromURL = void 0;
// Function to parse JWT from URL
const parseJWTFromURL = (url) => {
    const urlObject = new URL(url);
    const fragment = urlObject.hash.substring(1);
    const params = new URLSearchParams(fragment);
    return params.get('id_token');
};
exports.parseJWTFromURL = parseJWTFromURL;
// Function to decode ephemeral key pairs (example)
const decodeEphemeralKeyPairs = (encodedEphemeralKeyPairs) => {
    // Implement your decoding logic here based on how you've encoded the key pairs
    return JSON.parse(encodedEphemeralKeyPairs);
};
exports.decodeEphemeralKeyPairs = decodeEphemeralKeyPairs;
