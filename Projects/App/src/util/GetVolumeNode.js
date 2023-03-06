let count = 0;
export const GetVolumeNode = async (mediaStream) => {
    console.log(`GetVolumeNode count = ${count++}`);
    const audioContext = new AudioContext();
    try {
        await audioContext.audioWorklet.addModule("../vumeter.js");
    } catch (e) {
        console.log(`GetVolumeNode addModule exception1`);
    }
    try {
        await audioContext.audioWorklet.addModule("../WebGLTest/vumeter.js");
    } catch (e) {
        console.log(`GetVolumeNode addModule exception2`);
    }

    const stream = audioContext.createMediaStreamSource(mediaStream);
    const node = new AudioWorkletNode(audioContext, "vumeter");
    stream.connect(node).connect(audioContext.destination);

    return node;
};
