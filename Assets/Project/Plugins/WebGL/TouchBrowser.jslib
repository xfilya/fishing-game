mergeInto(LibraryManager.library, {
    IsTouchBrowser: function () {
        return navigator.maxTouchPoints > 0 && window.matchMedia('(pointer: coarse)').matches ? 1 : 0;
    }
});
