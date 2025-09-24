




function SetData(menu, table) {
    alert("dd");
    const headers = [].slice.call(table.querySelectorAll('th'));
    const cells = [].slice.call(table.querySelectorAll('th, td'));
    const numColumns = headers.length;

    const thead = table.querySelector('thead');
    thead.addEventListener('contextmenu', function (e) {
        e.preventDefault();

        const rect = thead.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        menu.style.top = `${y}px`;
        menu.style.left = `${x}px`;


        menu.classList.toggle('Conti__menu--hidden');

        document.addEventListener('click', documentClickHandler);
    });

    // Hide the menu when clicking outside of it
    const documentClickHandler = function (e) {
        const isClickedOutside = !menu.contains(e.target);

        if (isClickedOutside) {
            menu.classList.add('Conti__menu--hidden');
            document.removeEventListener('click', documentClickHandler);
        }
    };

    const showColumn = function (index) {
        cells
            .filter(function (cell) {
                return cell.getAttribute('data-column-index') === `${index}`;
            })
            .forEach(function (cell) {
                cell.style.display = '';
                cell.setAttribute('data-shown', 'true');
            });

        menu.querySelectorAll(`[type="checkbox"][disabled]`).forEach(function (checkbox) {
            checkbox.removeAttribute('disabled');
        });
    };

    const hideColumn = function (index) {
        cells
            .filter(function (cell) {
                return cell.getAttribute('data-column-index') === `${index}`;
            })
            .forEach(function (cell) {
                cell.style.display = 'none';
                cell.setAttribute('data-shown', 'false');
            });
        // How many columns are hidden
        const numHiddenCols = headers
            .filter(function (th) {
                return th.getAttribute('data-shown') === 'false';
            })
            .length;
        if (numHiddenCols === numColumns - 1) {
            // There's only one column which isn't hidden yet
            // We don't allow user to hide it
            const shownColumnIndex = thead.querySelector('[data-shown="true"]').getAttribute('data-column-index');

            const checkbox = menu.querySelector(`[type="checkbox"][data-column-index="${shownColumnIndex}"]`);
            checkbox.setAttribute('disabled', 'true');
        }
    };

    cells.forEach(function (cell, index) {
        cell.setAttribute('data-column-index', index % numColumns);
        cell.setAttribute('data-shown', 'true');
    });

    headers.forEach(function (th, index) {
        // Build the menu item
        const li = document.createElement('li');
        const label = document.createElement('label');
        const checkbox = document.createElement('input');
        checkbox.setAttribute('type', 'checkbox');
        checkbox.setAttribute('checked', 'true');
        checkbox.setAttribute('data-column-index', index);
        checkbox.style.marginRight = '.25rem';

        const text = document.createTextNode(th.textContent);

        label.appendChild(checkbox);
        label.appendChild(text);
        label.style.display = 'flex';
        label.style.alignItems = 'center';
        li.appendChild(label);
        menu.appendChild(li);

        // Handle the event
        checkbox.addEventListener('change', function (e) {
            e.target.checked ? showColumn(index) : hideColumn(index);
            menu.classList.add('Conti__menu--hidden');
        });
    });
}