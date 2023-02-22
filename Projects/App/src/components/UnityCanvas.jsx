import { useEffect, useState } from "react";
// import styled from "styled-components";
import { useUnity } from "../hooks/useUnity";
import { Button } from "@mui/material";
import styled from "@emotion/styled";

const unityBuildRoot = "./Build";
const buildName = "docs";

let unityInstanceRef = null;

export const UnityCanvas = (props) => {
    // Canvasの大きさをセット
    const { width, height } = props;
    const canvas = document.createElement("canvas");
    canvas.style.width = width;
    canvas.style.height = height;

    // Unityスクリプト読み込み
    useEffect(() => {
        const scriptSrc = `${unityBuildRoot}/${buildName}.loader.js`;
        const root = document.getElementById("root");
        const scriptTag = document.createElement("script");
        scriptTag.type = "text/javascript";
        scriptTag.src = scriptSrc;
        root.appendChild(scriptTag);
    }, []);

    // Unityインスタンス生成
    const { instanceRef, containerRef } = useUnity({
        canvas,
        unityBuildRoot,
        buildName,
    });
    unityInstanceRef = instanceRef;

    // ボタン設定
    const [startBtnEnabled, setStartBtnEnabled] = useState(true);
    const ClickStartBtn = () => {
        console.log("ClickStartBtn");
        setStartBtnEnabled(false);
        unityInstanceRef.current.SendMessage("GameManager", "StartBotProcess");
    };
    
    return (
        <>
            <SCanvasTitle>Unity Canvas</SCanvasTitle>
            <SCanvas ref={containerRef} width={width} height={height} />;
            <SButton variant="contained" disabled={!startBtnEnabled} onClick={ClickStartBtn}>Start</SButton>
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
    width: ${(props) => props.width};
    height: ${(props) => props.height};
    margin-top: 10px;
    margin-left: auto;
    margin-right: auto;
`;

const SButton = styled(Button)`
    display: block;
    /* margin-top: 10px; */
    margin-left: auto;
    margin-right: auto;
`