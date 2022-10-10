function HighlightAll() {
    document.querySelectorAll('pre').forEach((block) => {
        hljs.highlightBlock(block);
    });
}

function ShowModal(id) {
    var myModal = new bootstrap.Modal(document.getElementById(id));
    myModal.show();
}

function ShowModalStatic(id) {
    var myModal = new bootstrap.Modal(document.getElementById(id), {
        backdrop: 'static'
    });
    myModal.show();
}

function HideModal(id) {
    var myModalEl = document.getElementById(id);
    var modal = bootstrap.Modal.getInstance(myModalEl);
    modal.hide();
}

document.onkeydown = function (t) {
    if (t.which == 9) {
        if (t.srcElement.type == 'textarea') {
            t.preventDefault();
            const TAB_SIZE = 4;
            document.execCommand('insertText', false, ' '.repeat(TAB_SIZE));
        }
    }
}

function BlazorScrollToId(id) {
    const element = document.getElementById(id);
    if (element instanceof HTMLElement) {
        element.scrollIntoView({
            behavior: "smooth",
            block: "start",
            inline: "nearest"
        });
    }
}
