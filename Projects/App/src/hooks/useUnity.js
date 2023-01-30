import { useEffect, useRef, useState } from "react";

export const useUnity = (props) => {
    const { canvas, unityBuildRoot, buildName } = props;
    const [retryCount, setRetryCount] = useState(0);
    const containerRef = useRef(null);
    const instanceRef = useRef(null);

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
                dataUrl: `${unityBuildRoot}/${buildName}.data`,
                frameworkUrl: `${unityBuildRoot}/${buildName}.framework.js`,
                codeUrl: `${unityBuildRoot}/${buildName}.wasm`,
                streamingAssetsUrl: "StreamingAssets",
            })
            .then((instance) => {
                instanceRef.current = instance;
            })
            .catch((msg) => {
                console.error(msg);
            });
    };

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

    const scriptSrc = `${unityBuildRoot}/${buildName}.loader.js`;

    return {
        instanceRef,
        containerRef,
        scriptSrc,
    };
};
