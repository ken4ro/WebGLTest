const SpeechRecognitionPlugin =
{
    StartRecognition: function()
    {
        console.log("StartRecognition");
        const event = new Event("recognition");
        window.dispatchEvent(event);
    }
};

mergeInto(LibraryManager.library, SpeechRecognitionPlugin);