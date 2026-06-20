import type { CpuStatsModel } from "@/models/api/cpu-stats.model";
import { createApi } from "@reduxjs/toolkit/query/react";
import { customWebviewBaseQuery } from "./utils/interceptor";
import type { DiskStatsModel } from "@/models/api/disk-stats.model";
import type { RamStatsModel } from "@/models/api/ram-stats.model";

export const mainApi = createApi({
  reducerPath: "mainApi",
  baseQuery: customWebviewBaseQuery,
  endpoints: (builder) => ({
    getCpuStats: builder.query<CpuStatsModel, void>({
      query: () => ({ endpoint: "getCpuStats" }),
      transformResponse: (response: any) => {
        const transformed: CpuStatsModel = {
          coreUsages: response.coreUsagesPercent,
          currentClock: response.currentClockHz,
          temperature: response.temperatureKelvin - 273,
          totalUsage: response.totalUsagePercent,
          baseClock: response.maxClockHz,
        };
        return transformed;
      },
    }),
    getDiskStats: builder.query<DiskStatsModel, void>({
      query: () => ({ endpoint: "getDiskStats" }),
      transformResponse: (response: any) => {
        const transformed: DiskStatsModel = {
          activeTime: response.activeTimePercent,
          readSpeed: response.readBytesPerSec,
          writeSpeed: response.writeBytesPerSec,
        };
        return transformed;
      },
    }),
    getRamStats: builder.query<RamStatsModel, void>({
      query: () => ({ endpoint: "getRamStats" }),
      transformResponse: (response: any) => {
        const transformed: RamStatsModel = {
          total: response.totalBytes,
          used: response.usedBytes,
        };
        return transformed;
      },
    }),
  }),
});

export const { useGetCpuStatsQuery, useGetDiskStatsQuery, useGetRamStatsQuery } = mainApi;
