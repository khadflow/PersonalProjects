// JavaScript source code

function deleteNote(id) {
    fetch('/delete-note', { // send request to javascript backend (more in AJAX!)
        method: "POST",
        body: JSON.stringify({ noteId: id })
    }).then((_res) => {
        window.location.href = "/"; // Redirect to the homepage
    });
}