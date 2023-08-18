/* eslint-disable @typescript-eslint/no-var-requires */
// react
import { useEffect, useState } from "react";
// import styled from "styled-components";
import styled from "@emotion/styled";
import SpeechRecognition, { useSpeechRecognition } from "react-speech-recognition";
import { Button } from "@mui/material";
import { useUnity } from "../hooks/useUnity";
import { socket } from "../util/SocketProvider";

type Props = {
    width?: number;
    height?: number;
};

type SendUserTokenType = {
    token: string;
};

type NotificationMessage = {
    type: string;
    from_id: string;
};

type ResponseMessage = {
    method: string;
    result: string;
    message: string;
};

type RelayToOperatorType = {
    message: string;
};

const unityBuildRoot = "./Build";
const buildName = "docs";
export let unityInstanceRef: React.MutableRefObject<UnityInstance | undefined>;

export const UnityCanvas = ({ width, height }: Props) => {
    const [startBtnEnabled, setStartBtnEnabled] = useState(false);
    const [stopBtnEnabled, setStopBtnEnabled] = useState(false);
    const [recognitionBtnEnabled, setRecognitionBtnEnabled] = useState(true);
    const [userToken, setUserToken] = useState("");

    // Canvasの大きさをセット
    const canvas = window.document.createElement("canvas");
    canvas.style.width = width + "px";
    canvas.style.height = height + "px";

    // Unityスクリプト読み込み
    useEffect(() => {
        const scriptSrc = `${unityBuildRoot}/${buildName}.loader.js`;
        const root = window.document.getElementById("root");
        const scriptTag = window.document.createElement("script");
        scriptTag.type = "text/javascript";
        scriptTag.src = scriptSrc;
        if (root) {
            root.appendChild(scriptTag);
        }
        const loadingId = setTimeout(() => {
            // 8秒と見なす
            setStartBtnEnabled(true);
        }, 8000);

        // リセットボタンを有効化
        const enableResetBtn = () => {
            console.log("enable_reset_btn event from Unity");
            setStopBtnEnabled(true);
        };
        // リセットボタン有効化イベント購読
        window.addEventListener("enable_reset_btn", enableResetBtn);

        // リセットボタンを無効化
        const disableResetBtn = () => {
            console.log("disable_reset_btn event from Unity");
            setStopBtnEnabled(false);
        };
        // リセットボタン無効化イベント購読
        window.addEventListener("disable_reset_btn", disableResetBtn);

        return () => {
            clearTimeout(loadingId);
            // リセットボタン有効化イベント購読解除
            window.removeEventListener("enable_reset_btn", enableResetBtn);
            // リセットボタン無効化イベント購読解除
            window.removeEventListener("disable_reset_btn", disableResetBtn);
        };
    }, []);

    // トークン更新時の処理
    useEffect(() => {
        // シグナリングサーバと接続された
        const onConnect = () => {
            console.log("onConnect");
        };
        // シグナリングサーバから切断された
        const onDisconnect = () => {
            console.log("onDisconnect");
        };
        // シグナリングサーバから通知受信
        const onNotification = (message: NotificationMessage) => {
            console.log(`onNotification: type = ${message.type}, from id = ${message.from_id}`);
            if (unityInstanceRef.current !== undefined) {
                unityInstanceRef.current.SendMessage("GameManager", "SignalingServerOnNotification", JSON.stringify(message));
            }
        };
        // シグナリングサーバからレスポンス受信
        const onResponse = (message: ResponseMessage) => {
            console.log(`onResponse: method = ${message.method}, result = ${message.result}`);
            if (unityInstanceRef.current !== undefined) {
                unityInstanceRef.current.SendMessage("GameManager", "SignalingServerOnResponse", JSON.stringify(message));
            }
        };
        // シグナリングサーバからエラー受信
        const onError = (error: string) => {
            console.log("onError: ", error);
        };

        // ユーザートークン受信時ハンドラ
        const receivedTokenHandler = (ev: CustomEvent<SendUserTokenType>) => {
            console.log("send_user_token event from Unity: token = ", ev.detail.token);
            setUserToken(ev.detail.token);
        };

        // シグナリングサーバ接続開始時ハンドラ
        const connectHandler = () => {
            console.log("signaling_connect event from Unity");
            // socket.io接続
            socket.on("connect", onConnect);
            socket.on("disconnect", onDisconnect);
            socket.on("error", onError);
            socket.on("notification", onNotification);
            socket.on("response", onResponse);
            socket.connect();
        };

        // シグナリングサーバ接続終了時ハンドラ
        const disconnectHandler = () => {
            console.log("signaling_disconnect event from Unity");
            // socket.io切断
            socket.off("connect", onConnect);
            socket.off("disconnect", onDisconnect);
            socket.off("error", onError);
            socket.off("notification", onNotification);
            socket.off("response", onResponse);
            socket.disconnect();
        };

        // シグナリングサーバログイン時ハンドラ
        const loginHandler = () => {
            console.log("signaling_login event from Unity");
            const value = userToken + "," + "peer id" + "," + "calling" + "," + "map" + "," + "presend payload";
            console.log("loginUser value = ", value);
            socket.emit("loginUser", value);
        };

        // オペレーターにメッセージ送信時ハンドラ
        const relayToOperatorHandler = (ev: CustomEvent<RelayToOperatorType>) => {
            console.log("relay_to_operator event from Unity: message = ", ev.detail.message);
            const value = userToken + "," + "target peer id" + "," + ev.detail.message;
            console.log("relayToOperator value = ", value);
            socket.emit("relayToOperator", value);
        };

        // ユーザートークン取得イベント購読
        window.addEventListener("send_user_token", receivedTokenHandler as EventListenerOrEventListenerObject);

        // シグナリングサーバ接続開始イベント購読
        window.addEventListener("signaling_connect", connectHandler);

        // シグナリングサーバ接続終了イベント購読
        window.addEventListener("signaling_disconnect", disconnectHandler);

        // シグナリングサーバログインイベント購読
        window.addEventListener("signaling_login", loginHandler);

        // オペレーターへにメッセージ送信時イベント購読
        window.addEventListener("relay_to_operator", relayToOperatorHandler as EventListenerOrEventListenerObject);

        return () => {
            // ユーザートークン取得イベント購読解除
            window.removeEventListener("send_user_token", receivedTokenHandler as EventListenerOrEventListenerObject);
            // シグナリングサーバ接続開始イベント購読解除
            window.removeEventListener("signaling_connect", connectHandler);
            // シグナリングサーバ接続終了イベント購読解除
            window.removeEventListener("signaling_disconnect", disconnectHandler);
            // シグナリングサーバログインイベント購読解除
            window.removeEventListener("signaling_login", loginHandler);
            // オペレーターへにメッセージ送信時イベント購読解除
            window.removeEventListener("relay_to_operator", relayToOperatorHandler as EventListenerOrEventListenerObject);
        };
    }, [userToken]);

    // Unityインスタンス生成
    const { instanceRef, containerRef } = useUnity({
        canvas,
        unityBuildRoot,
        buildName,
    });
    unityInstanceRef = instanceRef;
    if (containerRef.current) {
        containerRef.current.style.width = width + "px";
        containerRef.current.style.height = height + "px";
    }

    // シナリオ開始ボタン設定
    const ClickStartBtn = () => {
        // シナリオ開始ボタン無効化
        setStartBtnEnabled(false);
        // シナリオ開始信号をUnityへ
        if (unityInstanceRef.current !== undefined) {
            unityInstanceRef.current.SendMessage("GameManager", "StartBotProcess");
        }
    };

    // リセットボタン設定
    const ClickStopBtn = () => {
        // リセットボタン無効化
        setStopBtnEnabled(false);
        // 音声認識停止
        SpeechRecognition.stopListening();
        // リセット処理停止信号をUnityへ
        if (unityInstanceRef.current !== undefined) {
            unityInstanceRef.current.SendMessage("GameManager", "StopBotProcess");
        }
        // シナリオ開始ボタン有効化
        setStartBtnEnabled(true);
    };

    // 音声認識初期化
    const { transcript, listening, browserSupportsSpeechRecognition } = useSpeechRecognition();

    // 音声認識開始イベント購読
    window.addEventListener("speechrecognition_start", function () {
        console.log("speechrecognition_start event from Unity");
        SpeechRecognition.startListening();
    });

    // 音声認識終了イベント購読
    window.addEventListener("speechrecognition_end", function () {
        console.log("speechrecognition_start event from Unity");
        SpeechRecognition.stopListening();
    });

    // 音声認識中の処理
    useEffect(() => {
        if (transcript !== "") {
            window.console.log("transcript: " + transcript);
            // 音声認識中の経過文字列をUnity側に送信
            if (unityInstanceRef.current !== undefined) {
                unityInstanceRef.current.SendMessage("GameManager", "SetSpeakingText", transcript);
            }
        }
    }, [transcript]);

    // 音声認識終了時の処理
    useEffect(() => {
        if (transcript !== "" && listening === false) {
            // 音声認識完了時の最終文字列をUnity側に送信
            if (unityInstanceRef.current !== undefined) {
                unityInstanceRef.current.SendMessage("GameManager", "SetUserMessage", transcript);
            }
        }
    }, [listening]);

    // ブラウザ対応確認
    if (!browserSupportsSpeechRecognition) {
        window.console.log("Browser doesn't support speech recognition.");
        return <span>Browser doesn't support speech recognition.</span>;
    }

    return (
        <>
            <SCanvasTitle>Unity Canvas</SCanvasTitle>
            <SCanvas ref={containerRef} />
            <SButton variant="contained" disabled={!startBtnEnabled} onClick={ClickStartBtn}>
                シナリオ開始
            </SButton>
            <SButton variant="contained" disabled={!stopBtnEnabled} onClick={ClickStopBtn}>
                リセット
            </SButton>
        </>
    );
};

const SCanvasTitle = styled.div`
    text-align: center;
    margin-top: 30px;
    margin-left: auto;
    margin-right: auto;
`;

const SCanvas = styled.div`
    align-items: center;
    align-content: center;
    margin-top: 10px;
    margin-left: auto;
    margin-right: auto;
`;

const SButton = styled(Button)`
    display: block;
    /* margin-top: 10px; */
    margin-bottom: 1rem;
    margin-left: auto;
    margin-right: auto;
`;

const SLabel = styled.div`
    text-align: center;
    margin-top: 10px;
`;
