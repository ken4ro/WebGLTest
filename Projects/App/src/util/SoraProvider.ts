import Sora from "sora-js-sdk";

// マルチストリーム送受信（いったん固定で）
export const SoraProvider = () => {
    const channelID = "ken4ro_43071663_webgltest";
    const signalingURL = "wss://0001.stable.sora-labo.shiguredo.app/signaling";
    const accessToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjaGFubmVsX2lkIjoia2VuNHJvXzQzMDcxNjYzX3dlYmdsdGVzdCJ9.DfIrzfcwqGkJJGYN-LFtFgbuMsPwnb1DNIA66gHn3cs";
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
