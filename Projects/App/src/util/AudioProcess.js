export const AudioProcess = async (mediaStream) => {
    const audioContext = new AudioContext();
    await audioContext.audioWorklet.addModule("../vumeter.js");
    const stream = audioContext.createMediaStreamSource(mediaStream);
    const node = new AudioWorkletNode(audioContext, "vumeter");
    stream.connect(node).connect(audioContext.destination);

    return node;
};
