// apiUtils.js - API utilities and AJAX helper functions
// Centralized API communication for ASP.NET Core MVC integration

// API Configuration
const API_CONFIG = {
    baseUrl: window.location.origin,
    defaultHeaders: {
        'Content-Type': 'application/json'
    },
    timeout: 10000, // 10 seconds
    retryAttempts: 3,
    retryDelay: 1000 // 1 second
};

// CORE API FUNCTIONS -----------------------------------------------------------------------------------------

/**
 * Make an authenticated API request
 * @param {string} endpoint - The API endpoint
 * @param {object} options - Request options
 * @returns {Promise} - Response promise
 */
async function apiRequest(endpoint, options = {}) {
    const {
        method = 'GET',
        data = null,
        headers = {},
        timeout = API_CONFIG.timeout,
        retries = API_CONFIG.retryAttempts
    } = options;

    // Prepare request configuration
    const requestConfig = {
        method: method.toUpperCase(),
        headers: {
            ...API_CONFIG.defaultHeaders,
            ...headers
        }
    };

    // Add request verification token for POST requests
    if (method.toUpperCase() !== 'GET') {
        const token = getRequestVerificationToken();
        if (token) {
            requestConfig.headers['RequestVerificationToken'] = token;
        }
    }

    // Add request body for non-GET requests
    if (data && method.toUpperCase() !== 'GET') {
        requestConfig.body = JSON.stringify(data);
    }

    // Create abort controller for timeout
    const controller = new AbortController();
    requestConfig.signal = controller.signal;

    // Set timeout
    const timeoutId = setTimeout(() => {
        controller.abort();
    }, timeout);

    try {
        const response = await fetch(`${API_CONFIG.baseUrl}${endpoint}`, requestConfig);
        clearTimeout(timeoutId);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        return result;
    } catch (error) {
        clearTimeout(timeoutId);

        // Retry logic for network errors
        if (retries > 0 && (error.name === 'AbortError' || error.name === 'TypeError')) {
            console.log(`Retrying request to ${endpoint}. Attempts remaining: ${retries - 1}`);
            await delay(API_CONFIG.retryDelay);
            return apiRequest(endpoint, { ...options, retries: retries - 1 });
        }

        throw error;
    }
}

/**
 * GET request helper
 */
async function apiGet(endpoint, options = {}) {
    return apiRequest(endpoint, { ...options, method: 'GET' });
}

/**
 * POST request helper
 */
async function apiPost(endpoint, data, options = {}) {
    return apiRequest(endpoint, { ...options, method: 'POST', data });
}

/**
 * PUT request helper
 */
async function apiPut(endpoint, data, options = {}) {
    return apiRequest(endpoint, { ...options, method: 'PUT', data });
}

/**
 * DELETE request helper
 */
async function apiDelete(endpoint, options = {}) {
    return apiRequest(endpoint, { ...options, method: 'DELETE' });
}

// SPECIALIZED API FUNCTIONS -----------------------------------------------------------------------------------------

/**
 * Quest-related API calls
 */
const QuestAPI = {
    /**
     * Start a quest
     */
    async startQuest(questId) {
        try {
            const result = await apiPost('/Home/StartQuest', { questId });

            if (result.success) {
                console.log('Quest started successfully:', questId);
                return result;
            } else {
                throw new Error(result.message || 'Failed to start quest');
            }
        } catch (error) {
            console.error('Error starting quest:', error);
            throw error;
        }
    },

    /**
     * Claim quest reward
     */
    async claimQuest(questId) {
        try {
            const result = await apiPost('/Home/ClaimQuest', { questId });

            if (result.success) {
                console.log('Quest claimed successfully:', questId);
                return result;
            } else {
                throw new Error(result.message || 'Failed to claim quest');
            }
        } catch (error) {
            console.error('Error claiming quest:', error);
            throw error;
        }
    },

    /**
     * Get quest progress
     */
    async getQuestProgress(questId) {
        try {
            return await apiGet(`/Home/GetQuestProgress/${questId}`);
        } catch (error) {
            console.error('Error getting quest progress:', error);
            throw error;
        }
    },

    /**
     * Get all active quests
     */
    async getActiveQuests() {
        try {
            return await apiGet('/Home/GetActiveQuests');
        } catch (error) {
            console.error('Error getting active quests:', error);
            throw error;
        }
    }
};

/**
 * User-related API calls
 */
const UserAPI = {
    /**
     * Get user profile
     */
    async getProfile() {
        try {
            return await apiGet('/User/GetProfile');
        } catch (error) {
            console.error('Error getting user profile:', error);
            throw error;
        }
    },

    /**
     * Update user profile
     */
    async updateProfile(profileData) {
        try {
            return await apiPost('/User/UpdateProfile', profileData);
        } catch (error) {
            console.error('Error updating user profile:', error);
            throw error;
        }
    },

    /**
     * Get user token balance
     */
    async getTokenBalance() {
        try {
            return await apiGet('/User/GetTokenBalance');
        } catch (error) {
            console.error('Error getting token balance:', error);
            throw error;
        }
    }
};

/**
 * Item-related API calls
 */
