class MyWorkletProcessor extends AudioWorkletProcessor {
    constructor() {
        super();

        // 周波数パラメータを追加
        this.frequency = 440;

        this._volume = 0;
    }

    // コールバック
    // ノードに接続されている 入力 の配列です。この配列のそれぞれの要素は チャンネル の配列です。
    // それぞれの チャンネル は 128 個のサンプルが入った Float32Array です。
    // たとえば、inputs[n][m][i] は n 番目の入力の m 番目のチャンネルの i 番目のサンプルにアクセスします
    process(inputs, outputs, parameters) {
        const output = outputs[0];

        if (output.length === 0) {
            return true;
        }

        /*
        for (let channel = 0; channel < output.length; channel++) {
            const outputChannel = output[channel];

            for (let i = 0; i < outputChannel.length; i++) {
                // サイン波を生成
                outputChannel[i] = Math.sin((2 * Math.PI * this.frequency * i) / sampleRate);
            }
        }
        return true;
        */

        const samples = output[0];

        let sum = 0;
        let rms = 0;
        for (let i = 0; i < samples.length; i++) {
            sum += samples[i] * samples[i];
        }
        rms = Math.sqrt(sum / samples.length);
        this._volume = Math.max(rms, this._volume * 0.8);
        this.port.postMessage({ volume: this._volume });

        return true;
    }
}

registerProcessor("vumeter", MyWorkletProcessor);
