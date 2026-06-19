declare global {
  interface Window {
    chrome?: {
      webview?: {
        postMessage: (message: unknown) => void;
        addEventListener: (type: string, listener: EventListener) => void;
        removeEventListener: (type: string, listener: EventListener) => void;
      };
    };
  }
}

interface WebviewResponse {
  id?: string;
  data?: unknown;
  error?: string;
}

export const webviewRequest = (endpoint: string, payload?: unknown): Promise<unknown> => {
  return new Promise((resolve, reject) => {
    const webview = window.chrome?.webview;

    if (!webview) {
      console.warn("WebView2 is not available. Are you running in a normal browser?");
      reject("WebView2 not found");
      return;
    }

    const id = crypto.randomUUID();
    const requestMessage = { id, endpoint, payload };

    const handleMessage = (event: Event) => {
      const messageEvent = event as MessageEvent;
      const response = messageEvent.data as WebviewResponse;

      if (response && response.id === id) {
        webview.removeEventListener("message", handleMessage);

        if (response.error) {
          reject(response.error);
        } else {
          resolve(response.data);
        }
      }
    };

    webview.addEventListener("message", handleMessage);
    webview.postMessage(requestMessage);
  });
};
