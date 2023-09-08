const SpeechRecognitionPlugin = {
    StartSpeechRecognition: function () {
        const event = new Event("speechrecognition_start");
        window.dispatchEvent(event);
    },

    StopSpeechRecognition: function () {
        const event = new Event("speechrecognition_end");
        window.dispatchEvent(event);
    },
};

mergeInto(LibraryManager.library, SpeechRecognitionPlugin);
