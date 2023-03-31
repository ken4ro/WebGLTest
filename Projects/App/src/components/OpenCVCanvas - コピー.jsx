import styled from "@emotion/styled";
import { Button } from "@mui/material";
import * as faceapi from "@vladmandic/face-api";
import { useCallback, useEffect, useRef, useState } from "react";

let isLoaded = false;
let cv = null;
export const OpenCVCanvas = (props) => {
    const videoRef = useRef();
    const canvasRef = useRef();
    const requestAnimationRef = useRef();

    const a = useCallback(async () => {
        requestAnimationRef.current = requestAnimationFrame(a);
        console.log("requestAnimationFrame");
        if (canvasRef && canvasRef.current) {
            canvasRef.current.innerHTML = faceapi.createCanvas(videoRef.current);
            const displaySize = {
                width: 640,
                height: 480,
            };

            faceapi.matchDimensions(canvasRef.current, displaySize);

            // Webカメラの映像から顔認識を行う
            const detections = await faceapi.detectSingleFace(videoRef.current, new faceapi.TinyFaceDetectorOptions()).withFaceLandmarks();

            if (!detections) {
                return;
            }

            // 認識データをリサイズ
            const resizedDetections = faceapi.resizeResults(detections, displaySize);

            // ランドマークをキャンバスに描画
            canvasRef && canvasRef.current && canvasRef.current.getContext("2d").clearRect(0, 0, 640, 480);
            canvasRef && canvasRef.current && faceapi.draw.drawDetections(canvasRef.current, resizedDetections);
            canvasRef && canvasRef.current && faceapi.draw.drawFaceLandmarks(canvasRef.current, resizedDetections);

            if (resizedDetections !== undefined) {
                // 以後使用するランドマーク座標
                const landmarks = resizedDetections.landmarks;
                const nose = landmarks.getNose();
                const leftEye = landmarks.getLeftEye();
                const rightEye = landmarks.getRightEye();
                const jaw = landmarks.getJawOutline();
                const leftMouth = landmarks.getMouth();
                const rightMouth = landmarks.getMouth();
                const leftOutline = landmarks.getJawOutline();
                const rightOutline = landmarks.getJawOutline();

                // capture model points
                const detectPoints = [
                    // nose
                    ...[0.0, 0.0, 0.0],
                    // jaw
                    ...[0, -330, -65],
                    // left eye
                    ...[-240, 170, -135],
                    // right eye
                    ...[240, 170, -135],
                    // left mouth
                    ...[-150, -150, -125],
                    // right mouth
                    ...[150, -150, -125],
                    // left outline
                    ...[-480, 170, -340],
                    // right outline
                    ...[480, 170, -340],
                ];

                let { success, imagePoints, cameraMatrix, distCoeffs, rvec, tvec } = solve({
                    nose,
                    leftEye,
                    rightEye,
                    jaw,
                    leftMouth,
                    rightMouth,
                    leftOutline,
                    rightOutline,
                });
                function solve({ nose, leftEye, rightEye, jaw, leftMouth, rightMouth, leftOutline, rightOutline }) {
                    const rows = detectPoints.length / 3;
                    const modelPoints = cv.matFromArray(rows, 3, cv.CV_64FC1, detectPoints);

                    // camera matrix
                    const size = {
                        width: 640,
                        height: 480,
                    };
                    const center = [size.width / 2, size.height / 2];
                    const cameraMatrix = cv.matFromArray(3, 3, cv.CV_64FC1, [
                        ...[size.width, 0, center[0]],
                        ...[0, size.width, center[1]],
                        ...[0, 0, 1],
                    ]);

                    // image matrix
                    const imagePoints = cv.Mat.zeros(rows, 2, cv.CV_64FC1);
                    const distCoeffs = cv.Mat.zeros(4, 1, cv.CV_64FC1);
                    const rvec = new cv.Mat({ width: 1, height: 3 }, cv.CV_64FC1);
                    const tvec = new cv.Mat({ width: 1, height: 3 }, cv.CV_64FC1);

                    [...nose, ...jaw, ...leftEye, ...rightEye, ...leftMouth, ...rightMouth, ...leftOutline, ...rightOutline].map((v, i) => {
                        imagePoints.data64F[i] = v;
                    });

                    // 移動ベクトルと回転ベクトルの初期値を与えることで推測速度の向上をはかる
                    tvec.data64F[0] = -100;
                    tvec.data64F[1] = 100;
                    tvec.data64F[2] = 1000;
                    const distToLeftEyeX = Math.abs(leftEye[0] - nose[0]);
                    const distToRightEyeX = Math.abs(rightEye[0] - nose[0]);
                    if (distToLeftEyeX < distToRightEyeX) {
                        // 左向き
                        rvec.data64F[0] = -1.0;
                        rvec.data64F[1] = -0.75;
                        rvec.data64F[2] = -3.0;
                    } else {
                        // 右向き
                        rvec.data64F[0] = 1.0;
                        rvec.data64F[1] = -0.75;
                        rvec.data64F[2] = -3.0;
                    }

                    const success = cv.solvePnP(modelPoints, imagePoints, cameraMatrix, distCoeffs, rvec, tvec, true);

                    return {
                        success,
                        imagePoints,
                        cameraMatrix,
                        distCoeffs,
                        rvec, // 回転ベクトル
                        tvec, // 移動ベクトル
                    };
                }

                if (success) {
                    let { yaw, pitch, roll } = headpose({ rvec, tvec, cameraMatrix, distCoeffs, imagePoints });
                    console.log(`yaw = ${yaw}, pitch = ${pitch}, roll = ${roll}`);
                }
                function headpose({ rvec, tvec, cameraMatrix, distCoeffs, imagePoints }) {
                    const noseEndPoint2DZ = new cv.Mat();
                    const noseEndPoint2DY = new cv.Mat();
                    const noseEndPoint2DX = new cv.Mat();

                    const pointZ = cv.matFromArray(1, 3, cv.CV_64FC1, [0.0, 0.0, 500.0]);
                    const pointY = cv.matFromArray(1, 3, cv.CV_64FC1, [0.0, 500.0, 0.0]);
                    const pointX = cv.matFromArray(1, 3, cv.CV_64FC1, [500.0, 0.0, 0.0]);
                    const jaco = new cv.Mat();

                    cv.projectPoints(pointZ, rvec, tvec, cameraMatrix, distCoeffs, noseEndPoint2DZ, jaco);
                    cv.projectPoints(pointY, rvec, tvec, cameraMatrix, distCoeffs, noseEndPoint2DY, jaco);
                    cv.projectPoints(pointX, rvec, tvec, cameraMatrix, distCoeffs, noseEndPoint2DX, jaco);

                    const rmat = new cv.Mat();
                    cv.Rodrigues(rvec, rmat);

                    const projectMat = cv.Mat.zeros(3, 4, cv.CV_64FC1);
                    projectMat.data64F[0] = rmat.data64F[0];
                    projectMat.data64F[1] = rmat.data64F[1];
                    projectMat.data64F[2] = rmat.data64F[2];
                    projectMat.data64F[4] = rmat.data64F[3];
                    projectMat.data64F[5] = rmat.data64F[4];
                    projectMat.data64F[6] = rmat.data64F[5];
                    projectMat.data64F[8] = rmat.data64F[6];
                    projectMat.data64F[9] = rmat.data64F[7];
                    projectMat.data64F[10] = rmat.data64F[8];

                    const cmat = new cv.Mat();
                    const rotmat = new cv.Mat();
                    const travec = new cv.Mat();
                    const rotmatX = new cv.Mat();
                    const rotmatY = new cv.Mat();
                    const rotmatZ = new cv.Mat();
                    const eulerAngles = new cv.Mat();

                    cv.decomposeProjectionMatrix(
                        projectMat,
                        cmat,
                        rotmat,
                        travec,
                        rotmatX,
                        rotmatY,
                        rotmatZ,
                        eulerAngles // 顔の角度情報
                    );

                    return {
                        yaw: eulerAngles.data64F[1],
                        pitch: eulerAngles.data64F[0],
                        roll: eulerAngles.data64F[2],
                    };
                }
            }
        }
    }, []);

    useEffect(() => {
        console.log("useEffect");
        if (!isLoaded) {
            // モデル読み込み
            const loadModels = async () => {
                const MODEL_URL = "../models";
                Promise.all([
                    faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
                    faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
                    faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL),
                    faceapi.nets.faceExpressionNet.loadFromUri(MODEL_URL),
                ]).then(() => {
                    console.log("load models completed");
                });
            };
            loadModels();
            // OpenCV読み込み
            const loadOpenCV = async () => {
                cv = await window.cv;
            };
            loadOpenCV();
            // const head = document.getElementsByTagName("head")[0];
            // const scriptUrl = document.createElement("script");
            // scriptUrl.type = "text/javascript";
            // scriptUrl.src = "./opencv.js";
            // head.appendChild(scriptUrl);
            isLoaded = true;
            console.log("load");

            a();
        }
        return () => {
            console.log("useEffect return");
            // cancelAnimationFrame(requestAnimationFrame.current);
        };
    }, []);

    // const handleVideoOnPlay = () => {
    //     setInterval(async () => {
    //         await a();
    //     }, 100);
    // };

    // Startボタン処理
    const startVideo = async () => {
        // mediastream接続
        const stream = await navigator.mediaDevices.getUserMedia({ audio: false, video: true });
        let video = videoRef.current;
        video.srcObject = stream;
        await video.play();
    };

    const stopVideo = () => {
        videoRef.current.pause();
        videoRef.current.srcObject.getTracks()[0].stop();
    };

    return (
        <>
            {true && (
                <SContainer>
                    <Button onClick={startVideo}>開始</Button>
                    <Button onClick={stopVideo}>終了</Button>
                    <SVideo ref={videoRef} width="640" height="480" />
                    <SCanvas ref={canvasRef} width="640" height="480" />
                </SContainer>
            )}
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
