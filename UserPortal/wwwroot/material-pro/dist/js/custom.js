
function TableColumnHideNdShow(menuList, tableList) {
  
    const menu = menuList;
    const table = tableList;
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

function FilterGrid(txtInput, tblTable, DivMsg) {
   
    // Declare variables
    var input, filter, table, tr,trAfter, td, i, txtValue;
    var DivMsgEle = document.getElementById(DivMsg.id);
    input = document.getElementById(txtInput.id);
    filter = input.value.toUpperCase();
    table = document.getElementById(tblTable.id);
    tr = table.getElementsByTagName("tr");
    var Vis=1;
   
    // Loop through all table rows, and hide those who don't match the search query
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td");
        for (j = 0; j < td.length; j++)
        {
            if (td[j]) {
                txtValue = td[j].textContent || td[j].innerText;
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    tr[i].style.display = "";
                   
                    break;
                } else {
                    tr[i].style.display = "none";
                  
                   
                   
                }
            }


        }


    }

    for (k = 0; k < tr.length; k++)
    {
        if (tr[k].style.display == "none")
        {
            Vis = Vis + 1;
        }
    }
   
    if (Vis == tr.length) {
        DivMsgEle.innerText = "No Data Found";
    } else
    {
        DivMsgEle.innerText = "";
    }
}
function ChangeDateYYMMDD(TxtDate)
{
    const DataDate = TxtDate.split("-");
   
    return DataDate[2] + '-' + DataDate[1] + '-' + DataDate[0];

}