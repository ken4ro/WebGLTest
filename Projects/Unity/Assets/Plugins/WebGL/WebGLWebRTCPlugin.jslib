const WebRTCPlugin =
{
    WebRTCConnect: function()
    {
        console.log("WebRTCPlugin: Connect");
        const event = new Event("webrtc_connect");
        window.dispatchEvent(event);
    },

    WebRTCDisconnect: function()
    {
        console.log("WebRTCPlugin: Disconnect");
        const event = new Event("webrtc_disconnect");
        window.dispatchEvent(event);
    }
};

mergeInto(LibraryManager.library, WebRTCPlugin);