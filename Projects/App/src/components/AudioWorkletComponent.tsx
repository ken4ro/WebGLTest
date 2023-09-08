import React, { useState, useEffect } from "react";

interface Props {
    canvas: HTMLCanvasElement;
}

const AudioWorkletComponent = ({ canvas }: Props) => {
    const [audioContext, setAudioContext] = useState<AudioContext | null>(null);
    const [workletNode, setWorkletNode] = useState<AudioWorkletNode | null>(null);

    useEffect(() => {
        const setupAudio = async () => {
            try {
                // AudioContextを作成
                const context = new AudioContext();
                setAudioContext(context);

                // AudioWorkletを登録
                try {
                    await context.audioWorklet.addModule("./vumeter.js");
                } catch (error) {
                    await context.audioWorklet.addModule("./WebGLTest/vumeter.js");
                }

                // AudioWorkletNodeを作成
                const node = new AudioWorkletNode(context, "vumeter");
                setWorkletNode(node);
                node.port.onmessage = (ev) => {
                    const volume = ev.data.volume;
                    console.log(`speaker volume = ${volume}`);
                };
                node.connect(context.destination);
            } catch (error) {
                console.error("Error setting up AudioWorklet:", error);
            }
        };

        setupAudio();

        return () => {
            // コンポーネントがアンマウントされる際にクリーンアップ
            if (workletNode) {
                workletNode.disconnect();
            }
            if (audioContext) {
                audioContext.close().then(() => {
                    setAudioContext(null);
                });
            }
        };
    }, [canvas]);

    const startAudio = () => {
        audioContext?.resume();
    };

    const stopAudio = () => {
        audioContext?.suspend();
    };

    return (
        <div>
            <button onClick={startAudio}>Start</button>
            <button onClick={stopAudio}>Stop</button>
        </div>
    );
};

export default AudioWorkletComponent;
