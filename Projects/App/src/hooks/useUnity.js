import { useEffect, useRef, useState } from "react";

export const useUnity = (props) => {
    const { canvas, unityBuildRoot, buildName } = props;
    const [retryCount, setRetryCount] = useState(0);
    const containerRef = useRef(null);
    const instanceRef = useRef(null);

    useEffect(() => {
        if (!window.createUnityInstance) {
            const t = window.setTimeout(() => {
                setRetryCount((c) => c + 1);
            }, 100);
            return () => {
                window.clearTimeout(t);
            };
        }
        handleStart();
        return () => {
            const { current } = instanceRef;
            if (current) {
                current.Quit();
            }
        };
    }, [retryCount]);

    const handleStart = () => {
        const { current } = containerRef;
        if (!current) {
            return;
        }

        current.innnerHTML = "";
        // canvas.setAttribute("id", `unity-canvas-${buildName}`);
        canvas.setAttribute("id", `unity-canvas`);
        current.appendChild(canvas);

        window
            .createUnityInstance(canvas, {
                companyName: "DefaultCompany",
                productName: "telexistence_test_avatar",
                productVersion: "0.1",
                dataUrl: `${unityBuildRoot}/${buildName}.data.unityweb`,
                frameworkUrl: `${unityBuildRoot}/${buildName}.framework.js.unityweb`,
                codeUrl: `${unityBuildRoot}/${buildName}.wasm.unityweb`,
                streamingAssetsUrl: "StreamingAssets",
            })
            .then((instance) => {
                instanceRef.current = instance;
            })
            .catch((msg) => {
                console.error(msg);
            });
    };

    return {
        instanceRef,
        containerRef,
    };
};
