function isVisibleOnScreen(elem) {
    var top = $(window).scrollTop()
    var bottom = $(window).height() + top;
    var pos = $(elem).offset().top

    return pos < bottom && pos > top;
}





