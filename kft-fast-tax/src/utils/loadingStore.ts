type LoadingListener = (loading: boolean) => void;

let activeRequests = 0;
const listeners = new Set<LoadingListener>();

function notify() {
  const loading = activeRequests > 0;

  listeners.forEach((listener) => {
    listener(loading);
  });
}

export function startLoading() {
  activeRequests++;
  notify();
}

export function stopLoading() {
  activeRequests = Math.max(0, activeRequests - 1);
  notify();
}

export function subscribeToLoading(listener: LoadingListener) {
  listeners.add(listener);

  listener(activeRequests > 0);

  return () => {
    listeners.delete(listener);
  };
}
