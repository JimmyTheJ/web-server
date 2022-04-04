export const Roles = {
  Name: {
    Admin: 'Administrator',
    Elevated: 'Elevated',
    General: 'User',
    Default: '',
    None: 'None',
  },
  Level: {
    Admin: 3,
    Elevated: 2,
    General: 1,
    Default: 0,
    None: -2,
  },
}

export const TokenValidation = {
  Valid: 0,
  MissingRefreshToken: 1,
  InvalidRefreshToken: 2,
  RequiresNewJwt: 3,
}

export const MediaTypes = {
  Video: 1,
  Image: 2,
  Text: 3,
}

export const Admin = {
  RefreshTokenTimer: 60 * 1000,
}

export const Delay = {
  '1': 10,
  '2': 25,
  '3': 50,
  '4': 100,
  '5': 250,
  '6': 500,
  '7': 750,
  '8': 1000,
  '9': 1500,
  '10': 2000,
  '11': 2500,
  '12': 3000,
}

export const DirectoryAccessFlags = {
  ReadFile: 1,
  ReadFolder: 2,
  UploadFile: 4,
  DeleteFile: 8,
  CreateFolder: 16,
  DeleteFolder: 32,
  MoveFolder: 64,
  MoveFile: 128,
}

export const ApiEndpoints = {
  Signout: 'api/account/logout',
  RefreshToken: 'api/account/refresh-jwt',
  Login: 'api/account/login',
  Register: 'api/account/register',
  Logout: 'api/account/logout',
}

export const RouterEndpoints = {
  Signout: '/account/logout',
  RefreshToken: '/account/refresh-jwt',
  Login: '/account/login',
  Register: '/account/register',
  Logout: '/account/logout',
}

export const NotificationActions = {
  Success: 0,
  Info: 1,
  Failed: 2,
}

export const NotificationTypes = {
  General: 0,
  Chat: 1,
  Browser: 3,
  Upload: 2,
  LibraryBook: 4,
}

export const Modules = {
  Chat: 'chat',
  Directory: 'directory',
  Library: 'library',
  Notes: 'notes',
  Weight: 'weight',
}
