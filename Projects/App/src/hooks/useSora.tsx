import Sora from "sora-js-sdk";

const channelID = "test@ken4ro#43071663";
const signalingURL = "wss://0001.stable.sora-labo.shiguredo.app/signaling";
const accessToken =
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjaGFubmVsX2lkIjoidGVzdEBrZW40cm8jNDMwNzE2NjMifQ.n2YNOmgUwzCBz_e0Q5oZf9-6oQIDqvuFtvjHm7jQcWk";
const debug = false;
const sora = Sora.connection(signalingURL, debug);

// マルチストリーム送受信（いったん固定で）
export const useSora = () => {
    const metadata = {
        access_token: accessToken,
    };
    const options = {
        multistream: true,
    };
    const sendrecv = sora.sendrecv(channelID, metadata, options);

    return { sendrecv };
};
