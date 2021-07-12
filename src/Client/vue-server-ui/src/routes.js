import { Roles } from './constants'

import Index from './components/index'
import Login from './components/modules/login'
import Register from './components/modules/register'

import Home from './components/home'
import Start from './components/pages/start'
import Profile from './components/pages/profile'
import AdminTools from './components/pages/admin-tools'
import Chat from './components/pages/chat-messaging'
import Browser from './components/pages/browser'
import Notes from './components/pages/notes'
import Documentation from './components/pages/documentation'
import Doc from './components/modules/doc'
import Weight from './components/pages/weight'
import Library from './components/pages/library'

export const baseRoutes = [
  {
    path: '/',
    name: 'index',
    component: Index,

    meta: {
      display: 'Login',
      authLevel: Roles.Level.Default,
    },
    children: [
      {
        path: 'login',
        name: 'login',
        component: Login,
        meta: {
          display: 'Login',
        },
      },
      {
        path: 'register',
        name: 'register',
        component: Register,
        meta: {
          display: 'Register',
        },
      },
    ],
  },
  {
    path: '/home',
    name: 'home',
    component: Home,
    meta: {
      display: 'Home',
      authLevel: Roles.Level.Default,
    },
    children: [
      {
        path: 'start',
        name: 'start',
        component: Start,
        meta: {
          display: 'Home',
          authLevel: Roles.Level.Default,
          hidden: false,
        },
      },
    ],
  },
]

export const adminToolsRoute = {
  path: 'admin-tools',
  name: 'admin-tools',
  component: AdminTools,
  meta: {
    display: 'Admin Tools',
    authLevel: Roles.Level.Admin,
    hidden: false,
  },
}

export const defaultRoutes = [
  {
    path: 'profile',
    name: 'profile',
    component: Profile,
    meta: {
      display: 'Profile',
      authLevel: Roles.Level.Default,
      hidden: true,
    },
  },
]

export const moduleRoutes = [
  {
    path: 'chat',
    name: 'chat',
    component: Chat,
    meta: {
      display: 'Chat',
      authLevel: Roles.Level.General,
      hidden: false,
    },
  },
  {
    path: 'browser',
    name: 'browser',
    component: Browser,
    meta: {
      display: 'Browser',
      authLevel: Roles.Level.General,
      hidden: false,
    },
  },
  {
    path: 'browser/:folder*',
    name: 'browser-folder',
    component: Browser,
    meta: {
      display: 'Browser',
      relative: 'browser',
      authLevel: Roles.Level.General,
      hidden: true,
    },
  },
  {
    path: 'notes',
    name: 'notes',
    component: Notes,
    meta: {
      display: 'Notes',
      authLevel: Roles.Level.General,
      hidden: false,
    },
  },
  {
    path: 'doc',
    name: 'documentation',
    component: Documentation,
    meta: {
      display: 'Documentation',
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
      },
    ],
  },
  {
    path: 'weight',
    name: 'weight',
    component: Weight,
    meta: {
      display: 'Weight',
      authLevel: Roles.Level.General,
      hidden: false,
    },
  },
  {
    path: 'library',
    name: 'library',
    component: Library,
    meta: {
      display: 'Library',
      authLevel: Roles.Level.General,
      hidden: false,
    },
  },
]
