document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('tokenModal');

    document.querySelectorAll('.open-token').forEach(el => {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            modal.classList.add('show');
        });
    });

    modal.addEventListener('mousemove', (e) => {
        const rect = modal.getBoundingClientRect();
        modal.style.setProperty('--mouse-x', `${e.clientX - rect.left}px`);
        modal.style.setProperty('--mouse-y', `${e.clientY - rect.top}px`);
    });

    document.addEventListener('click', (e) => {
        if (!modal.contains(e.target)) {
            modal.classList.remove('show');
        }
    });
});