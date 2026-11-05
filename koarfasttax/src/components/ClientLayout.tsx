import { useEffect, useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { Bell } from "lucide-react";

import {
  getNotifications,
  markNotificationAsRead,
  type Notification,
} from "../api/notifications";

import { getProfile, type Profile } from "../api/profile";

function ClientLayout() {
  //
  const navigate = useNavigate();

  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [notificationsOpen, setNotificationsOpen] = useState(false);

  const unreadCount = notifications.filter(
    (notification) => !notification.isRead,
  ).length;

  const [profile, setProfile] = useState<Profile | null>(null);

  function handleLogout() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("userId");
    localStorage.removeItem("role");

    navigate("/login");
  }

  useEffect(() => {
    async function loadData() {
      try {
        const [notificationData, profileData] = await Promise.all([
          getNotifications(),
          getProfile(),
        ]);

        setNotifications(notificationData);
        setProfile(profileData);
      } catch (error) {
        console.error(error);
      }
    }

    loadData();
  }, []);

  async function handleNotificationClick(notification: Notification) {
    try {
      await markNotificationAsRead(notification.id);

      setNotifications((currentNotifications) =>
        currentNotifications.map((item) =>
          item.id === notification.id
            ? {
                ...item,
                isRead: true,
              }
            : item,
        ),
      );

      if (notification.link) {
        navigate(notification.link);
        setNotificationsOpen(false);
      }
    } catch (error) {
      console.error(error);
    }
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <button
            type="button"
            onClick={() => navigate("/client/dashboard")}
            className="text-xl font-bold text-gray-900"
          >
            KFT Fast Tax
          </button>

          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">Client Portal</span>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-4">
                <div className="relative">
                  <button
                    type="button"
                    onClick={() => setNotificationsOpen((current) => !current)}
                    className="relative rounded-lg bg-transparent p-2 text-gray-600 hover:bg-gray-100"
                  >
                    <Bell size={15} />

                    {unreadCount > 0 && (
                      <span className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-600 px-1 text-xs font-bold text-white">
                        {unreadCount}
                      </span>
                    )}
                  </button>

                  {notificationsOpen && (
                    <div className="absolute right-0 z-50 mt-2 w-96 overflow-hidden rounded-xl border border-gray-200 bg-white shadow-xl">
                      <div className="border-b border-gray-200 px-4 py-3">
                        <h3 className="font-semibold text-gray-900">
                          Notifications
                        </h3>
                      </div>

                      {notifications.length === 0 ? (
                        <div className="px-4 py-6 text-center text-sm text-gray-500">
                          No notifications.
                        </div>
                      ) : (
                        <div className="max-h-96 overflow-y-auto">
                          {notifications.map((notification) => (
                            <button
                              key={notification.id}
                              type="button"
                              onClick={() =>
                                handleNotificationClick(notification)
                              }
                              className={`w-full border-b border-gray-100 px-4 py-3 text-left hover:bg-gray-50 ${
                                notification.isRead ? "bg-white" : "bg-blue-50"
                              }`}
                            >
                              <div className="flex items-start justify-between gap-3">
                                <div>
                                  <p className="text-sm font-semibold text-gray-900">
                                    {notification.title}
                                  </p>

                                  <p className="mt-1 text-sm text-gray-600">
                                    {notification.message}
                                  </p>

                                  <p className="mt-2 text-xs text-gray-400">
                                    {new Date(
                                      notification.createdAt,
                                    ).toLocaleString()}
                                  </p>
                                </div>

                                {!notification.isRead && (
                                  <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-blue-600" />
                                )}
                              </div>
                            </button>
                          ))}
                        </div>
                      )}
                    </div>
                  )}
                </div>

                <button
                  type="button"
                  onClick={() => navigate("/client/profile")}
                  className="flex items-center gap-2 rounded-lg px-2 py-1 hover:bg-gray-100"
                >
                  <div className="flex h-9 w-9 items-center justify-center overflow-hidden rounded-full bg-gray-200">
                    {profile?.profileImageUrl ? (
                      <img
                        src={`https://localhost:7058${profile.profileImageUrl}`}
                        alt="Profile"
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <span className="font-semibold text-gray-600">
                        {profile?.firstName?.charAt(0).toUpperCase() || "C"}
                      </span>
                    )}
                  </div>

                  <span className="hidden font-medium text-gray-700 sm:block">
                    {profile?.firstName || "Profile"}
                  </span>
                </button>
              </div>
              <button
                type="button"
                onClick={handleLogout}
                className="rounded-lg bg-[rgb(65,65,65)] px-4 py-2 text-sm font-medium text-white hover:bg-[rgb(50,50,50)]"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      <main>
        <Outlet />
      </main>
    </div>
  );
}

export default ClientLayout;
