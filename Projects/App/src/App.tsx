import React from "react";
// import { OpenCVCanvas } from "./components/OpenCVCanvas";
// import { SoraCanvas } from "./components/SoraCanvas";
import { UnityCanvas } from "./components/UnityCanvas";
import { SoraCanvas } from "./components/SoraCanvas";

function App() {
    return (
        <>
            <UnityCanvas width="1280px" height="720px" />
            <SoraCanvas />
        </>
    );
}

export default App;
