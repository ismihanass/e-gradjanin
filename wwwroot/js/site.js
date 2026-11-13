// Shared client-side behavior. Server-side validation remains authoritative.
document.addEventListener('change', function (event) {
  if (event.target.matches('input[type=file]')) {
    const file = event.target.files && event.target.files[0];
    if (file && file.size > 10 * 1024 * 1024) {
      alert('Datoteka je veća od dozvoljenih 10 MB.');
      event.target.value = '';
    }
  }
});
