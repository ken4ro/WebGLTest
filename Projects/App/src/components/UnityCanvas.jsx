import { useEffect } from "react";
import styled from "styled-components";
import { useUnity } from "../hooks/useUnity";

const unityBuildRoot = "./Build";
const buildName = "docs";

let unityInstanceRef = null;

const ClickStartBtn = () => {
    console.log("ClickStartBtn");
    unityInstanceRef.current.SendMessage("GameManager", "StartBotProcess");
};

export const UnityCanvas = (props) => {
    const { width, height } = props;
    const canvas = document.createElement("canvas");
    canvas.style.width = width;
    canvas.style.height = height;

    useEffect(() => {
        const scriptSrc = `${unityBuildRoot}/${buildName}.loader.js`;
        const root = document.getElementById("root");
        const scriptTag = document.createElement("script");
        scriptTag.type = "text/javascript";
        scriptTag.src = scriptSrc;
        root.appendChild(scriptTag);
    }, []);

    const { instanceRef, containerRef } = useUnity({
        canvas,
        unityBuildRoot,
        buildName,
    });
    unityInstanceRef = instanceRef;

    return (
        <>
            <SCanvasTitle>Unity Canvas</SCanvasTitle>
            <button onClick={ClickStartBtn}>Start</button>
            <SCanvas ref={containerRef} width={width} height={height} />;
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
    margin-left: auto;
    margin-right: auto;
`;
