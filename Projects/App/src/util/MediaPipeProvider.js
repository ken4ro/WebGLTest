import * as faceLandmarksDetection from "@tensorflow-models/face-landmarks-detection";

let detector;
export const MediaPipeProvider = (canvas, video, callback) => {};

export const InitMediaPipe = async () => {
    const model = faceLandmarksDetection.SupportedModels.MediaPipeFaceMesh;
    const detectorConfig = {
        runtime: "mediapipe", // or 'tfjs'
        refineLandmarks: false,
        solutionPath: "https://cdn.jsdelivr.net/npm/@mediapipe/face_mesh",
    };
    detector = await faceLandmarksDetection.createDetector(model, detectorConfig);
};

export const UpdateFaceLandmarks = async (video) => {
    const faces = await detector.estimateFaces(video);
    if (faces.length === 0) return null;
    const nose = faces[0].keypoints[195];
    const leftEye = faces[0].keypoints[133];
    const rightEye = faces[0].keypoints[362];
    const jaw = faces[0].keypoints[152];
    const leftMouth = faces[0].keypoints[61];
    const rightMouth = faces[0].keypoints[291];
    const leftOutline = faces[0].keypoints[132];
    const rightOutline = faces[0].keypoints[361];

    return { nose, leftEye, rightEye, jaw, leftMouth, rightMouth, leftOutline, rightOutline };
};
