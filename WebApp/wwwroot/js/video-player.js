const MAIN_VIDEO_PLAYER = "main-video-player";
function getMainVideoPlayer() {
    return videojs(MAIN_VIDEO_PLAYER);
}
function initMainVideoPlayer(videoElement, dotnetHelper) {
    const player = videojs(videoElement.id);
    
    player.ready((e) => {
        dotnetHelper.invokeMethodAsync('OnVideoPlayerReady', 'Video.js player is ready');
    });
};