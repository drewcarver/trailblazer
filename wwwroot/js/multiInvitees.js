(function () {
  function initMultiInvitees(root) {
    const scope = root || document;

    scope.querySelectorAll('.multi-select-container').forEach((container) => {
      if (container.dataset.multiInviteesInitialized === 'true') {
        return;
      }

      const input = container.querySelector('input[type="text"][data-field-name]');
      if (!input) {
        return;
      }

      const listId = input.dataset.listId;
      const badgesId = input.dataset.badgesId;
      const fieldName = input.dataset.fieldName;
      const listElement = document.getElementById(listId);
      const badgesContainer = document.getElementById(badgesId);

      if (!listElement || !badgesContainer || !fieldName) {
        return;
      }

      container.dataset.multiInviteesInitialized = 'true';

      function allowedValues() {
        return Array.from(listElement.options).map((option) => option.value);
      }

      function badgeValues() {
        return Array.from(badgesContainer.querySelectorAll('[data-value]')).map((badge) => badge.dataset.value || '');
      }

      function createBadge(value) {
        const badge = document.createElement('span');
        badge.className = 'inline-flex items-center rounded-full bg-neutral-200 px-3 py-1 text-sm text-neutral-800';
        badge.dataset.value = value;

        const hiddenInput = document.createElement('input');
        hiddenInput.type = 'hidden';
        hiddenInput.name = fieldName;
        hiddenInput.value = value;

        const label = document.createElement('span');
        label.textContent = value;

        const removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.className = 'ml-2 text-red-600 hover:text-red-800';
        removeButton.innerHTML = '&times;';
        removeButton.addEventListener('click', () => {
          badge.remove();
          input.focus();
        });

        badge.append(hiddenInput, label, removeButton);
        badgesContainer.appendChild(badge);
      }

      input.addEventListener('input', () => {
        const value = input.value.trim();
        if (!value) {
          return;
        }

        if (!allowedValues().includes(value)) {
          return;
        }

        if (badgeValues().includes(value)) {
          input.value = '';
          return;
        }

        createBadge(value);
        input.value = '';
        input.focus();
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => initMultiInvitees(document));
  } else {
    initMultiInvitees(document);
  }

  document.addEventListener('htmx:load', (event) => {
    initMultiInvitees(event.target);
  });

  window.initMultiInvitees = initMultiInvitees;
})();
