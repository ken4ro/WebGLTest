/* eslint-disable @typescript-eslint/no-empty-function */
import React from "react";
import styled from "@emotion/styled";
import { useEffect, useRef, useState } from "react";
import { Button, ButtonGroup } from "@mui/material";
import { GetVolumeNodeFromStream } from "../util/GetVolumeNode";
import { unityInstanceRef } from "./UnityCanvas";
import { SoraProvider } from "../util/SoraProvider";
import { ConnectionPublisher } from "sora-js-sdk";

export const SoraCanvas = () => {
    const [connect, setConnect] = useState<boolean>(false);
    const [sendrecv, setSendrecv] = useState<ConnectionPublisher>();
    const [node, setNode] = useState<AudioWorkletNode>();
    const remoteVideoRef = useRef<HTMLVideoElement>(null);
    const remoteVideoIdRef = useRef<HTMLParagraphElement>(null);
    const volumeTextRef = useRef<HTMLSpanElement>(null);

    // Startボタン処理
    const ClickStartSendRecv = async () => {
        // mediastream接続
        window.console.log("on webrtc_connect event");
        const mediaStream = await window.navigator.mediaDevices.getUserMedia({ audio: true, video: true });
        if (sendrecv) {
            await sendrecv.connect(mediaStream);
        }
        setConnect(true);
    };

    // Stopボタン処理
    const ClickStopSendRecv = async () => {
        // mediastream切断
        window.console.log("on webrtc_dicconnect event");
        if (sendrecv) {
            await sendrecv.disconnect();
        }
        if (remoteVideoRef.current) {
            remoteVideoRef.current.srcObject = null;
        }
        setConnect(false);
    };

    useEffect(() => {
        // Soraインスタンス生成
        const sendrecv = SoraProvider();
        setSendrecv(sendrecv);

        // WebRTC接続ハンドラ
        const Connect = async () => {
            // mediastream接続
            window.console.log("on webrtc_connect event");
            const mediaStream = await window.navigator.mediaDevices.getUserMedia({ audio: true, video: true });
            if (sendrecv) {
                await sendrecv.connect(mediaStream);
            }
            setConnect(true);
        };

        // WebRTC切断ハンドラ
        const Disconnect = async () => {
            // mediastream切断
            window.console.log("on webrtc_dicconnect event");
            if (sendrecv) {
                await sendrecv.disconnect();
            }
            if (remoteVideoRef.current) {
                remoteVideoRef.current.srcObject = null;
            }
            setConnect(false);
        };

        // 接続したチャネルIDにMediaStreamが追加された
        sendrecv.on("track", async (event) => {
            const stream = event.streams[0];
            if (!stream) {
                window.console.log(`on track error!`);
                return;
            }
            if (event.track.kind === "video") {
                // 接続相手のカメラを描画
                window.console.log(`Add mediastream video track: ${stream.id}`);
                if (remoteVideoRef.current) {
                    remoteVideoRef.current.style.border = "1px solid red";
                    remoteVideoRef.current.autoplay = true;
                    remoteVideoRef.current.playsInline = true;
                    remoteVideoRef.current.controls = true;
                    remoteVideoRef.current.width = 160;
                    remoteVideoRef.current.height = 120;
                    remoteVideoRef.current.srcObject = stream;
                }
                if (remoteVideoIdRef.current) {
                    remoteVideoIdRef.current.innerText = stream.id;
                }
            } else if (event.track.kind === "audio") {
                window.console.log(`Add mediastream audio track: ${stream.id}`);
                // 接続相手のマイク音量をUnityに送信
                const node = await GetVolumeNodeFromStream(stream);
                node.port.onmessage = (ev) => {
                    const volume = ev.data.volume;
                    if (unityInstanceRef.current) {
                        unityInstanceRef.current.SendMessage("GameManager", "SetVoiceVolume", volume * 10);
                    }
                    if (volumeTextRef.current) {
                        volumeTextRef.current.innerText = volume;
                    }
                };
                setNode(node);
            }
        });

        // 接続したチャネルIDからMediaStreamが削除された
        sendrecv.on("removetrack", (event) => {
            // リモートビデオ再生停止
            if (event.target) {
                // window.console.log(`Remove mediastream track: ${event.target.id}`);
            }
            if (remoteVideoRef.current) {
                remoteVideoRef.current.srcObject = null;
            }
            if (node) {
                node.port.onmessage = () => {
                    if (volumeTextRef.current) {
                        volumeTextRef.current.innerText = "";
                    }
                };
                setNode(undefined);
            }
        });

        // WebRTCイベント購読(Unityから発行)
        window.addEventListener("webrtc_connect", Connect);
        window.addEventListener("webrtc_disconnect", Disconnect);

        Connect();

        // クリーンアップ
        return () => {
            // WebRTCイベント購読解除
            if (sendrecv) {
                sendrecv.on("track", () => {});
            }
            window.removeEventListener("webrtc_connect", Connect);
            window.removeEventListener("webrtc_disconnect", Disconnect);
        };
    }, []);

    return (
        <SContainer>
            <SVideoLayout>
                <ButtonGroup variant="contained" aria-label="outlined primary button group">
                    {/* <Button disabled={connect} onClick={ClickStartSendRecv}>
                        start
                    </Button> */}
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
