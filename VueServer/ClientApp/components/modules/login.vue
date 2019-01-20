<template>
    <v-container>
        <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible>Username or password incorrect</v-alert>
        <v-form ref="form" v-model="valid" lazy-validation>
            <v-text-field v-model="form.username"
                            :rules="rules.username"
                            v-bind:label="`Username`"
                            name="Username"
                            required
                            @keyup.enter="login(form)"></v-text-field>
            <v-text-field :append-icon="passwordOn ? 'visibility' : 'visibility_off'"
                            @click:append="passwordOn = !passwordOn"
                            :type="passwordOn ? 'password' : 'text'"
                            v-model="form.password"
                            name="Password"
                            :rules="rules.password"
                            v-bind:label="`Password`"
                            required
                            @keyup.enter="login(form)"></v-text-field>
            <!--<v-checkbox v-model="form.checked" v-bind:label="`Keep me signed in`"></v-checkbox>-->

            <v-btn :disabled="!valid" @click.prevent="login(form)">Login</v-btn>
        </v-form>
    </v-container>
</template>

<script>
    import Auth from '../../mixins/authentication'

    export default {
        data() {
            return {
                form: {
                    username: '',
                    password: '',
                    checked: false
                },
                passwordOn: true,
                show: true,
                valid: true,
                rules: {
                    username: [v => !!v || "Required"],
                    password: [v => !!v || "Required"],
                }
            }
        },
        mixins: [Auth],
    }
</script>
