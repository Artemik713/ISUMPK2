window.applyTheme = function (isDark, themeColor) {
    // Отладка для проверки входных параметров
    console.log(`applyTheme вызван с параметрами: isDark=${isDark} (тип: ${typeof isDark}), themeColor=${themeColor}`);

    // Убедимся, что isDark точно булево значение
    isDark = Boolean(isDark);
    console.log(`isDark после приведения к boolean: ${isDark}`);

    // Получаем корневые элементы для MudBlazor
    const html = document.documentElement;
    const body = document.body;

    // Удаляем предыдущие классы темы
    body.classList.remove('mud-theme-dark', 'mud-theme-light');
    html.classList.remove('dark-mode', 'light-mode');

    // Добавляем соответствующие классы MudBlazor
    if (isDark) {
        body.classList.add('mud-theme-dark');
        html.classList.add('dark-mode');

        // Дополнительные настройки для темной темы
        html.style.setProperty('--mud-palette-background', '#121212');
        html.style.setProperty('--mud-palette-drawer-background', '#1e1e1e');
        html.style.setProperty('--mud-palette-surface', '#242424');
        html.style.setProperty('--mud-palette-text-primary', 'rgba(255,255,255,0.87)');
        html.style.setProperty('--mud-palette-text-secondary', 'rgba(255,255,255,0.6)');
        html.style.setProperty('--mud-palette-action-default', 'rgba(255,255,255,0.6)');

        // Возможно, требуется изменить другие CSS-переменные MudBlazor для темной темы
        html.style.setProperty('--mud-palette-app-bar-background', '#272727');
        html.style.setProperty('--mud-palette-drawer-background-hover', '#2e2e2e');
    } else {
        body.classList.add('mud-theme-light');
        html.classList.add('light-mode');

        // Возвращаем светлые настройки
        html.style.removeProperty('--mud-palette-background');
        html.style.removeProperty('--mud-palette-drawer-background');
        html.style.removeProperty('--mud-palette-surface');
        html.style.removeProperty('--mud-palette-text-primary');
        html.style.removeProperty('--mud-palette-text-secondary');
        html.style.removeProperty('--mud-palette-action-default');
        html.style.removeProperty('--mud-palette-app-bar-background');
        html.style.removeProperty('--mud-palette-drawer-background-hover');
    }

    // Устанавливаем основной цвет
    if (themeColor) {
        // Получаем CSS-переменную для выбранного цвета
        const primaryColor = getComputedStyle(document.body).getPropertyValue(`--mud-palette-${themeColor}`);
        if (primaryColor) {
            html.style.setProperty('--mud-palette-primary', primaryColor);

            // Также обновляем акцентный цвет для лучшего контраста
            html.style.setProperty('--mud-palette-primary-text', isDark ? '#ffffff' : '#ffffff');
        }
    }

    // Сохраняем настройки в localStorage
    localStorage.setItem('theme', isDark ? 'Dark' : 'Light');
    localStorage.setItem('themeColor', themeColor || 'primary');

    console.log(`Тема применена: ${isDark ? 'темная' : 'светлая'}, цвет: ${themeColor}`);
}