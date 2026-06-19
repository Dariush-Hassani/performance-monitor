import { useGetSystemInfoQuery } from "./store/api";
import { Button } from "@/components/ui/button";

function App() {
  const { data, isLoading, isError } = useGetSystemInfoQuery();

  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4 bg-zinc-100">
      <Button>Shadcn Button</Button>

      <div className="p-4 bg-white rounded shadow">
        <h2 className="font-bold mb-2">Data from C#:</h2>
        {isLoading && <p>Loading...</p>}
        {isError && <p className="text-red-500">Error fetching data</p>}
        {data && <pre>{JSON.stringify(data, null, 2)}</pre>}
      </div>
    </div>
  );
}

export default App;
