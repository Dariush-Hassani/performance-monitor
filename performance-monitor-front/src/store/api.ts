import { webviewRequest } from "@/lib/webview-bridge";
import { createApi, type BaseQueryFn } from "@reduxjs/toolkit/query/react";

type WebviewQueryArgs = {
  endpoint: string;
  payload?: any;
};

const customWebviewBaseQuery: BaseQueryFn<WebviewQueryArgs, unknown, unknown> = async ({ endpoint, payload }) => {
  try {
    const result = await webviewRequest(endpoint, payload);
    return { data: result };
  } catch (error) {
    return { error: { status: "CUSTOM_ERROR", error: String(error) } };
  }
};

export const mainApi = createApi({
  reducerPath: "mainApi",
  baseQuery: customWebviewBaseQuery,
  endpoints: (builder) => ({
    getSystemInfo: builder.query<any, void>({
      query: () => ({ endpoint: "getSystemInfo" }),
    }),
    sendDataToHost: builder.mutation<any, { text: string }>({
      query: (data) => ({ endpoint: "saveData", payload: data }),
    }),
  }),
});

export const { useGetSystemInfoQuery, useSendDataToHostMutation } = mainApi;
