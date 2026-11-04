import { useEffect, useState } from "react";

import { subscribeToLoading } from "../utils/loadingStore";

function GlobalLoader() {
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    return subscribeToLoading(setLoading);
  }, []);

  if (!loading) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/20">
      <div className="rounded-xl bg-white px-6 py-5 shadow-xl">
        <div className="flex items-center gap-3">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-gray-300 border-t-blue-600" />

          <span className="text-sm font-medium text-gray-700">Loading...</span>
        </div>
      </div>
    </div>
  );
}

export default GlobalLoader;
