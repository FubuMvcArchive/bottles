$(function () {
    $('.header').click(function () {
        var that = $(this);
        var id = '#' + this.id.substring(1);

        that.toggleClass('expanded');
        $(id).toggle();
    });
});
