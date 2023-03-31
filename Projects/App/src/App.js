import { OpenCVCanvas } from "./components/OpenCVCanvas";
import { SoraCanvas } from "./components/SoraCanvas";
import { UnityCanvas } from "./components/UnityCanvas";

function App() {
    return (
        <>
            <UnityCanvas width="1280px" height="720px" />
            {/* <SoraCanvas /> */}
            <OpenCVCanvas />
        </>
    );
}

export default App;
