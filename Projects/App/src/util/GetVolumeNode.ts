export const GetVolumeNodeFromStream = async (mediaStream: MediaStream) => {
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

export const GetVolumeNodeFromSpeaker = async () => {
    const audioContext = new AudioContext();
    audioContext.resume(); // 明示的にリジュームする
    // 開発時とデプロイ時でパスが違う。しかもhomepageタグと連動していない。要検討...
    try {
        await audioContext.audioWorklet.addModule("../vumeter.js");
    } catch (e) {
        await audioContext.audioWorklet.addModule("../WebGLTest/vumeter.js");
    }
    const a = new OscillatorNode(audioContext);
    const node = new AudioWorkletNode(audioContext, "vumeter");
    // a.connect(node).connect(audioContext.destination);
    a.connect(audioContext.destination);
    // node.connect(audioContext.destination);
    a.start(0);
    return node;
};
