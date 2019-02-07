<template>
    <v-container>
        <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible>There was an error registering you</v-alert>
        <v-alert v-if="success" v-model="success" :value="true" type="success" dismissible>User {{ form.username }} successfully created</v-alert>
        <v-form ref="form" v-model="valid" lazy-validation>
            <v-text-field v-model="form.username"
                          :rules="rules.username"
                          v-bind:label="`Username`"
                          name="Username"
                          required></v-text-field>
            <v-text-field name="Password"
                          v-model="form.password"
                          v-bind:label="`Password`"
                          :append-icon="passwordOn ? 'visibility' : 'visibility_off'"
                          :type="passwordOn ? 'password' : 'text'"
                          :rules="rules.password"
                          required
                          @click:append="passwordOn = !passwordOn"></v-text-field>
            <v-text-field name="ConfirmPassword"
                          v-model="form.confirmPassword"
                          v-bind:label="`Confirm Password`"
                          :append-icon="passwordConfirmOn ? 'visibility' : 'visibility_off'"
                          :type="passwordConfirmOn ? 'password' : 'text'"
                          :rules="rules.confirmPassword"
                          required
                          @click:append="passwordConfirmOn = !passwordConfirmOn"></v-text-field>
            <v-select :items="getRoleList"
                      v-model="form.role"
                      v-bind:label="`Role`"
                      name="Role"
                      single-line></v-select>
            <v-btn :disabled="!valid || btnClicked" @click.prevent="register()">Register</v-btn>
        </v-form>
    </v-container>
</template>

<script>
    import Auth from '../../mixins/authentication'

    export default {
        mixins: [Auth],
        data() {
            return {
                passwordOn: true,
                passwordConfirmOn: true,
                form: {
                    username: '',
                    password: '',
                    confirmPassword: '',
                    role: null
                },
                rules: {
                    username: [],
                    password: [],
                    confirmPassword: []
                },
                show: true,
                valid: true,
                btnClicked: false,
                error: false,
                success: false,
            }
        },
        computed: {
            getRoleList: function() {
                return [
                    { value: 'Administrator', text: 'Administrator' },
                    { value: 'Elevated', text: 'Elevated' },
                    { value: 'User', text: 'User' },
                ]
            }
        },
        beforeDestroy() {
            window.removeEventListener('keyup', this.enterKeyListener)
        },
        mounted() {
            window.addEventListener('keyup', this.enterKeyListener)

            this.form.role = (this.getRoleList)[0].value;
        },
        methods: {
            enterKeyListener(e) {
                if (e.keyCode === 13) {
                    if (this.valid && !this.btnClicked)
                        this.register();
                }
            },
            register() {
                this.btnClicked = true;
                this.$_auth_register(this.form).then(() => {
                    this.error = false;
                    this.success = true;
                }).catch(() => {
                    this.error = true;
                    this.success = false;
                }).then(() => this.btnClicked = false);
            }
        }
    }
</script>
