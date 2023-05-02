declare class UnityInstance {
    public SetFullscreen(fullscreen: boolean): void;

    public SendMessage(gameObject: string, methodName: string, ...args: unknown[]): void;

    public Quit(): Promise<void>;
}

declare function createUnityInstance(
    canvas: HTMLCanvasElement,
    options: {
        dataUrl: string;
        frameworkUrl: string;
        codeUrl: string;
        streamingAssetsUrl: string;
        companyName: string;
        productName: string;
        productVersion: string;
        matchWebGLToCanvasSize?: boolean;
        devicePixelRatio?: number;
    },
    onProgress?: (progress: number) => void,
    onSuccess?: (unityInstance: UnityInstance) => void,
    onError?: (message: string) => void
): Promise<UnityInstance>;
