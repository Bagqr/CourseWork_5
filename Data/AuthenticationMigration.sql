-- Data/AuthenticationMigration.sql
-- Таблица пользователей системы
CREATE TABLE IF NOT EXISTS system_users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL,
    employee_id INTEGER,
    is_active BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_login DATETIME,
    must_change_password BOOLEAN DEFAULT 0,
    FOREIGN KEY(employee_id) REFERENCES сотрудник(id)
);

-- Таблица пунктов меню
CREATE TABLE IF NOT EXISTS menu_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    code TEXT UNIQUE NOT NULL,  -- Уникальный код пункта меню
    parent_id INTEGER,
    display_order INTEGER DEFAULT 0,
    FOREIGN KEY(parent_id) REFERENCES menu_items(id)
);

-- Таблица прав пользователей (R W E D)
CREATE TABLE IF NOT EXISTS user_permissions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    menu_item_id INTEGER NOT NULL,
    can_read BOOLEAN DEFAULT 0,
    can_write BOOLEAN DEFAULT 0,
    can_edit BOOLEAN DEFAULT 0,
    can_delete BOOLEAN DEFAULT 0,
    UNIQUE(user_id, menu_item_id),
    FOREIGN KEY(user_id) REFERENCES system_users(id),
    FOREIGN KEY(menu_item_id) REFERENCES menu_items(id)
);

-- Таблица сессий (для отслеживания активности)
CREATE TABLE IF NOT EXISTS user_sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    session_token TEXT UNIQUE NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NOT NULL,
    last_activity DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(user_id) REFERENCES system_users(id)
);