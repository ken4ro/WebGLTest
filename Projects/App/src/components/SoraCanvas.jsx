import styled from "@emotion/styled";
import { useEffect, useRef, useState } from "react";
import { useSora } from "../hooks/useSora";
import { Button, ButtonGroup } from "@mui/material";
import { AudioProcess } from "../util/AudioProcess";
import { unityInstanceRef } from "./UnityCanvas";

let count = 0;
export const SoraCanvas = () => {
    console.log(`SoraCanvas render count = ${count++}`);
    const [connect, setIsConnect] = useState(false);
    const remoteVideoRef = useRef(null);
    const remoteVideoIdRef = useRef(null);
    const volumeTextRef = useRef(null);

    // 接続したチャネルIDにMediaStreamが追加された
    const { sendrecv } = useSora();
    sendrecv.on("track", async (event) => {
        const stream = event.streams[0];
        if (!stream) return;
        console.log(`Add mediastream track: ${stream.id}`);
        remoteVideoRef.current.style.border = "1px solid red";
        remoteVideoRef.current.autoplay = true;
        remoteVideoRef.current.playsinline = true;
        remoteVideoRef.current.controls = true;
        remoteVideoRef.current.width = "160";
        remoteVideoRef.current.height = "120";
        remoteVideoRef.current.srcObject = stream;
        remoteVideoIdRef.current.innerText = stream.id;
        // 接続相手のマイク音量をUnityに送信
        const node = await AudioProcess(stream);
        node.port.onmessage = (event) => {
            if (connect === true) {
                const volume = event.data.volume;
                unityInstanceRef.current.SendMessage("GameManager", "SetVoiceVolume", volume * 10);
                volumeTextRef.current.innerText = volume;
            }
        };
    });

    // 接続したチャネルIDからMediaStreamが削除された
    sendrecv.on("removetrack", (event) => {
        // リモートビデオ再生停止
        console.log(`Remove mediastream track: ${event.target.id}`);
        remoteVideoRef.current.srcObject = null;
    });

    // Startボタン処理
    const ClickStartSendRecv = async () => {
        // mediastream接続
        const mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
        await sendrecv.connect(mediaStream);
        setIsConnect(true);
    };

    // Stopボタン処理
    const ClickStopSendRecv = async () => {
        // mediastream切断
        await sendrecv.disconnect();
        setIsConnect(false);
    };

    useEffect(() => {
        // WebRTC接続ハンドラ
        const Connect = async () => {
            // mediastream接続
            console.log("on webrtc_connect event");
            const mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });
            await sendrecv.connect(mediaStream);
            setIsConnect(true);
        };

        // WebRTC切断ハンドラ
        const Disconnect = async () => {
            // mediastream切断
            console.log("on webrtc_dicconnect event");
            remoteVideoRef.current.srcObject = null;
            setIsConnect(false);
        };

        // WebRTCイベント購読(Unityから発行)
        console.log("addEventListener webrtc_connect");
        window.addEventListener("webrtc_connect", Connect);
        window.addEventListener("webrtc_disconnect", Disconnect);

        // クリーンアップ
        return () => {
            // WebRTCイベント購読解除
            console.log("removeEventListener webrtc_connect");
            window.removeEventListener("webrtc_connect", Connect);
            window.removeEventListener("webrtc_disconnect", Disconnect);
        };
    }, []);

    return (
        <SContainer>
            <SVideoLayout>
                <ButtonGroup variant="contained" aria-label="outlined primary button group">
                    <Button disabled={connect} onClick={ClickStartSendRecv}>
                        start
                    </Button>
                    <Button disabled={!connect} onClick={ClickStopSendRecv}>
                        stop
                    </Button>
                </ButtonGroup>
                <br />
                <SVideo ref={remoteVideoRef}></SVideo>
                <p ref={remoteVideoIdRef}>Remote id</p>
                <p>
                    Volume: <span ref={volumeTextRef} />
                </p>
            </SVideoLayout>
        </SContainer>
    );
};

const SContainer = styled.div`
    width: 800px;
    align-items: center;
    text-align: center;
    margin-top: 30px;
    margin-left: auto;
    margin-right: auto;
`;

const SVideoLayout = styled.div``;

const SVideo = styled.video`
    margin-top: 1px;
    width: 320px;
    height: 240px;
    border: 1px solid black;
`;
