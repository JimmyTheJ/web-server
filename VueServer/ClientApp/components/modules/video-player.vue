<template>
    <div class="video-container center">
        <video id="video-player"
               class="video-player"
               ref="player"
               preload="none"
               controls
               :width="getWidth">
            <source :src="url" :type="type" />
        </video>
    </div>
</template>

<script>
    window.mobilecheck = function () {
        var check = false;
        (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true; })(navigator.userAgent || navigator.vendor || window.opera);
        return check;
    };

    export default {
        name: "video-player",
        data() {
            return {
                windowHeight: window.innerHeight,
                windowWidth: window.innerWidth,
                //player: {},
            }
        },
        props: {
            url: {
                type: String,
                required: true,
            },
            type: {
                type: String,
                required: true,
            }
        },
        computed: {
            getWidth: function () {
                let check = window.mobilecheck();
                this.$_console_log(`Check: ${check}`);

                if (check)
                    return 320;

                if (this.windowWidth >= 1904)
                    return 1785;
                else if (this.windowWidth >= 1264)
                    return 1185;
                else if (this.windowWidth >= 960)
                    return 900;
                else
                    return this.windowWidth * 0.95;
            },
        },
        watch: {
            url: function () {
                let player = document.getElementById('video-player');

                player.pause();
                player.load();

                if (player.paused) {
                    this.$_console_log('Player was paused. Playing now');
                    player.play();
                }
                else {
                    this.$_console_log('Player wasn\'t paused. Pausing.');
                    player.pause();
                }

                //let self = this;
                //setTimeout(() => {
                //    this.$_console_log('Playing new file!');
                //    let playing = this.player.play();

                //    if (playing !== undefined) {
                //        playing.then(_ => {
                //            this.$_console_log('Playback started');
                //        }).catch(error => {
                //            this.$_console_log('Error trying to play video');
                //            this.$_console_log(error);
                //        });
                //    }
                //    else {
                //        this.$_console_log('Playing is undefined :(');
                //    }                    
                //}, 10);
            },
        },
        mounted() {
            this.$nextTick(() => {
                window.addEventListener('resize', this.getWindowSize);
            });
        },
        beforeDestroy() {
            window.removeEventListener('resize', this.getWindowSize);
        },
        methods: {
            getWindowSize() {
                this.windowHeight = window.innerHeight;
                this.windowWidth = window.innerWidth;
            },
        },
    }
</script>

<style>
    .center {
        margin: 0 auto;
    }
    /*.border-red {
        border: 1px solid red;
    }*/

    /*.video-container {
        margin-bottom: 15px;
        margin-top: 5px;
    }

    .video-player {
        position: absolute;
        left: 0;
    }*/

    /*@media screen and (min-width: 240px) and (max-width: 767px) {
        .video-player {
            width: 540px;
        }
    }

    @media screen and (max-width: 540px) {
        .video-player {
            width: 490px;
        }
    }

    @media screen and (max-width: 380px) {
        .video-player {
            width: 330px;
        }
    }*/
</style>
