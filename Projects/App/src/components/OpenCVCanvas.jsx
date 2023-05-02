import { useEffect, useRef } from "react";
import styled from "@emotion/styled";
import { Button } from "@mui/material";
import { unityInstanceRef } from "./UnityCanvas";
import { GetYawPitchRoll } from "../util/OpenCVProvider";
import { InitMediaPipe, UpdateFaceLandmarks } from "../util/MediaPipeProvider";

let isInit = false;
const sleep = (waitTime) => new Promise((resolve) => window.setTimeout(resolve, waitTime));
export const OpenCVCanvas = () => {
    const videoRef = useRef();
    const canvasRef = useRef();
    const requestAnimationRef = useRef();

    const FaceRecognition = async () => {
        requestAnimationRef.current = window.requestAnimationFrame(FaceRecognition);
        // 特徴点検出
        const face = await UpdateFaceLandmarks(videoRef.current);
        if (face === null) return;
        const { nose, leftEye, rightEye, jaw, leftMouth, rightMouth, leftOutline, rightOutline } = face;
        let { yaw, pitch, roll } = await GetYawPitchRoll(canvasRef.current, {
            nose,
            leftEye,
            rightEye,
            jaw,
            leftMouth,
            rightMouth,
            leftOutline,
            rightOutline,
        });
        if (pitch > 0) {
            pitch = pitch - 180;
        } else {
            pitch = pitch + 180;
        }
        const faceInfo = {
            yaw: -yaw * 0.7,
            pitch: pitch * 0.7,
            roll: roll * 0.7,
            bodyYaw: -yaw * 0.3,
            bodyPitch: pitch * 0.3,
            bodyRoll: roll * 0.3,
        };
        window.console.log(`yaw = ${yaw}, pitch = ${pitch}, roll = ${roll}`);
        unityInstanceRef.current.SendMessage("GameManager", "SetFaceInfo", JSON.stringify(faceInfo));
    };

    const handleVideoOnPlay = async () => {
        await sleep(1000);
        FaceRecognition();
    };

    useEffect(() => {
        if (!isInit) {
            Init();
            isInit = true;
        }
        return () => {
            window.cancelAnimationFrame(requestAnimationRef.current);
        };
    }, []);

    const Init = async () => {
        // MediaPipe初期化
        await InitMediaPipe();
        window.console.log("InitMediaPipe Complete");
    };

    // WebCamera開始
    const startVideo = async () => {
        // mediastream接続
        const stream = await window.navigator.mediaDevices.getUserMedia({ audio: false, video: true });
        let video = videoRef.current;
        video.srcObject = stream;
        await video.play();
    };

    // WebCamera終了
    const stopVideo = async () => {
        window.cancelAnimationFrame(requestAnimationRef.current);
        videoRef.current.pause();
        videoRef.current.srcObject.getTracks()[0].stop();
        videoRef.current.srcObject = null;
        canvasRef.current.getContext("2d").clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
    };

    return (
        <>
            <SContainer>
                <SVideo ref={videoRef} width="640" height="480" onPlay={handleVideoOnPlay} />
                <SCanvas ref={canvasRef} width="640" height="480" />
            </SContainer>
            <Button onClick={startVideo}>開始</Button>
            <Button onClick={stopVideo}>終了</Button>
        </>
    );
};

const SContainer = styled.div`
    position: relative;
`;

const SVideo = styled.video`
    transform: scaleX(-1);
`;

const SCanvas = styled.canvas`
    position: absolute;
    top: 0px;
    left: 0px;
    transform: scaleX(-1);
`;
