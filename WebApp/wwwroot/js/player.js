const videoPlayerConfig = {};
function loadPlayer(videoElement, options, sources, dotnetHelper) {
    videoPlayerConfig.player = videojs(videoElement.id, options, function() {
        var player = this;

        player.controlBar.addChild('QualitySelector');
    });
    loadPlayerSources(sources, dotnetHelper);
    videoPlayerConfig.player.on('canplay', () => {
        dotnetHelper.invokeMethodAsync('OnVideoPlayerReady', 'Video.js player is ready');
    })
}
function loadPlayerSources(sources, dotnetHelper) {
    if (videoPlayerConfig.player) {
        videoPlayerConfig.player.src(sources);
    }
}
function destroyPlayer(videoElement, dotnetHelper) {
    if (!videoElement) {
        console.warn(`Cannot dispose video player! ${videojs.getPlayers()}`);
        return;
    }
    const player = videojs.getPlayers()[videoElement.id];
    if (player) {
        player.dispose();
        console.warn(`Disposing videojs player! ${videoElement.id}`);
    }
}