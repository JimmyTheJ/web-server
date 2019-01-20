<template>
    <v-container>
        <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible>There was an error registering you</v-alert>
        <v-form ref="form" v-model="valid" lazy-validation>
            <v-text-field v-model="form.username"
                          :rules="rules.username"
                          v-bind:label="`Username`"
                          name="Username"
                          required
                          @keyup.enter="register(form)"></v-text-field>
            <v-text-field name="Password"
                          v-model="form.password"
                          v-bind:label="`Password`"
                          :append-icon="passwordOn ? 'visibility' : 'visibility_off'"
                          :type="passwordOn ? 'password' : 'text'"
                          :rules="rules.password"
                          required
                          @click:append="passwordOn = !passwordOn"
                          @keyup.enter="register(form)"></v-text-field>
            <v-text-field name="ConfirmPassword"
                          v-model="form.confirmPassword"
                          v-bind:label="`Confirm Password`"
                          :append-icon="passwordConfirmOn ? 'visibility' : 'visibility_off'"
                          :type="passwordConfirmOn ? 'password' : 'text'"
                          :rules="rules.confirmPassword"
                          required
                          @click:append="passwordConfirmOn = !passwordConfirmOn"
                          @keyup.enter="register(form)"></v-text-field>
            <v-select :items="getRoleList"
                      v-model="form.role"
                      v-bind:label="`Role`"
                      name="Role"
                      single-line></v-select>
            <v-btn :disabled="!valid" @click.prevent="register(form)">Register</v-btn>
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
        mounted() {
            this.form.role = (this.getRoleList)[0].value;
        },
        methods: {

        }
    }
</script>
