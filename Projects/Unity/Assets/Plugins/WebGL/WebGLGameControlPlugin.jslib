const GameControlPlugin = {
    EnableResetBtn: function () {
        const event = new Event("enable_reset_btn");
        window.dispatchEvent(event);
    },

    DisableResetBtn: function () {
        const event = new Event("disable_reset_btn");
        window.dispatchEvent(event);
    },

    GameControllerInitialized: function () {
        const event = new Event("gamecontroller_initialized");
        window.dispatchEvent(event);
    },
};

mergeInto(LibraryManager.library, GameControlPlugin);
