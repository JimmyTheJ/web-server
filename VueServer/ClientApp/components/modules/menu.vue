<template>
    <div>
        <v-navigation-drawer style="top: 56px; overflow: hidden"
                             v-model="drawer"
                             class="grey lighten-4"
                             width="150"
                             height="340"
                             absolute
                             app>
            <v-list light>
                <template v-for="child in routes.children">
                    <v-list-tile :to="{ name: child.name }" v-if="authorized(child.meta.authLevel) && !child.meta.hidden">
                        <v-list-tile-content>
                            {{ child.display }}
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </v-list>
        </v-navigation-drawer>

        <v-toolbar color="purple" dark>
            <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
            <div v-show="$vuetify.breakpoint.name !== 'xs'">
                <v-toolbar-items dark>
                    <template v-for="(child, index) in routes.children">
                        <template v-if="authorized(child.meta.authLevel)">
                            <div v-show="(index+1) < maxMenuItems">
                                <!--<router-link :to="child.route"></router-link>-->
                                <template v-if="!child.meta.hidden">
                                    <v-btn dark flat @click="$router.push({ name: child.name })">{{ child.display }}</v-btn>
                                    <v-divider class="mx-3" inset vertical></v-divider>
                                </template>
                            </div>
                        </template>
                    </template>
                    <div v-show="maxMenuItems < menuItems">...</div>
                </v-toolbar-items>
            </div>
            <v-spacer></v-spacer>
            <v-menu :close-on-content-click="true"
                    :nudge-width="200"
                    v-model="menu"
                    offset-x>
                <v-btn slot="activator" icon dark>
                    <v-icon small>fas fa-ellipsis-v</v-icon>
                </v-btn>
                <v-card>
                    <v-list>
                        <v-list-tile>
                            <v-icon left>fas fa-user</v-icon>
                            <v-list-tile-title class="ml-2">{{ $store.getters.getUsername }}</v-list-tile-title>
                        </v-list-tile>
                        <v-list-tile @click="logout">
                            <v-icon left>fas fa-door-open</v-icon>
                            <v-list-tile-title class="ml-2">Logout</v-list-tile-title>
                        </v-list-tile>
                    </v-list>
                </v-card>
            </v-menu>
        </v-toolbar>
    </div>
</template>

<script>
    import Auth from '../../mixins/authentication'
    import { routes } from '../../routes'
    import { Roles } from '../../constants'
    import { setTimeout } from 'core-js';

    export default {
        data() {
            return {
                routes: [],
                drawer: false,

                menu: '',
                menuItems: 0,

                authLevel: 0,

                screenSize: window.outerWidth,
            }
        },
        mixins: [Auth],
        props: {
            source: String
        },
        computed: {
            getDrawerHeight() {
                return 120 + (this.menuItems * 40);
            },
            maxMenuItems() {
                return (this.screenSize - 150) / 170;
            },
        },
        created() {
            this.getAuthLevel();

            this.routes = routes.find(x => x.name === this.source);
            this.$_console_log(this.routes);

            this.getMenuItemCount();
        },
        mounted() {
            window.addEventListener('resize', this.resizeScreen, false)
        },
        beforeDestroy() {
            window.removeEventListener('resize', this.resizeScreen, false);
        },
        methods: {
            resizeScreen(e) {
                this.screenSize = window.outerWidth;
            },
            getAuthLevel() {
                let role = this.$store.getters.getUserRole;

                if (role === Roles.Name.Admin)
                    this.authLevel = Roles.Level.Admin;
                else if (role === Roles.Name.Elevated)
                    this.authLevel = Roles.Level.Elevated;
                else if (role === Roles.Name.General)
                    this.authLevel = Roles.Level.General;
                else
                    this.authLevel = Roles.Level.Default;
            },
            authorized(level) {
                if (level > this.authLevel) {
                    //this.$_console_log("[Menu] Level is greater than auth level. Not authorized");
                    return false;
                }

                return true;
            },
            getMenuItemCount() {
                this.$_console_log('[Menu] Get Menu Count.');
                for (let i = 0; i < this.routes.children.length; i++) {
                    let auth = this.authorized(this.routes.children[i].meta.authLevel);
                    if (auth)
                        this.menuItems++;
                }
            },
            //checkScreenSize(i) {
            //    this.$_console_log('Checking screen after resize... index value: ' + (i + 1));

            //    if ((this.screenSize - 50) / (i + 1) < 190) {
            //        this.screenOverflow = true;
            //        return false;
            //    }

            //    this.screenOverflow = false;
            //    return true;
            //}
        }
    }
</script>
