import { useGetCpuStatsQuery, useGetDiskStatsQuery, useGetRamStatsQuery } from "./store/api/main-api";

function App() {
  const { data: cpuData, isLoading: isLoadingCpu, isError: isErrorCpu } = useGetCpuStatsQuery(undefined, { pollingInterval: 2000 });
  const { data: diskData, isLoading: isLoadingDisk, isError: isErrorDisk } = useGetDiskStatsQuery(undefined, { pollingInterval: 2000 });
  const { data: ramData, isLoading: isLoadingRam, isError: isErrorRam } = useGetRamStatsQuery(undefined, { pollingInterval: 2000 });

  return (
    <div className="flex h-screen flex items-center justify-center gap-4 bg-zinc-100">
      <div className="p-4 bg-white rounded shadow">
        {isLoadingCpu && <p>Loading...</p>}
        {isErrorCpu && <p className="text-red-500">Error fetching data</p>}
        {cpuData && !isLoadingCpu && (
          <div>
            Clock: <div>{cpuData.currentClock.toFixed(2)}</div>
            Tmp: <div>{cpuData.temperature.toFixed(2)}</div>
            Usage: <div>{cpuData.totalUsage.toFixed(2)}</div>
          </div>
        )}
      </div>

      <div className="p-4 bg-white rounded shadow">
        {isLoadingDisk && <p>Loading...</p>}
        {isErrorDisk && <p className="text-red-500">Error fetching data</p>}
        {diskData && !isLoadingDisk && (
          <div>
            Active Time: <div>{diskData.activeTime.toFixed(2)}</div>
            Read Speed: <div>{diskData.readSpeed.toFixed(2)}</div>
            Write Speed: <div>{diskData.writeSpeed.toFixed(2)}</div>
          </div>
        )}
      </div>

      <div className="p-4 bg-white rounded shadow">
        {isLoadingRam && <p>Loading...</p>}
        {isErrorRam && <p className="text-red-500">Error fetching data</p>}
        {ramData && !isLoadingRam && (
          <div>
            Used: <div>{ramData.used.toFixed(2)}</div>
            Total: <div>{ramData.total.toFixed(2)}</div>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
