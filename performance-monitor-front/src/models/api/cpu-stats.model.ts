export type CpuStatsModel = {
  totalUsage: number;
  coreUsages: number[];
  currentClock: number;
  baseClock: number;
  temperature: number;
};
