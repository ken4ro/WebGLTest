// need 'npm install @vladmandic/face-api'
/*
import * as faceapi from "@vladmandic/face-api";

export const LoadModels = () => {
    const MODEL_URL = "../models";
    Promise.all([
        faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
        faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
        faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL),
        faceapi.nets.faceLandmark68TinyNet.loadFromUri(MODEL_URL),
    ]).then(() => {
        console.log("load models completed");
    });
};

export const GetLandmarks = async (canvas, video) => {
    const useTinyModel = true;
    const displaySize = {
        width: 640,
        height: 480,
    };

    canvas.innerHTML = faceapi.createCanvas(video);
    faceapi.matchDimensions(canvas, displaySize);

    // Webカメラの映像から顔認識を行う
    const detections = await faceapi.detectSingleFace(video, new faceapi.TinyFaceDetectorOptions({ inputSize: 160 })).withFaceLandmarks(useTinyModel);
    if (!detections) {
        return null;
    }

    // 認識データをリサイズ
    const resizedDetections = faceapi.resizeResults(detections, displaySize);

    // ランドマークをキャンバスに描画
    canvas.getContext("2d").clearRect(0, 0, displaySize.width, displaySize.height);
    faceapi.draw.drawDetections(canvas, resizedDetections);
    faceapi.draw.drawFaceLandmarks(canvas, resizedDetections);

    // 以後使用するランドマーク座標
    const landmarks = resizedDetections.landmarks;
    const nose = landmarks.getNose()[3];
    const leftEye = landmarks.getLeftEye()[0];
    const rightEye = landmarks.getRightEye()[3];
    const jaw = landmarks.getJawOutline()[8];
    const leftMouth = landmarks.getMouth()[0];
    const rightMouth = landmarks.getMouth()[6];
    const leftOutline = landmarks.getJawOutline()[0];
    const rightOutline = landmarks.getJawOutline()[16];

    return { nose, leftEye, rightEye, jaw, leftMouth, rightMouth, leftOutline, rightOutline };
};
*/
