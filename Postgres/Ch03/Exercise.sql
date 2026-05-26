CREATE SCHEMA mm;
Set search_path to mm;

DROP TABLE IF EXISTS order_items, orders, products, customers CASCADE;

CREATE TABLE customers (
    customer_id   SERIAL PRIMARY KEY,
    name          TEXT NOT NULL,
    signup_date   DATE NOT NULL
);

CREATE TABLE products (
    product_id    SERIAL PRIMARY KEY,
    name          TEXT NOT NULL,
    category      TEXT NOT NULL,
    unit_price    NUMERIC(10,2) NOT NULL
);

CREATE TABLE orders (
    order_id      SERIAL PRIMARY KEY,
    customer_id   INT NOT NULL REFERENCES customers(customer_id),
    order_date    DATE NOT NULL
);

CREATE TABLE order_items (
    order_item_id SERIAL PRIMARY KEY,
    order_id      INT NOT NULL REFERENCES orders(order_id),
    product_id    INT NOT NULL REFERENCES products(product_id),
    quantity      INT NOT NULL
);

INSERT INTO customers (name, signup_date) VALUES
('Alice Chen',     '2022-03-10'),
('Bob Martinez',   '2023-01-15'),
('Carla Reyes',    '2021-07-22'),
('David Okafor',   '2023-11-01'),
('Elena Popov',    '2022-09-30'),
('Frank Liu',      '2024-02-14');

INSERT INTO products (name, category, unit_price) VALUES
('Wireless Mouse',     'Electronics',  25.00),
('Mechanical Keyboard','Electronics', 120.00),
('USB-C Hub',          'Electronics',  45.00),
('Office Chair',       'Furniture',   210.00),
('Standing Desk',      'Furniture',   430.00),
('Desk Lamp',          'Furniture',    35.00),
('Notebook Pack',      'Stationery',   12.00),
('Gel Pen Set',        'Stationery',    8.00);

INSERT INTO orders (customer_id, order_date) VALUES
(1, '2024-01-05'), (1, '2024-04-18'), (1, '2024-09-02'),
(2, '2024-02-20'), (2, '2024-06-11'),
(3, '2024-03-15'), (3, '2024-03-16'), (3, '2024-08-30'), (3, '2024-12-01'),
(4, '2024-05-22'),
(5, '2024-01-30'), (5, '2024-07-19'),
(6, '2023-12-28'),  -- note: not in 2024
(1, '2023-11-11');  -- note: not in 2024

INSERT INTO order_items (order_id, product_id, quantity) VALUES
-- Alice's 2024 orders
(1, 2, 1), (1, 1, 2),          -- keyboard + 2 mice
(2, 5, 1),                      -- standing desk
(3, 3, 3), (3, 1, 1),          -- 3 hubs + mouse
-- Bob's 2024 orders
(4, 7, 5), (4, 8, 4),          -- notebooks + pens
(5, 6, 2),                      -- 2 desk lamps
-- Carla's 2024 orders
(6, 5, 1), (6, 4, 2),          -- desk + 2 chairs
(7, 2, 2),                      -- 2 keyboards
(8, 4, 1),                      -- chair
(9, 3, 4),                      -- 4 hubs
-- David's 2024 order
(10, 7, 3),                     -- notebooks
-- Elena's 2024 orders
(11, 1, 1), (11, 2, 1),        -- mouse + keyboard
(12, 6, 1),                     -- desk lamp
-- Frank's 2023 order (should be excluded)
(13, 5, 1),
-- Alice's 2023 order (should be excluded)
(14, 2, 1);


---------------------------


WITH line_items_2024 AS (
    SELECT
        o.customer_id,
        c.name AS customer_name,
        p.category,
        p.unit_price * oi.quantity AS line_total
    FROM orders o
    JOIN order_items oi ON oi.order_id = o.order_id
    JOIN customers c   ON c.customer_id = o.customer_id
    JOIN products p    ON p.product_id = oi.product_id
    WHERE o.order_date >= '2024-01-01'
      AND o.order_date <  '2025-01-01'
),
customer_totals AS (
    SELECT customer_id, customer_name, SUM(line_total) AS total_spent_2024
    FROM line_items_2024
    GROUP BY customer_id, customer_name
),
customer_category_totals AS (
    SELECT
        customer_id,
        category,
        SUM(line_total) AS customer_category_spend,
        ROW_NUMBER() OVER (PARTITION BY customer_id ORDER BY SUM(line_total) DESC) AS rn
    FROM line_items_2024
    GROUP BY customer_id, category
),
category_totals AS (
    SELECT category, SUM(line_total) AS category_total_revenue
    FROM line_items_2024
    GROUP BY category
),
threshold AS (
    SELECT AVG(total_spent_2024) AS avg_spend FROM customer_totals
)
SELECT
    ct.customer_name,
    ct.total_spent_2024,
    cct.category AS top_category,
    cct.customer_category_spend,
    catt.category_total_revenue,
    ROUND(cct.customer_category_spend * 100.0 / catt.category_total_revenue, 1) AS customer_share_pct
FROM customer_totals ct
JOIN customer_category_totals cct ON cct.customer_id = ct.customer_id AND cct.rn = 1
JOIN category_totals catt         ON catt.category = cct.category
CROSS JOIN threshold th
WHERE ct.total_spent_2024 > th.avg_spend
ORDER BY ct.total_spent_2024 DESC;