const SignalingServerPlugin = {
    SendUserToken: function (userToken) {
        var tokenStr = UTF8ToString(userToken);
        const event = new CustomEvent("send_user_token", {
            detail: { token: tokenStr },
        });
        window.dispatchEvent(event);
    },

    SignalingConnect: function () {
        console.log("SignalingServerPlugin: SignalingConnect");
        const event = new Event("signaling_connect");
        window.dispatchEvent(event);
    },

    SignalingDisconnect: function () {
        console.log("SignalingServerPlugin: SignalingDisconnect");
        const event = new Event("signaling_disconnect");
        window.dispatchEvent(event);
    },

    SignalingLogin: function () {
        console.log("SignalingServerPlugin: SignalingLogin");
        const event = new Event("signaling_login");
        window.dispatchEvent(event);
    },

    SignalingRelayToOperator: function (message) {
        console.log("SignalingServerPlugin: SignalingRelayToOperator");
        var messageStr = UTF8ToString(message);
        const event = new CustomEvent("relay_to_operator", {
            detail: { message: messageStr },
        });
        window.dispatchEvent(event);
    },
};

mergeInto(LibraryManager.library, SignalingServerPlugin);
