<template>
    <div>
        <v-container>
            <template v-for="line in lines">
                <v-row>
                    <span v-for="letter in line">
                        <template v-if="letter === ' '">
                            &nbsp;
                        </template>
                        <template v-if="letter === '\t'">
                            &nbsp;&nbsp;&nbsp;&nbsp;
                        </template>
                        <template v-if="letter === '\r'">
                            <!-- noop -->
                        </template>
                        <template v-else>
                            {{ letter }}
                        </template>
                    </span>
                </v-row>
            </template>
        </v-container>
    </div>
</template>

<script>
    import service from '../../../services/file-explorer'
    import DispatchFactory from '../../../factories/dispatchFactory'

    export default {
        name: "text-viewer",
        data() {
            return {
                text: null,
                lines: []
            }
        },
        props: {
            url: {
                type: String,
                required: true,
            },
            on: {
                type: Boolean,
                required: true,
            }
        },
        watch: {
            on(newValue) {
                if (newValue === true) {
                    this.lines = [];
                    this.getData();
                }                    
            },
            url(newValue) {
                this.$_console_log('[Text Viewer] Url watcher: url value', newValue)

                this.getData();
            },
        },
        methods: {
            getData() {
                if (typeof this.url === 'undefined' || this.url === null || this.url === '') {
                    this.$_console_log('[Text Viewer] Get Data: url value is null or empty');
                    return;
                }

                const tmpUrl = this.url;
                DispatchFactory.request(() => {
                    return service.getFile(tmpUrl).then(resp => {
                        this.$_console_log('File data:', resp.data);
                        let text = resp.data;
                        this.lines = [];

                        let line = '';
                        for (let i = 0; i < text.length; i++) {
                            // Separate new lines into different array objects so we can loop through them in the template
                            if (text[i] === '\n') {
                                this.lines.push(line);
                                line = '';
                            }
                            else {
                                line += text[i];
                            }
                        }
                        // Push what's left in the buffer to the list
                        this.lines.push(line);
                    }).catch(() => {
                        this.$_console_log(`[Text Viewer] Get Data: Failed to get data from ${tmpUrl}`);
                    });
                });
            },
        }
    }
</script>
