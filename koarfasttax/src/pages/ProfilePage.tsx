import { useEffect, useRef, useState } from "react";
import {
  getProfile,
  updateProfile,
  uploadProfilePhoto,
  deleteProfilePhoto,
  type Profile,
} from "../api/profile";

function ProfilePage() {
  const [profile, setProfile] = useState<Profile | null>(null);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");

  //const [selectedPhoto, setSelectedPhoto] = useState<File | null>(null);
  const [uploadingPhoto, setUploadingPhoto] = useState(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    async function loadProfile() {
      try {
        const data = await getProfile();

        setProfile(data);
        setFirstName(data.firstName);
        setLastName(data.lastName);
        setEmail(data.email);
        setPhone(data.phone ?? "");
      } catch (error) {
        console.error(error);
        setMessage("Failed to load profile.");
      } finally {
        setLoading(false);
      }
    }

    loadProfile();
  }, []);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setSaving(true);
      setMessage("");

      const updatedProfile = await updateProfile({
        firstName,
        lastName,
        email,
        phone: phone || null,
      });

      setProfile(updatedProfile);
      setMessage("Profile updated successfully.");
    } catch (error) {
      console.error(error);
      setMessage("Failed to update profile.");
    } finally {
      setSaving(false);
    }
  }

  /* function handlePhotoUpload() {
    if (!selectedPhoto) {
      setMessage("Please choose an image first.");
      return;
    }

    try {
      setUploadingPhoto(true);
      setMessage("");

      const updatedProfile = await uploadProfilePhoto(selectedPhoto);

      setProfile(updatedProfile);
      setSelectedPhoto(null);
      setMessage("Profile photo updated successfully.");
    } catch (error) {
      console.error(error);
      setMessage("Failed to upload profile photo.");
    } finally {
      setUploadingPhoto(false);
    }
  }*/

  async function handlePhotoDelete() {
    try {
      setUploadingPhoto(true);
      setMessage("");

      await deleteProfilePhoto();

      setProfile((currentProfile) =>
        currentProfile
          ? {
              ...currentProfile,
              profileImageUrl: null,
            }
          : currentProfile,
      );

      //setSelectedPhoto(null);
      setMessage("Profile photo removed.");
    } catch (error) {
      console.error(error);
      setMessage("Failed to remove profile photo.");
    } finally {
      setUploadingPhoto(false);
    }
  }

  if (loading) {
    return <div className="p-6">Loading profile...</div>;
  }

  if (!profile) {
    return <div className="p-6">Profile could not be loaded.</div>;
  }

  return (
    <div className="mx-auto max-w-3xl p-6">
      <h1 className="text-2xl font-bold text-gray-900">Profile</h1>

      <p className="mt-2 text-gray-600">
        Manage your personal information and profile picture.
      </p>

      <div className="mt-6 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="mb-8 border-b border-gray-200 pb-6">
          <h2 className="mb-4 font-semibold text-gray-900">Profile Picture</h2>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <div className="flex h-24 w-24 items-center justify-center overflow-hidden rounded-full bg-gray-200">
              {profile.profileImageUrl ? (
                <img
                  src={`${import.meta.env.VITE_API_URL}${profile.profileImageUrl}`}
                  alt="Profile"
                  className="h-full w-full object-cover"
                />
              ) : (
                <span className="text-2xl font-semibold text-gray-500">
                  {profile.firstName?.charAt(0).toUpperCase() || "U"}
                </span>
              )}
            </div>

            <div className="flex-1">
              <input
                ref={fileInputRef}
                type="file"
                accept=".jpg,.jpeg,.png,.webp"
                className="hidden"
                onChange={async (event) => {
                  const file = event.target.files?.[0];

                  if (!file) {
                    return;
                  }

                  try {
                    setUploadingPhoto(true);
                    setMessage("");

                    const updatedProfile = await uploadProfilePhoto(file);

                    setProfile(updatedProfile);
                    setMessage("Profile photo updated successfully.");
                  } catch (error) {
                    console.error(error);
                    setMessage("Failed to upload profile photo.");
                  } finally {
                    setUploadingPhoto(false);

                    event.target.value = "";
                  }
                }}
              />

              <p className="mt-2 text-xs text-gray-500">
                JPG, PNG, or WEBP. Maximum file size: 5 MB.
              </p>

              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={uploadingPhoto}
                  className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white hover:opacity-90 disabled:opacity-50"
                >
                  {uploadingPhoto
                    ? "Uploading..."
                    : profile.profileImageUrl
                      ? "Change Photo"
                      : "Choose Profile Photo"}
                </button>

                {profile.profileImageUrl && (
                  <button
                    type="button"
                    onClick={handlePhotoDelete}
                    disabled={uploadingPhoto}
                    className="rounded-lg border border-red-300 px-4 py-2 font-medium text-red-600 hover:bg-red-50 disabled:opacity-50"
                  >
                    Remove Photo
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
        <div className="mb-6">
          <p className="text-sm text-gray-500">Account Role</p>

          <p className="font-medium text-gray-900">{profile.role}</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="mb-1 block font-medium text-gray-700">
              First Name
            </label>

            <input
              type="text"
              value={firstName}
              onChange={(event) => setFirstName(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:border-gray-500"
            />
          </div>

          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Last Name
            </label>

            <input
              type="text"
              value={lastName}
              onChange={(event) => setLastName(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:border-gray-500"
            />
          </div>

          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Email
            </label>

            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:border-gray-500"
            />
          </div>

          <div>
            <label className="mb-1 block font-medium text-gray-700">
              Phone
            </label>

            <input
              type="text"
              value={phone}
              onChange={(event) => setPhone(event.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:border-gray-500"
            />
          </div>

          {message && <p className="text-sm text-gray-600">{message}</p>}

          <button
            type="submit"
            disabled={saving}
            className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white hover:opacity-90 disabled:opacity-50"
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </form>
      </div>
    </div>
  );
}

export default ProfilePage;
