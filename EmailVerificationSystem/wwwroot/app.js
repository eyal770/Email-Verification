// Email Verification System - Main Application Logic
class EmailVerificationApp {
    constructor() {
        this.initializeElements();
        this.bindEvents();
        this.setupValidation();
    }

    initializeElements() {
        this.form = document.getElementById('emailForm');
        this.emailInput = document.getElementById('email');
        this.emailError = document.getElementById('emailError');
        this.submitBtn = document.getElementById('submitBtn');
        this.loading = document.getElementById('loading');
        this.message = document.getElementById('message');
    }

    bindEvents() {
        this.form.addEventListener('submit', this.handleSubmit.bind(this));
        this.emailInput.addEventListener('input', this.handleEmailInput.bind(this));
        this.emailInput.addEventListener('focus', this.handleEmailFocus.bind(this));
    }

    setupValidation() {
        // Real-time email validation
        this.emailInput.addEventListener('blur', this.validateEmailOnBlur.bind(this));
    }

    // Email validation using modern regex
    validateEmail(email) {
        const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
        return emailRegex.test(email);
    }

    // Handle email input changes
    handleEmailInput(event) {
        const email = event.target.value.trim();
        
        if (email && !this.validateEmail(email)) {
            this.showError('Please enter a valid email address');
        } else {
            this.hideError();
        }
    }

    // Validate email on blur
    validateEmailOnBlur(event) {
        const email = event.target.value.trim();
        
        if (email && !this.validateEmail(email)) {
            this.showError('Please enter a valid email address');
        }
    }

    // Show error message
    showError(message) {
        this.emailInput.classList.add('error');
        this.emailError.textContent = message;
        this.emailError.style.display = 'block';
    }

    // Hide error message
    hideError() {
        this.emailInput.classList.remove('error');
        this.emailError.style.display = 'none';
    }

    // Show message
    showMessage(text, type) {
        this.message.textContent = text;
        this.message.className = `message ${type}`;
        this.message.style.display = 'block';
        
        // Auto-hide success messages after 5 seconds
        if (type === 'success') {
            setTimeout(() => {
                this.hideMessage();
            }, 5000);
        }
    }

    // Hide message
    hideMessage() {
        this.message.style.display = 'none';
    }

    // Show loading state
    showLoading() {
        this.loading.style.display = 'block';
        this.submitBtn.disabled = true;
        this.submitBtn.textContent = 'Sending...';
    }

    // Hide loading state
    hideLoading() {
        this.loading.style.display = 'none';
        this.submitBtn.disabled = false;
        this.submitBtn.textContent = 'Send Verification Email';
    }

    // Handle email focus
    handleEmailFocus() {
        this.hideMessage();
        this.hideError();
    }

    // Handle form submission
    async handleSubmit(event) {
        event.preventDefault();
        
        const email = this.emailInput.value.trim();
        
        // Validate email
        if (!email) {
            this.showError('Email address is required');
            return;
        }
        
        if (!this.validateEmail(email)) {
            this.showError('Please enter a valid email address');
            return;
        }

        // Clear previous messages and errors
        this.hideMessage();
        this.hideError();

        // Show loading state
        this.showLoading();

        try {
            const response = await this.sendVerificationRequest(email);
            const data = await response.json();

            if (response.ok) {
                this.showMessage(data.message || 'Verification email sent successfully!', 'success');
                this.form.reset();
            } else {
                this.showMessage(data.message || 'An error occurred. Please try again.', 'error');
            }
        } catch (error) {
            console.error('Error:', error);
            this.showMessage('Network error. Please check your connection and try again.', 'error');
        } finally {
            this.hideLoading();
        }
    }

    // Send verification request
    async sendVerificationRequest(email) {
        return await fetch('/api/verification/submit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ email: email })
        });
    }
}

// Initialize the application when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new EmailVerificationApp();
});

// Add some modern features
if ('serviceWorker' in navigator) {
    // Register service worker for offline capabilities
    navigator.serviceWorker.register('/sw.js')
        .then(registration => console.log('ServiceWorker registered'))
        .catch(error => console.log('ServiceWorker registration failed'));
}

// Add keyboard shortcuts
document.addEventListener('keydown', (event) => {
    // Ctrl/Cmd + Enter to submit form
    if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
        const submitBtn = document.getElementById('submitBtn');
        if (submitBtn && !submitBtn.disabled) {
            submitBtn.click();
        }
    }
});
