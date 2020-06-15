import { Roles } from './constants'

import Index from './components/index'
import Login from './components/modules/login'
import Register from './components/modules/register'

import Home from './components/home'
import Start from './components/pages/start'
import AdminTools from './components/pages/admin-tools'
import Chat from './components/pages/chat-messaging'
import Browser from './components/pages/browser'
import Notes from './components/pages/notes'
import Documentation from './components/pages/documentation'
import Doc from './components/modules/doc'
import Weight from './components/pages/weight'
import Library from './components/pages/library'

export const routes = [
    {
        path: '/', name: 'index', component: Index, display: 'Login',
        meta: {
            authLevel: Roles.Level.Default,
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
            authLevel: Roles.Level.Default,
        },
        children: [
            {
                path: 'start',
                name: 'start',
                component: Start,
                display: 'Home',
                meta: {
                    authLevel: Roles.Level.Default,
                    hidden: false,
                }
            },
            {
                path: 'admin-tools',
                name: 'admin-tools',
                component: AdminTools,
                display: 'Admin Tools',
                meta: {
                    authLevel: Roles.Level.Admin,
                    hidden: false,
                }
            },
            {
                path: 'chat',
                name: 'chat',
                component: Chat,
                display: 'Chat',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: false,
                }
            },
            {
                path: 'browser',
                name: 'browser',
                component: Browser,
                display: 'Browser',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: false,
                }
            },
            {
                path: 'browser/:folder*',
                name: 'browser-folder',
                component: Browser,
                display: 'Browser',
                meta: {
                    relative: 'browser',
                    authLevel: Roles.Level.General,
                    hidden: true,
                }
            },
            {
                path: 'notes',
                name: 'notes',
                component: Notes,
                display: 'Notes',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: false,
                }
            },
            {
                path: 'doc',
                name: 'documentation',
                component: Documentation,
                display: 'Documentation',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: true,
                },
                children: [
                    {
                        path: ':id',
                        name: 'documentation-item',
                        component: Doc,
                        display: 'Documentation',
                        meta: {
                            relative: 'documentation',
                            authLevel: Roles.Level.General,
                            hidden: true,
                        },
                    }
                ],
            },
            {
                path: 'weight',
                name: 'weight',
                component: Weight,
                display: 'Weight',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: false,
                }
            },
            {
                path: 'library',
                name: 'library',
                component: Library,
                display: 'Library',
                meta: {
                    authLevel: Roles.Level.General,
                    hidden: false,
                }
            },
        ]
    },
]
