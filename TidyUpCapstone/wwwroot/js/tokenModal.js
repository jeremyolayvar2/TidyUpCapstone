// REPLACE the entire content of tokenModal.js with this:

document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('tokenModal');
    const tokenBalanceElement = modal?.querySelector('h2');
    
    if (!modal) {
        console.warn('Token modal not found');
        return;
    }

    // Enhanced mouse tracking for spotlight effect
    modal.addEventListener('mousemove', (e) => {
        const rect = modal.getBoundingClientRect();
        modal.style.setProperty('--mouse-x', `${e.clientX - rect.left}px`);
        modal.style.setProperty('--mouse-y', `${e.clientY - rect.top}px`);
    });

    // Open token modal triggers
    document.querySelectorAll('.open-token').forEach(el => {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            showTokenModal();
        });
    });

    // Close on outside click
    document.addEventListener('click', (e) => {
        if (modal.classList.contains('show') && !modal.contains(e.target)) {
            hideTokenModal();
        }
    });

    // Close on escape key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.classList.contains('show')) {
            hideTokenModal();
        }
    });

    // Function to refresh token balance from server
    async function refreshTokenBalance() {
        if (!tokenBalanceElement) return;

        try {
            const response = await fetch('/Home/GetUserTokenBalance', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    // Update the token display with animation
                    tokenBalanceElement.style.transition = 'transform 0.3s ease';
                    tokenBalanceElement.style.transform = 'scale(1.1)';
                    tokenBalanceElement.textContent = data.tokenBalance.toLocaleString();
                    
                    setTimeout(() => {
                        tokenBalanceElement.style.transform = 'scale(1)';
                    }, 300);

                    console.log('Token balance updated:', data.tokenBalance);
                }
            }
        } catch (error) {
            console.error('Error refreshing token balance:', error);
        }
    }

    // Global function to show token modal
    window.showTokenModal = function() {
        modal.classList.add('show');
        
        // Refresh token balance when showing
        refreshTokenBalance();
        
        // Auto-close after 5 seconds
        setTimeout(() => {
            if (modal.classList.contains('show')) {
                hideTokenModal();
            }
        }, 5000);
        
        console.log('Token modal shown');
        return true;
    };

    // Global function to hide token modal
    window.hideTokenModal = function() {
        modal.classList.remove('show');
        console.log('Token modal hidden');
    };

    // Global function to refresh token balance (can be called from other scripts)
    window.refreshTokenBalance = refreshTokenBalance;
});