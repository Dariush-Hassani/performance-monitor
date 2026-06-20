import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import { webviewRequest } from "./webview-bridge";

type WebviewQueryArgs = {
  endpoint: string;
  payload?: any;
};

export const customWebviewBaseQuery: BaseQueryFn<WebviewQueryArgs, unknown, unknown> = async ({ endpoint, payload }) => {
  try {
    const result = await webviewRequest(endpoint, payload);
    return { data: result };
  } catch (error) {
    return { error: { status: "CUSTOM_ERROR", error: String(error) } };
  }
};
