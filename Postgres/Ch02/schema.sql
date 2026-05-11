CREATE SCHEMA products;
CREATE SCHEMA customers;
CREATE SCHEMA sales;

CREATE TABLE products.catalog (
    id SERIAL PRIMARY KEY,
    name VARCHAR (100) NOT NULL,
    description TEXT NOT NULL,
    category TEXT CHECK (category IN ('coffee', 'mug', 't-shirt')),
    price NUMERIC(10, 2),
    stock_quantity INT CHECK (stock_quantity >= 0)
);

CREATE TABLE products.reviews (  
    id BIGSERIAL PRIMARY KEY,  
    product_id INT,
    customer_id INT,
    review TEXT,
    rank SMALLINT 
);

INSERT INTO products.catalog (name, description, category, price, stock_quantity)
VALUES
    ('Sunrise Blend', 'A smooth and balanced blend with notes of caramel and citrus.', 'coffee', 14.99, 50),
    ('Midnight Roast', 'A dark roast with rich flavors of chocolate and toasted nuts.', 'coffee', 16.99, 40),
    ('Morning Glory', 'A light roast with bright acidity and floral notes.', 'coffee', 13.99, 30),
    ('Sunrise Brew Co. Mug', 'A ceramic mug with the Sunrise Brew Co. logo.', 'mug', 9.99, 100),
    ('Sunrise Brew Co. T-Shirt', 'A soft cotton t-shirt with the Sunrise Brew Co. logo.', 't-shirt', 19.99, 25);

ALTER TABLE products.catalog 
ADD CONSTRAINT catalog_price_check CHECK (price > 0);

ALTER TABLE products.reviews 
    ALTER COLUMN review SET NOT NULL,
    ADD CONSTRAINT review_rank_check CHECK (rank BETWEEN 1 AND 5);

ALTER TABLE products.reviews
    ADD CONSTRAINT products_review_product_id_fk
    FOREIGN KEY (product_id) REFERENCES products.catalog(id);

CREATE TABLE customers.accounts (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT NOT NULL,
    passwd_hash TEXT NOT NULL
);

ALTER TABLE products.reviews 
    ADD CONSTRAINT products_review_customer_id_fk
    FOREIGN KEY (customer_id) REFERENCES customers.accounts(id);

INSERT INTO customers.accounts (name, email, passwd_hash)
VALUES
    ('Alice Johnson', 'alice.johnson@example.com', '5f4dcc3b5aa765d61d8327deb882cf99'),
    ('Bob Smith', 'bob.smith@example.com', 'd8578edf8458ce06fbc5bb76a58c5ca4'), 
    ('Charlie Brown', 'charlie.brown@example.com', '5f4dcc3b5aa765d61d8327deb882cf99');

INSERT INTO products.reviews (product_id, customer_id, review, rank)
VALUES (4, 1, 'This mug is perfect — sturdy, stylish, and keeps my coffee warm for a good while.', 5);

ALTER TABLE customers.accounts 
    ADD COLUMN deleted boolean DEFAULT false;

UPDATE products.catalog SET stock_quantity = stock_quantity + 100 
WHERE id = 1;

UPDATE products.catalog SET stock_quantity = stock_quantity + 50 
WHERE id = 1 or id = 3;

CREATE TABLE sales.orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),  
    customer_id INT REFERENCES customers.accounts(id),
    order_date TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    total_amount DECIMAL(10, 2)
);

CREATE TABLE sales.order_items (
    order_id UUID REFERENCES sales.orders(id),
    product_id INT REFERENCES products.catalog(id),
    quantity INT CHECK (quantity > 0),
    price DECIMAL (10, 2), 
    PRIMARY KEY (order_id, product_id)  
);

BEGIN; 

INSERT INTO sales.orders (id, customer_id, total_amount)  
VALUES ('19a0cffc-8757-453c-a4d2-b554fdc08954', 1, 26.53);

INSERT INTO sales.order_items (order_id, product_id, quantity, price)  
VALUES ('19a0cffc-8757-453c-a4d2-b554fdc08954', 1, 1, 16.54),
 ('19a0cffc-8757-453c-a4d2-b554fdc08954', 4, 1, 9.99);

UPDATE products.catalog  
SET stock_quantity = stock_quantity - 1
WHERE id IN (1, 4);

COMMIT;

BEGIN;
    
SELECT stock_quantity FROM products.catalog
WHERE id = 1;
    
UPDATE products.catalog
SET stock_quantity = stock_quantity - 1
WHERE id = 1;
    
COMMIT;

SELECT c.name, c.id, count(*) as total_orders
FROM customers.accounts c
JOIN sales.orders s ON c.id = s.customer_id
GROUP BY c.id
ORDER BY total_orders DESC
LIMIT 3;

SELECT c.name 
FROM customers.accounts c 
LEFT JOIN sales.orders s ON c.id = s.customer_id 
WHERE s.customer_id IS NULL;

SELECT c.name, c.category, c.price, SUM(oi.quantity) AS total_sold
FROM products.catalog c
LEFT JOIN sales.order_items oi ON c.id = oi.product_id
GROUP BY c.id
ORDER BY total_sold DESC NULLS LAST, price DESC;

CREATE OR REPLACE FUNCTION products.get_product_price(product_id INT)
RETURNS NUMERIC(10, 2) AS $$
    SELECT price
    FROM products.catalog
    WHERE id = product_id;
$$ LANGUAGE sql;

ALTER TABLE sales.orders 
ADD COLUMN status TEXT DEFAULT 'pending' CHECK (status in ('pending','ordered'));

UPDATE sales.orders SET status = 'ordered';  

ALTER TABLE sales.orders  
ADD CONSTRAINT one_pending_order_per_customer
EXCLUDE USING btree (customer_id WITH =)
WHERE (status = 'pending');