// Email Verification Result Page - Main Application Logic
class VerificationResultPage {
    constructor() {
        this.initializePage();
    }

    initializePage() {
        const urlParams = new URLSearchParams(window.location.search);
        const status = urlParams.get('status');
        const message = urlParams.get('message');
        const email = urlParams.get('email');
        const token = urlParams.get('token');
        
        this.renderContent(status, message, email, token);
    }

    renderContent(status, message, email, token) {
        const content = document.getElementById('verificationContent');
        
        let icon, title, description, actions, details;
        
        switch(status) {
            case 'success':
                icon = '✓';
                title = 'Email Verified Successfully!';
                description = message || 'Your email has been successfully verified. Thank you!';
                actions = this.createSuccessActions();
                details = this.createDetails(email, token, true);
                break;
                
            case 'already-verified':
                icon = '✓';
                title = 'Already Verified';
                description = message || 'This email has already been verified.';
                actions = this.createSuccessActions();
                details = this.createDetails(email, token, true);
                break;
                
            case 'expired':
                icon = '⏰';
                title = 'Verification Expired';
                description = message || 'Your verification link has expired. Please request a new one.';
                actions = this.createExpiredActions(email);
                details = this.createDetails(email, token, false);
                break;
                
            case 'invalid':
                icon = '✗';
                title = 'Invalid Verification';
                description = message || 'The verification link is invalid or has expired.';
                actions = this.createInvalidActions();
                details = this.createDetails(email, token, false);
                break;
                
            case 'error':
                icon = '⚠️';
                title = 'Verification Error';
                description = message || 'An error occurred during verification.';
                actions = this.createErrorActions();
                details = this.createDetails(email, token, false);
                break;
                
            default:
                icon = '❓';
                title = 'Unknown Status';
                description = 'Unable to determine verification status.';
                actions = this.createDefaultActions();
                details = '';
        }
        
        content.innerHTML = `
            <div class="verification-icon ${status === 'success' || status === 'already-verified' ? 'success' : 'error'}">
                ${icon}
            </div>
            <h1 class="verification-title">${title}</h1>
            <p class="verification-message">${description}</p>
            <div class="verification-actions">
                ${actions}
            </div>
            ${details}
        `;
        
        // Add any special behaviors
        if (status === 'expired') {
            this.startCountdown();
        }
    }

    createSuccessActions() {
        return `
            <a href="/" class="btn">Back to Home</a>
        `;
    }

    createExpiredActions(email) {
        return `
            <a href="/" class="btn">Request New Verification</a>
            <a href="/contact" class="btn-secondary">Contact Support</a>
        `;
    }

    createInvalidActions() {
        return `
            <a href="/" class="btn">Try Again</a>
            <a href="/help" class="btn-secondary">Get Help</a>
        `;
    }

    createErrorActions() {
        return `
            <a href="/" class="btn">Go Home</a>
            <a href="/contact" class="btn-secondary">Contact Support</a>
        `;
    }

    createDefaultActions() {
        return `
            <a href="/" class="btn">Go Home</a>
        `;
    }

    createDetails(email, token, isVerified) {
        if (!email && !token) return '';
        
        return `
            <div class="verification-details">
                <h3>Verification Details</h3>
                ${email ? `<p><strong>Email:</strong> ${email}</p>` : ''}
                ${token ? `<p><strong>Token:</strong> <code>${token.substring(0, 8)}...</code></p>` : ''}
                <p><strong>Status:</strong> ${isVerified ? 'Verified' : 'Not Verified'}</p>
                <p><strong>Timestamp:</strong> ${new Date().toLocaleString()}</p>
            </div>
        `;
    }

    startCountdown() {
        // Add countdown logic if needed
        console.log('Verification expired - countdown started');
    }
}

// Initialize the page when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new VerificationResultPage();
});
