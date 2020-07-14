/*
 * Authentication
 */

export const LOGIN_SUCCESS = 'LOGIN_SUCCESS'
export const LOGIN_FAILED = 'LOGIN_FAILED'
export const LOGOUT = 'LOGOUT'

export const JWT_TOKEN_CREATE = 'JWT_TOKEN_CREATE'
export const JWT_TOKEN_DESTROY = 'JWT_TOKEN_DESTROY'
export const REFRESH_TOKEN_CREATE = 'REFRESH_TOKEN_CREATE'
export const REFRESH_TOKEN_DESTROY = 'REFRESH_TOKEN_DESTROY'

export const CSRF_CREATE = 'CSRF_TOKEN_CREATE'
export const CSRF_DESTROY = 'CSRF_TOKEN_DESTROY'

export const GET_MODULES = 'GET_MODULES'

export const USER_UPDATE_AVATAR = 'USER_UPDATE_AVATAR'
export const USER_UPDATE_DISPLAY_NAME = 'USER_UPDATE_DISPLAY_NAME'

/*
 * Library
 */

export const LIBRARY_CLEAR = 'LIBRARY_CLEAR'

export const LIBRARY_AUTHOR_GET_ALL = 'LIBRARY_AUTHOR_GET_ALL'
export const LIBRARY_BOOK_GET_ALL = 'LIBRARY_BOOK_GET_ALL'
export const LIBRARY_BOOKCASE_GET_ALL = 'LIBRARY_BOOKCASE_GET_ALL'
export const LIBRARY_GENRE_GET_ALL = 'LIBRARY_GENRE_GET_ALL'
export const LIBRARY_SERIES_GET_ALL = 'LIBRARY_SERIES_GET_ALL'
export const LIBRARY_SHELF_GET_ALL = 'LIBRARY_SHELF_GET_ALL'

export const LIBRARY_AUTHOR_ADD = 'LIBRARY_AUTHOR_ADD'
export const LIBRARY_BOOK_ADD = 'LIBRARY_BOOK_ADD'
export const LIBRARY_BOOKCASE_ADD = 'LIBRARY_BOOKCASE_ADD'
export const LIBRARY_SERIES_ADD = 'LIBRARY_SERIES_ADD'
export const LIBRARY_SHELF_ADD = 'LIBRARY_SHELF_ADD'

export const LIBRARY_AUTHOR_EDIT = 'LIBRARY_AUTHOR_EDIT'
export const LIBRARY_BOOK_EDIT = 'LIBRARY_BOOK_EDIT'
export const LIBRARY_BOOKCASE_EDIT = 'LIBRARY_BOOKCASE_EDIT'
export const LIBRARY_SERIES_EDIT = 'LIBRARY_SERIES_EDIT'
export const LIBRARY_SHELF_EDIT = 'LIBRARY_SHELF_EDIT'

export const LIBRARY_AUTHOR_DELETE = 'LIBRARY_AUTHOR_DELETE'
export const LIBRARY_BOOK_DELETE = 'LIBRARY_BOOK_DELETE'
export const LIBRARY_BOOKCASE_DELETE = 'LIBRARY_BOOKCASE_DELETE'
export const LIBRARY_SERIES_DELETE = 'LIBRARY_SERIES_DELETE'
export const LIBRARY_SHELF_DELETE = 'LIBRARY_SHELF_DELETE'

/*
 * Notifications
 */

export const MESSAGE_CLEAR = 'MESSAGE_CLEAR'
export const MESSAGE_PUSH = 'MESSAGE_PUSH'
export const MESSAGE_POP = 'MESSAGE_POP'
export const MESSAGE_OPEN_DRAWER = 'MESSAGE_OPEN_DRAWER'
export const MESSAGE_READ = 'MESSAGE_READ'


/*
 * Browser
 */

export const BROWSER_CLEAR = 'BROWSER_CLEAR'
export const BROWSER_LOAD_DIRECTORY = 'BROWSER_LOAD_DIRECTORY'
export const BROWSER_CHANGE_DIRECTORY = 'BROWSER_CHANGE_DIRECTORY'
export const BROWSER_PUSH_DIRECTORY = 'BROWSER_PUSH_DIRECTORY'
export const BROWSER_POP_DIRECTORY = 'BROWSER_POP_DIRECTORY'
export const BROWSER_GET_FOLDERS = 'BROWSER_GET_FOLDERS'
export const BROWSER_FILE_ADD = 'BROWSER_FILE_ADD'
export const BROWSER_FILE_DELETE = 'BROWSER_FILE_DELETE'


/*
 * Chat System
 */

export const CHAT_CONVERSATION_NOTIFICATIONS_GET_NEW = 'CHAT_CONVERSATION_NOTIFICATIONS_GET_NEW'
export const CHAT_CONVERSATION_GET_ALL = 'CHAT_CONVERSATION_GET_ALL'
export const CHAT_CONVERSATION_START_NEW = 'CHAT_CONVERSATION_START_NEW'
export const CHAT_CONVERSATION_DELETE = 'CHAT_CONVERSATION_DELETE'
export const CHAT_CONVERSATION_UPDATE_TITLE = 'CHAT_CONVERSATION_UPDATE_TITLE'
export const CHAT_CONVERSATION_GET_MESSAGES = 'CHAT_CONVERSATION_GET_MESSAGES'
export const CHAT_MESSAGE_ADD = 'CHAT_MESSAGE_ADD'
export const CHAT_MESSAGE_SEND = 'CHAT_MESSAGE_SEND'
export const CHAT_MESSAGE_DELETE = 'CHAT_MESSAGE_DELETE'
export const CHAT_MESSAGE_READ = 'CHAT_MESSAGE_READ'
export const CHAT_MESSAGE_HIGHLIGHT = 'CHAT_MESSAGE_HIGHLIGHT'
export const CHAT_MESSAGE_UNHIGHLIGHT = 'CHAT_MESSAGE_UNHIGHLIGHT'
