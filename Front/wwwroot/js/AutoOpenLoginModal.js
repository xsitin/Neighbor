function OpenModal() {
  if (document.location.href.indexOf('login') !== -1) {
    $("#login").modal('toggle');
  }
}
