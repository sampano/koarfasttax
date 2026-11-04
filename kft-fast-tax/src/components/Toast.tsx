type ToastProps = {
  message: string;
  type?: "success" | "error";
  onClose: () => void;
};

function Toast({ message, type = "success", onClose }: ToastProps) {
  if (!message) {
    return null;
  }

  const isError = type === "error";

  return (
    <div
      className={`fixed right-5 top-5 z-[120] max-w-sm rounded-xl border px-4 py-3 shadow-lg ${
        isError ? "border-red-200 bg-red-50" : "border-green-200 bg-green-50"
      }`}
    >
      <div className="flex items-start gap-3">
        <p
          className={`flex-1 text-sm font-medium ${
            isError ? "text-red-700" : "text-green-700"
          }`}
        >
          {message}
        </p>

        <button
          type="button"
          onClick={onClose}
          className={
            isError
              ? "text-red-400 hover:text-red-600"
              : "text-green-400 hover:text-green-600"
          }
        >
          ✕
        </button>
      </div>
    </div>
  );
}

export default Toast;
