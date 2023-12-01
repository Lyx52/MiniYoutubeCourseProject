const MAIN_VIDEO_PLAYER = "main-video-player";
const EVENTS = [
    // HTMLMediaElement events
    'abort',
    'canplay',
    'canplaythrough',
    'durationchange',
    'emptied',
    'ended',
    'error',
    'loadeddata',
    'loadedmetadata',
    'loadstart',
    'pause',
    'play',
    'playing',
    'progress',
    'ratechange',
    'seeked',
    'seeking',
    'stalled',
    'suspend',
    'timeupdate',
    'volumechange',
    'waiting',

    // HTMLVideoElement events
    'enterpictureinpicture',
    'leavepictureinpicture',

    // Element events
    'fullscreenchange',
    'resize',

    // video.js events
    'audioonlymodechange',
    'audiopostermodechange',
    'controlsdisabled',
    'controlsenabled',
    'debugon',
    'debugoff',
    'disablepictureinpicturechanged',
    'dispose',
    'enterFullWindow',
    'error',
    'exitFullWindow',
    'firstplay',
    'fullscreenerror',
    'languagechange',
    'loadedmetadata',
    'loadstart',
    'playerreset',
    'playerresize',
    'posterchange',
    'ready',
    'textdata',
    'useractive',
    'userinactive',
    'usingcustomcontrols',
    'usingnativecontrols',
];
const Plugin = videojs.getPlugin('plugin');

class ExamplePlugin extends Plugin {

    constructor(player, options) {
        console.debug(player);
        super(player, options);

        if (options.customClass) {
            player.addClass(options.customClass);
        }

        player.on('playing', function() {
            console.debug(this);
            videojs.log('playback began!');
        });
    }
}
function initVideoPlayer(videoPlayerElem, dotnetHelper) {
    const player = videojs(videoPlayerElem.id);
}

function getMainVideoPlayer() {
    return videojs(MAIN_VIDEO_PLAYER);
}
function playVideoPlayer(videoElement, dotnetHelper) {
    
}
function initMainVideoPlayer(videoElement, dotnetHelper) {
    const player = videojs(videoElement.id);
    
    player.ready((e) => {
        dotnetHelper.invokeMethodAsync('OnVideoPlayerReady', 'Video.js player is ready');
    });
};