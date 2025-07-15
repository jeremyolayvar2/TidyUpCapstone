const container = document.querySelector('.container2');
const loginForm = document.querySelector('.login-form');
const signupForm = document.querySelector('.signup-form');
const signupBtn = document.getElementById('signup-btn');
const backLoginBtn = document.getElementById('back-login-btn');

// Function to switch forms and auto-resize container
function switchForm(showSignup) {
    if (showSignup) {
        loginForm.classList.remove('active');
        signupForm.classList.add('active');
    } else {
        signupForm.classList.remove('active');
        loginForm.classList.add('active');
    }

    // Trigger container resize to fit content
    const activeForm = showSignup ? signupForm : loginForm;
    const height = activeForm.offsetHeight;
    container.style.height = `${height + 140}px`; // +140 for padding/margin buffer
}

// Initial resize
window.addEventListener('DOMContentLoaded', () => {
    loginForm.classList.add('active');
    container.style.height = `${loginForm.offsetHeight + 140}px`;
});

function switchForm(showSignup) {
    if (showSignup) {
        loginForm.classList.remove('active');
        signupForm.classList.add('active');
    } else {
        signupForm.classList.remove('active');
        loginForm.classList.add('active');
    }

    const activeForm = showSignup ? signupForm : loginForm;
    const height = activeForm.offsetHeight;
    container.style.height = `${height + 140}px`; // ✅ use backticks
}

signupBtn.addEventListener('click', () => switchForm(true));
backLoginBtn.addEventListener('click', () => switchForm(false));

// --- Login functionality with welcome popup ---
document.getElementById('login-btn').addEventListener('click', async () => {
    const username = document.getElementById('log-username').value;
    const password = document.getElementById('log-password').value;

    try {
        const response = await fetch('/Account/Login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Username: username, Password: password })
        });

        const result = await response.json();
        if (response.ok) {
            if (result.showWelcomePopup) {
                showWelcomePopup();
                setTimeout(() => {
                    window.location.href = result.redirectUrl || '/Home/Home';
                }, 1200); // Show popup before redirect
            } else {
                window.location.href = result.redirectUrl || '/Home/Home';
            }
        } else {
            alert(result.message || "Login failed.");
        }
    } catch (e) {
        alert("Login failed. Please try again.");
    }
});

function showWelcomePopup() {
    const popup = document.createElement('div');
    popup.innerHTML = "🎉 Welcome! You've received 25 tokens as a registration reward.";
    popup.style.position = "fixed";
    popup.style.top = "20px";
    popup.style.right = "20px";
    popup.style.background = "#fffbe7";
    popup.style.border = "2px solid #FFB300";
    popup.style.padding = "16px";
    popup.style.fontSize = "1.1rem";
    popup.style.fontWeight = "bold";
    popup.style.borderRadius = "8px";
    popup.style.zIndex = "9999";
    popup.style.boxShadow = "0 2px 8px rgba(0,0,0,.2)";
    document.body.appendChild(popup);

    setTimeout(() => {
        popup.remove();
    }, 7000); // Show for 1.2 seconds before redirect
}


// --- Sign-up functionality ---
document.getElementById('create-account-btn').addEventListener('click', async () => {
    const username = document.getElementById('sign-username').value;
    const email = document.getElementById('sign-email').value;
    const password = document.getElementById('sign-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;

    console.log('Registering with:', {
        username,
        email,
        password,
        confirmPassword
    });

    try {
        const response = await fetch('/Account/Register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Username: username,
                Email: email,
                Password: password,
                ConfirmPassword: confirmPassword
            })
        });

        try {
            const result = await response.json();
            if (response.ok) {
                alert("Account created! You can now log in.");
                switchForm(false);
            } else {
                alert(result.message || "Sign-up failed.");
            }
        } catch (jsonError) {
            console.error("Failed to parse response as JSON:", jsonError);
            alert("Server error occurred. Check backend logs.");
        }

    } catch (networkError) {
        console.error("Network error:", networkError);
        alert("Failed to send request. Is the backend running?");
    }
});