const ItemAPI = {
    /**
     * Upload new item
     */
    async uploadItem(itemData) {
        try {
            return await apiPost('/Items/Upload', itemData);
        } catch (error) {
            console.error('Error uploading item:', error);
            throw error;
        }
    },

    /**
     * Get user's items
     */
    async getUserItems(page = 1, limit = 10) {
        try {
            return await apiGet(`/Items/GetUserItems?page=${page}&limit=${limit}`);
        } catch (error) {
            console.error('Error getting user items:', error);
            throw error;
        }
    },

    /**
     * Delete item
     */
    async deleteItem(itemId) {
        try {
            return await apiDelete(`/Items/Delete/${itemId}`);
        } catch (error) {
            console.error('Error deleting item:', error);
            throw error;
        }
    }
};

/**
 * Message-related API calls
 */
const MessageAPI = {
    /**
     * Send message
     */
    async sendMessage(messageData) {
        try {
            return await apiPost('/Messages/Send', messageData);
        } catch (error) {
            console.error('Error sending message:', error);
            throw error;
        }
    },

    /**
     * Get conversations
     */
    async getConversations() {
        try {
            return await apiGet('/Messages/GetConversations');
        } catch (error) {
            console.error('Error getting conversations:', error);
            throw error;
        }
    },

    /**
     * Get messages for conversation
     */
    async getMessages(conversationId, page = 1) {
        try {
            return await apiGet(`/Messages/GetMessages/${conversationId}?page=${page}`);
        } catch (error) {
            console.error('Error getting messages:', error);
            throw error;
        }
    }
};

// UTILITY FUNCTIONS -----------------------------------------------------------------------------------------

/**
 * Get the request verification token
 */
function getRequestVerificationToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');

    return tokenInput?.value || tokenMeta?.getAttribute('content') || '';
}

/**
 * Create a delay promise
 */
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Handle API errors with user-friendly messages
 */
function handleApiError(error, context = '') {
    let userMessage = 'An unexpected error occurred. Please try again.';

    if (error.name === 'AbortError') {
        userMessage = 'Request timed out. Please check your connection and try again.';
    } else if (error.message.includes('400')) {
        userMessage = 'Invalid request. Please check your input and try again.';
    } else if (error.message.includes('401')) {
        userMessage = 'You need to log in to perform this action.';
        // Redirect to login page
        window.location.href = '/Account/Login';
        return;
    } else if (error.message.includes('403')) {
        userMessage = 'You do not have permission to perform this action.';
    } else if (error.message.includes('404')) {
        userMessage = 'The requested resource was not found.';
    } else if (error.message.includes('500')) {
        userMessage = 'Server error. Please try again later.';
    }

    // Log error for debugging
    console.error(`API Error${context ? ' in ' + context : ''}:`, error);

    // Show user-friendly error message
    showErrorNotification(userMessage);

    return userMessage;
}

/**
 * Show loading state for API calls
 */
function showLoadingState(element, loadingText = 'Loading...') {
    if (!element) return;

    const originalText = element.textContent;
    const originalDisabled = element.disabled;

    element.textContent = loadingText;
    element.disabled = true;
    element.classList.add('loading');

    // Return function to restore original state
    return function restoreState() {
        element.textContent = originalText;
        element.disabled = originalDisabled;
        element.classList.remove('loading');
    };
}

/**
 * Show error notification
 */
function showErrorNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'api-error-notification';
    notification.innerHTML = `
        <div class="notification-content">
            <span class="error-icon">⚠️</span>
            <span class="error-message">${message}</span>
            <button class="close-btn" aria-label="Close">&times;</button>
        </div>
    `;

    document.body.appendChild(notification);

    // Show notification
    setTimeout(() => notification.classList.add('show'), 100);

    // Auto hide after 5 seconds
    const hideTimer = setTimeout(() => {
        hideNotification(notification);
    }, 5000);

    // Close button
    notification.querySelector('.close-btn').addEventListener('click', () => {
        clearTimeout(hideTimer);
        hideNotification(notification);
    });
}

/**
 * Hide notification
 */
function hideNotification(notification) {
    notification.classList.remove('show');
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 300);
}

/**
 * Validate response data
 */
function validateResponse(response, requiredFields = []) {
    if (!response || typeof response !== 'object') {
        throw new Error('Invalid response format');
    }

    for (const field of requiredFields) {
        if (!(field in response)) {
            throw new Error(`Missing required field: ${field}`);
        }
    }

    return true;
}

/**
 * Create form data from object
 */
function createFormData(data) {
    const formData = new FormData();

    for (const [key, value] of Object.entries(data)) {
        if (value instanceof File) {
            formData.append(key, value);
        } else if (Array.isArray(value)) {
            value.forEach((item, index) => {
                formData.append(`${key}[${index}]`, item);
            });
        } else if (value !== null && value !== undefined) {
            formData.append(key, value.toString());
        }
    }

    return formData;
}

// EXPORT API FUNCTIONS -----------------------------------------------------------------------------------------

// Export main API functions
window.TidyUpAPI = {
    // Core functions
    apiRequest,
    apiGet,
    apiPost,
    apiPut,
    apiDelete,

    // Specialized APIs
    Quest: QuestAPI,
    User: UserAPI,
    Item: ItemAPI,
    Message: MessageAPI,

    // Utilities
    handleApiError,
    showLoadingState,
    validateResponse,
    createFormData,
    getRequestVerificationToken
};

// For backwards compatibility
window.TidyUpQuests = QuestAPI;
window.TidyUpMessages = MessageAPI;