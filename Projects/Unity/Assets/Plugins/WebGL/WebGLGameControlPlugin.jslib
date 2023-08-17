const GameControlPlugin = 
{
    EnableResetBtn: function () {
        console.log("GameControlPlugin: EnableResetBtn");
        const event = new Event("enable_reset_btn");
        window.dispatchEvent(event);
    },

    DisableResetBtn: function () {
        console.log("GameControlPlugin: DisableResetBtn");
        const event = new Event("disable_reset_btn");
        window.dispatchEvent(event);
    }
};

mergeInto(LibraryManager.library, GameControlPlugin);