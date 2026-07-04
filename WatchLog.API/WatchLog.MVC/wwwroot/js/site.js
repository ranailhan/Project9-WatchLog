// Site JavaScript
// WatchLog — site.js

document.addEventListener('DOMContentLoaded', () => {

    // Navbar scroll effect (backup, also in layout)
    const nav = document.getElementById('mainNav');
    if (nav) {
        window.addEventListener('scroll', () => {
            nav.classList.toggle('scrolled', window.scrollY > 50);
        }, { passive: true });
    }

    // Auto-hide toasts after 4 seconds
    const toasts = document.querySelectorAll('.wl-toast.show');
    if (toasts.length) {
        setTimeout(() => {
            toasts.forEach(t => {
                t.style.opacity = '0';
                t.style.transform = 'translateX(120%)';
                setTimeout(() => t.remove(), 400);
            });
        }, 4000);
    }

    // Confirm delete buttons
    document.querySelectorAll('[data-confirm]').forEach(btn => {
        btn.addEventListener('click', e => {
            if (!confirm(btn.dataset.confirm)) e.preventDefault();
        });
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    // Image lazy loading fallback
    document.querySelectorAll('img[loading="lazy"]').forEach(img => {
        img.addEventListener('error', function() {
            this.style.display = 'none';
            const placeholder = document.createElement('div');
            placeholder.className = 'wl-card-placeholder';
            placeholder.innerHTML = '<i class="fas fa-image"></i>';
            this.parentNode.appendChild(placeholder);
        });
    });

});
