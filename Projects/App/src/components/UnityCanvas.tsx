import { useEffect, useState } from "react";
// import styled from "styled-components";
import styled from "@emotion/styled";
import SpeechRecognition, { useSpeechRecognition } from "react-speech-recognition";
import { Button } from "@mui/material";
import { useUnity } from "../hooks/useUnity";

const unityBuildRoot = "./Build";
const buildName = "docs";

export let unityInstanceRef: React.MutableRefObject<UnityInstance | undefined>;

type Props = {
    width: string;
    height: string;
};

export const UnityCanvas = ({ width, height }: Props) => {
    // Canvasの大きさをセット
    const canvas = window.document.createElement("canvas");
    canvas.style.width = width;
    canvas.style.height = height;

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
    }, []);

    // Unityインスタンス生成
    const { instanceRef, containerRef } = useUnity({
        canvas,
        unityBuildRoot,
        buildName,
    });
    unityInstanceRef = instanceRef;
    if (containerRef.current) {
        containerRef.current.style.width = width;
        containerRef.current.style.height = height;
    }

    // ボットスタートボタン設定
    const [startBtnEnabled, setStartBtnEnabled] = useState(true);
    const ClickStartBtn = () => {
        setStartBtnEnabled(false);
        if (unityInstanceRef.current !== undefined) {
            unityInstanceRef.current.SendMessage("GameManager", "StartBotProcess");
        }
    };

    // 音声認識初期化
    const { transcript, listening, browserSupportsSpeechRecognition } = useSpeechRecognition();

    // 音声認識ボタン設定
    const [recognitionBtnEnabled, setRecognitionBtnEnabled] = useState(true);
    const ClickRecognitionBtn = () => {
        setRecognitionBtnEnabled(false);
        SpeechRecognition.startListening();
    };

    // 音声認識開始イベント購読
    window.addEventListener("recognition", function () {
        setRecognitionBtnEnabled(false);
        SpeechRecognition.startListening();
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
            <SCanvas ref={containerRef} />;
            <SButton variant="contained" disabled={!startBtnEnabled} onClick={ClickStartBtn}>
                シナリオ開始
            </SButton>
            {/* <SButton variant="contained" disabled={!recognitionBtnEnabled} onClick={ClickRecognitionBtn}>
                音声認識開始
            </SButton>
            <SLabel>Microphone: {listening ? "on" : "off"}</SLabel>
            <SLabel>transcript: {transcript}</SLabel> */}
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
    margin-top: 10px;
    /* margin-top: 10px; */
    margin-left: auto;
    margin-right: auto;
`;

const SLabel = styled.div`
    text-align: center;
    margin-top: 10px;
`;
