function loadPlayer(videoElement, options, sources, dotnetHelper) {
    const player = videojs(videoElement.id, options, function() {
        var player = this;

        player.controlBar.addChild('QualitySelector');
    });
    player.src(sources);
    player.ready((e) => {
        dotnetHelper.invokeMethodAsync('OnVideoPlayerReady', 'Video.js player is ready');
    });
}
function destroyPlayer(videoElement, dotnetHelper) {
    const player = videojs.getPlayers()[videoElement.id];
    if (player) {
        player.dispose();
        console.warn(`Disposing videojs player! ${videoElement.id}`);
    }
}