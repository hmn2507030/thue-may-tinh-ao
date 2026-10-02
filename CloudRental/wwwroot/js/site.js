document.addEventListener("DOMContentLoaded", function () {
  var toggle = document.getElementById("menu-toggle");
  var nav = document.getElementById("main-nav");
  if (toggle && nav) toggle.addEventListener("click", function () {
    var open = nav.classList.toggle("open");
    toggle.setAttribute("aria-expanded", String(open));
  });
  document.querySelectorAll("[data-toggle-password]").forEach(function (button) {
    button.addEventListener("click", function () {
      var input = document.getElementById(button.dataset.togglePassword);
      if (!input) return;
      var reveal = input.type === "password";
      input.type = reveal ? "text" : "password";
      button.textContent = reveal ? "Ẩn" : "Hiện";
      button.setAttribute("aria-pressed", String(reveal));
    });
  });
  document.querySelectorAll("[data-confirm]").forEach(function (form) {
    form.addEventListener("submit", function (event) { if (!window.confirm(form.dataset.confirm)) event.preventDefault(); });
  });
  var refresh = document.querySelector("[data-auto-refresh]");
  if (refresh) window.setTimeout(function () { window.location.reload(); }, Number(refresh.dataset.autoRefresh) || 5000);
  var form = document.getElementById("checkout-form");
  var months = document.getElementById("rental-months");
  if (form && months) {
    var monthly = Number(form.dataset.monthly);
    var total = document.getElementById("rental-total");
    var label = document.getElementById("month-label");
    var format = function (value) { return value.toLocaleString("vi-VN") + " ₫"; };
    months.addEventListener("change", function () {
      var count = Number(months.value) || 1;
      if (total) total.textContent = format(monthly * count);
      if (label) label.textContent = String(count);
    });
  }
});
