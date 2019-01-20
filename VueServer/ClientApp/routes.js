import Index from './components/index'
import Login from './components/modules/login'
import Register from './components/modules/register'

import Home from './components/home'
import Start from './components/pages/start'
import FileServer from './components/pages/file-server'
import Video from './components/pages/video'
import Upload from './components/pages/upload'
import Browser from './components/pages/browser'
import Notes from './components/pages/notes'

export const routes = [
    {
        path: '/', name: 'index', component: Index, display: 'Login',
        meta: {
            authLevel: 0,
        },
        children: [
            {
                path: 'login',
                name: 'login',
                component: Login,
                display: 'Login'
            },
            {
                path: 'register',
                name: 'register',
                component: Register,
                display: 'Register'
            },
        ]
    },
    {
        path: '/home', name: 'home', component: Home, display: 'Home',
        meta: {
            authLevel: 0,
        },
        children: [
            {
                path: 'start',
                name: 'start',
                component: Start,
                display: 'Home',
                meta: {
                    authLevel: 0,
                    hidden: false,
                }
            },
            {
                path: 'file-server',
                name: 'file-server',
                component: FileServer,
                display: 'Files',
                meta: {
                    authLevel: 1,
                    hidden: false,
                }
            },
            {
                path: 'file-server/:id/:folder',
                name: 'file-server-folder',
                component: FileServer,
                display: 'Files',
                meta: {
                    authLevel: 1,
                    hidden: true,
                }
            },
            {
                path: 'video-player',
                name: 'video-player',
                component: Video,
                display: 'Video',
                meta: {
                    authLevel: 1,
                    hidden: false,
                }
            },
            {
                path: 'upload',
                name: 'upload',
                component: Upload,
                display: 'Upload',
                meta: {
                    authLevel: 2
                }
            },
            {
                path: 'browser',
                name: 'browser',
                component: Browser,
                display: 'Browser',
                meta: {
                    authLevel: 2,
                    hidden: false,
                }
            },
            {
                path: 'browser/:id/:folder',
                name: 'browser-folder-subFolders',
                component: Browser,
                display: 'Browser',
                meta: {
                    authLevel: 2,
                    hidden: true,
                }
            },
            {
                path: 'notes',
                name: 'notes',
                component: Notes,
                display: 'Notes',
                meta: {
                    authLevel: 1,
                    hidden: false,
                }
            },
        ]
    },
]
