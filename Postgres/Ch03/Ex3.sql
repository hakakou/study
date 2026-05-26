-- Drop and recreate the schema for isolation
DROP SCHEMA IF EXISTS bookhaven CASCADE;
CREATE SCHEMA bookhaven;

-- Books inventory
CREATE TABLE bookhaven.books (
    id SERIAL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    author VARCHAR(100) NOT NULL,
    price NUMERIC(8, 2) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0,
    reorder_threshold INT NOT NULL DEFAULT 5
);

-- Customers
CREATE TABLE bookhaven.customers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    loyalty_points INT NOT NULL DEFAULT 0,
    tier VARCHAR(20) NOT NULL DEFAULT 'Standard'  -- Standard, Silver, Gold
);

-- Orders
CREATE TABLE bookhaven.orders (
    id SERIAL PRIMARY KEY,
    customer_id INT NOT NULL REFERENCES bookhaven.customers(id),
    order_date TIMESTAMP NOT NULL DEFAULT NOW(),
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Pending, Shipped, Cancelled
    total_amount NUMERIC(10, 2) NOT NULL DEFAULT 0
);

-- Order items
CREATE TABLE bookhaven.order_items (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL REFERENCES bookhaven.orders(id),
    book_id INT NOT NULL REFERENCES bookhaven.books(id),
    quantity INT NOT NULL,
    unit_price NUMERIC(8, 2) NOT NULL
);

-- Audit log for restocking events
CREATE TABLE bookhaven.restock_log (
    id SERIAL PRIMARY KEY,
    book_id INT NOT NULL REFERENCES bookhaven.books(id),
    quantity_added INT NOT NULL,
    logged_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Sample data
INSERT INTO bookhaven.books (title, author, price, stock_quantity, reorder_threshold) VALUES
    ('The Pragmatic Programmer', 'Andy Hunt', 39.99, 3, 5),
    ('Clean Code', 'Robert Martin', 34.99, 15, 5),
    ('Designing Data-Intensive Applications', 'Martin Kleppmann', 49.99, 2, 5),
    ('The Mythical Man-Month', 'Fred Brooks', 29.99, 8, 5),
    ('Refactoring', 'Martin Fowler', 44.99, 1, 5),
    ('Domain-Driven Design', 'Eric Evans', 54.99, 20, 5);

INSERT INTO bookhaven.customers (name, email, loyalty_points, tier) VALUES
    ('Alice Chen', 'alice@example.com', 450, 'Standard'),
    ('Bob Martinez', 'bob@example.com', 1200, 'Silver'),
    ('Carol Singh', 'carol@example.com', 2800, 'Gold'),
    ('David Lee', 'david@example.com', 90, 'Standard');

INSERT INTO bookhaven.orders (customer_id, status, total_amount) VALUES
    (1, 'Pending', 74.98),
    (2, 'Pending', 49.99),
    (3, 'Pending', 124.97),
    (4, 'Pending', 34.99),
    (1, 'Pending', 44.99);

INSERT INTO bookhaven.order_items (order_id, book_id, quantity, unit_price) VALUES
    (1, 1, 1, 39.99),
    (1, 4, 1, 29.99),
    (2, 3, 1, 49.99),
    (3, 2, 1, 34.99),
    (3, 6, 1, 54.99),
    (3, 4, 1, 29.99),
    (4, 2, 1, 34.99),
    (5, 5, 1, 44.99);