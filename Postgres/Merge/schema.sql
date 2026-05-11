-- Clean slate
DROP SCHEMA IF EXISTS warehouse CASCADE;
CREATE SCHEMA warehouse;

-- Suppliers we buy from
CREATE TABLE warehouse.suppliers (
    id          SERIAL PRIMARY KEY,
    name        TEXT NOT NULL,
    country     TEXT NOT NULL
);

-- Master product catalog (the "source of truth" for product info)
CREATE TABLE warehouse.products (
    id              SERIAL PRIMARY KEY,
    sku             TEXT UNIQUE NOT NULL,
    name            TEXT NOT NULL,
    unit_cost       NUMERIC(10,2) NOT NULL,
    supplier_id     INT NOT NULL REFERENCES warehouse.suppliers(id)
);

-- Stock levels per warehouse location
CREATE TABLE warehouse.stock (
    id              SERIAL PRIMARY KEY,
    location_id     INT NOT NULL,
    product_id      INT NOT NULL REFERENCES warehouse.products(id),
    quantity        INT NOT NULL CHECK (quantity >= 0),
    last_cost       NUMERIC(10,2) NOT NULL,
    last_restocked  TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (location_id, product_id)
);

-- Audit trail of every restock event
CREATE TABLE warehouse.restock_log (
    id              SERIAL PRIMARY KEY,
    location_id     INT NOT NULL,
    product_id      INT NOT NULL,
    quantity_added  INT NOT NULL,
    logged_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed suppliers
INSERT INTO warehouse.suppliers (name, country) VALUES
    ('Acme Components', 'DE'),
    ('Iberian Goods',   'PT'),
    ('Northwind Co',    'UK');

-- Seed products
INSERT INTO warehouse.products (sku, name, unit_cost, supplier_id) VALUES
    ('SKU-001', 'M3 Hex Bolt',        0.05, 1),
    ('SKU-002', 'AAC Block 600x250',  2.40, 2),
    ('SKU-003', 'Cat6 Cable 1m',      0.80, 3),
    ('SKU-004', 'Drainage Pipe 2m',   8.50, 2),
    ('SKU-005', 'Thermal Paste 4g',   3.20, 1);

-- Seed existing stock at location 1 (some products already stocked, others not)
INSERT INTO warehouse.stock (location_id, product_id, quantity, last_cost) VALUES
    (1, 1, 500,  0.04),   -- already have bolts
    (1, 3, 120,  0.75),   -- already have cables
    (1, 5, 10,   3.10);   -- already have thermal paste
-- Note: products 2 and 4 (AAC blocks, drainage pipe) have NO stock row yet at location 1