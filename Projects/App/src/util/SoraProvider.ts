import Sora from "sora-js-sdk";

// マルチストリーム送受信（いったん固定で）
export const SoraProvider = () => {
    const channelID = "ken4ro_43071663_webgltest";
    const signalingURL = "wss://0001.stable.sora-labo.shiguredo.app/signaling";
    const accessToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjaGFubmVsX2lkIjoia2VuNHJvXzQzMDcxNjYzX3dlYmdsdGVzdCIsImV4cCI6MTY4NjgxMDU5OX0.DpKnZUXc8MShA5frDztPd_AL0j43Sack-cTrSWnbOm4";
    const debug = false;
    const sora = Sora.connection(signalingURL, debug);
    const metadata = {
        access_token: accessToken,
    };
    const options = {
        multistream: true,
    };
    const sendrecv = sora.sendrecv(channelID, metadata, options);

    return sendrecv;
};
