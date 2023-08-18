import React from "react";
// import { OpenCVCanvas } from "./components/OpenCVCanvas";
// import { SoraCanvas } from "./components/SoraCanvas";
import { UnityCanvas } from "./components/UnityCanvas";
import { SoraCanvas } from "./components/SoraCanvas";

function App() {
    return (
        <>
            {/* このようにサイズ指定も可能 */}
            {/* <UnityCanvas width={1280} height={720} /> */}
            <UnityCanvas />
            <SoraCanvas />
        </>
    );
}

export default App;
