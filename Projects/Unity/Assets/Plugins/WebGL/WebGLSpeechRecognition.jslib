const SpeechRecognitionPlugin =
{
    StartSpeechRecognition: function()
    {
        console.log("SpeechRecognitionPlugin: StartRecognition");
        const event = new Event("speechrecognition_start");
        window.dispatchEvent(event);
    },

    StopSpeechRecognition: function()
    {
        console.log("SpeechRecognitionPlugin: StopRecognition");
        const event = new Event("speechrecognition_end")
        window.dispatchEvent(event);
    }
};

mergeInto(LibraryManager.library, SpeechRecognitionPlugin);