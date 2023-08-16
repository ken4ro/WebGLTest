/* eslint-disable @typescript-eslint/no-var-requires */
// import { io } from "socket.io-client";

// "undefined" means the URL will be computed from the `window.location` object
const URL = "https://dev.xrccg.com:4001";

const io = require("socket.io-client");
export const socket = io(URL, {
    reconnection: false,
    autoConnect: false,
});
