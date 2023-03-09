export const GetVolumeNode = async (mediaStream) => {
    const audioContext = new AudioContext();
    // 開発時とデプロイ時でパスが違う。しかもhomepageタグと連動していない。要検討...
    try {
        await audioContext.audioWorklet.addModule("../vumeter.js");
    } catch (e) {
        await audioContext.audioWorklet.addModule("../WebGLTest/vumeter.js");
    }
    const stream = audioContext.createMediaStreamSource(mediaStream);
    const node = new AudioWorkletNode(audioContext, "vumeter");
    stream.connect(node).connect(audioContext.destination);
    return node;
};
