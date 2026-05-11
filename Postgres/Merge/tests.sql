\echo '=== Setting up schema ==='
\ir schema.sql
\ir function.sql

\echo '--- Test 1'

BEGIN;

SELECT warehouse.restock(1, 'SKU-001', 200);

DO $$
DECLARE
    actual_qty INT;
BEGIN
    SELECT quantity INTO actual_qty
    FROM warehouse.stock
    WHERE location_id = 1 AND product_id = 1;

    IF actual_qty <> 700 THEN
        RAISE EXCEPTION 'Test 1 failed: expected quantity 700, got %', actual_qty;
    END IF;

    RAISE NOTICE 'Test 1 passed: bolts quantity is now %', actual_qty;
END $$;

ROLLBACK;

\echo '--- Test 2'

BEGIN;

SELECT warehouse.restock(1, 'SKU-002', 50);

DO $$
DECLARE
    actual_qty INT;
    actual_cost NUMERIC(10,2);
BEGIN
    SELECT quantity, last_cost INTO actual_qty, actual_cost
    FROM warehouse.stock
    WHERE location_id = 1 AND product_id = 2;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Test 2 failed: no stock row was created for product 2 at location 1';
    END IF;

    IF actual_qty <> 50 THEN
        RAISE EXCEPTION 'Test 2 failed: expected quantity 50, got %', actual_qty;
    END IF;

    IF actual_cost <> 2.40 THEN
        RAISE EXCEPTION 'Test 2 failed: expected last_cost 2.40, got %', actual_cost;
    END IF;

    RAISE NOTICE 'Test 2 passed: new stock row created with qty=% cost=%', actual_qty, actual_cost;
END $$;

ROLLBACK;


\echo '--- Test 3'

BEGIN;

SELECT warehouse.restock(2, 'SKU-003', 80);

DO $$
DECLARE
    actual_qty INT;
    loc1_qty INT;
BEGIN
    -- New row at location 2 should exist with qty=80
    SELECT quantity INTO actual_qty
    FROM warehouse.stock
    WHERE location_id = 2 AND product_id = 3;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Test 3 failed: no stock row was created at location 2 for product 3';
    END IF;

    IF actual_qty <> 80 THEN
        RAISE EXCEPTION 'Test 3 failed: expected quantity 80 at loc 2, got %', actual_qty;
    END IF;

    -- Sanity check: location 1 stock for cables should be untouched (still 120)
    SELECT quantity INTO loc1_qty
    FROM warehouse.stock
    WHERE location_id = 1 AND product_id = 3;

    IF loc1_qty <> 120 THEN
        RAISE EXCEPTION 'Test 3 failed: location 1 cables quantity changed unexpectedly to %', loc1_qty;
    END IF;

    RAISE NOTICE 'Test 3 passed: loc 2 has qty=%, loc 1 untouched at %', actual_qty, loc1_qty;
END $$;

ROLLBACK;


\echo '--- Test 4'

BEGIN;

UPDATE warehouse.products SET unit_cost = 0.06 WHERE sku = 'SKU-001';
SELECT warehouse.restock(1, 'SKU-001', 100);

DO $$
DECLARE
    actual_qty  INT;
    actual_cost NUMERIC(10,2);
BEGIN
    SELECT quantity, last_cost INTO actual_qty, actual_cost
    FROM warehouse.stock
    WHERE location_id = 1 AND product_id = 1;

    IF actual_qty <> 600 THEN
        RAISE EXCEPTION 'Test 4 failed: expected quantity 600, got %', actual_qty;
    END IF;

    IF actual_cost <> 0.06 THEN
        RAISE EXCEPTION 'Test 4 failed: expected last_cost 0.06, got %', actual_cost;
    END IF;

    RAISE NOTICE 'Test 4 passed: quantity=% last_cost=%', actual_qty, actual_cost;
END $$;

ROLLBACK;