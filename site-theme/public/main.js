export default {
  start() {
    const configure = () => {
      const header = document.querySelector('header');
      if (!header || header.querySelector('.sferix-family-nav')) return;
      const theme = header.querySelector('[data-bs-theme-value], #theme-toggle, .theme-toggle, [aria-label*="theme" i], [title*="theme" i]');
      const themeHost = theme && (theme.closest('.nav-item') || theme.closest('.dropdown') || theme.parentElement);
      const search = header.querySelector('input[type="search"], input[placeholder*="Search" i]');
      const searchHost = search && (search.closest('form') || search.closest('.search') || search.parentElement);
      const toolbar = document.createElement('div');
      toolbar.className = 'sferix-family-nav';
      const toggle = document.createElement('button');
      toggle.type = 'button';
      toggle.className = 'sferix-family-button';
      toggle.setAttribute('aria-expanded', 'false');
      toggle.setAttribute('aria-haspopup', 'true');
      toggle.setAttribute('aria-label', 'Sferix Family');
      const desktop = document.createElement('span');
      desktop.className = 'sferix-family-long';
      desktop.textContent = 'Sferix Family';
      const mobile = document.createElement('span');
      mobile.className = 'sferix-family-short';
      mobile.textContent = 'Family';
      toggle.append(desktop, mobile, document.createTextNode(' ▾'));
      const panel = document.createElement('div');
      panel.className = 'sferix-family-menu';
      panel.hidden = true;
      const products = [
        ['FrankenSPO','https://frankenspo.sferix.app/','core.svg'],
        ['FrankenSPO MiddleTier Server','https://frankenspo-server.sferix.app/','server.svg'],
        ['FrankenSPO WebAPI','https://frankenspo-webapi.sferix.app/','webapi.svg']
      ];
      products.forEach(([label, url, icon]) => {
        const link = document.createElement('a');
        link.className = 'sferix-family-item';
        link.href = url;
        if (location.hostname === new URL(url).hostname) {
          link.setAttribute('aria-current', 'page');
          link.classList.add('sferix-current');
        }
        const img = document.createElement('img');
        img.src = '/assets/family/' + icon;
        img.alt = '';
        img.width = 24;
        img.height = 24;
        link.append(img, document.createTextNode(label));
        panel.appendChild(link);
      });
      const setOpen = open => { panel.hidden = !open; toggle.setAttribute('aria-expanded', String(open)); };
      toggle.addEventListener('click', () => setOpen(panel.hidden));
      document.addEventListener('pointerdown', e => { if (!toolbar.contains(e.target)) setOpen(false); });
      document.addEventListener('keydown', e => { if (e.key === 'Escape') { setOpen(false); toggle.focus(); } });
      toolbar.append(toggle, panel);
      if (themeHost && themeHost.parentElement) {
        themeHost.insertAdjacentElement('beforebegin', toolbar);
        themeHost.classList.add('sferix-theme-host');
      } else if (searchHost && searchHost.parentElement) {
        searchHost.insertAdjacentElement('beforebegin', toolbar);
      } else header.querySelector('nav')?.append(toolbar);
      const nav = toolbar.closest('nav');
      if (nav) nav.classList.add('sferix-responsive-nav');
      const brand = header.querySelector('.navbar-brand');
      if (brand) brand.classList.add('sferix-header-brand');
      if (searchHost) searchHost.classList.add('sferix-search-host');
    };
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', configure, { once: true });
    else configure();
  }
};
