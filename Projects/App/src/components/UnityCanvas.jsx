import { useEffect } from "react";
import styled from "styled-components";
import { useUnity } from "../hooks/useUnity";

const unityBuildRoot = "./Build";
const buildName = "docs";

export const UnityCanvas = (props) => {
    const { width, height } = props;
    const canvas = document.createElement("canvas");
    canvas.style.width = width;
    canvas.style.height = height;
    const { instanceRef, containerRef, scriptSrc } = useUnity({
        canvas,
        unityBuildRoot,
        buildName,
    });

    useEffect(() => {
        const root = document.getElementById("root");
        const scriptTag = document.createElement("script");
        scriptTag.type = "text/javascript";
        scriptTag.src = scriptSrc;
        root.appendChild(scriptTag);
    }, []);

    return (
        <>
            <SCanvasTitle>Unity Canvas</SCanvasTitle>
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
