class MyWorkletProcessor extends AudioWorkletProcessor {
    _volume;

    constructor() {
        super();
        this._volume = 0;
    }

    // パラメータを自由に設定できる
    static get parameterDescriptors() {
        return [
            {
                name: "dummy",
                defaultValue: 0.5,
                minValue: 0,
                maxValue: 1,
                automationRate: "k-rate",
            },
        ];
    }

    // コールバック
    process(inputs, outputs, parameters) {
        // outputsを、inputsとparameters.dummyでいじる

        const input = inputs[0];
        if (input.length === 0) return false;
        const samples = input[0];

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
